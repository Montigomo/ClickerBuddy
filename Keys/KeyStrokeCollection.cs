using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Amib.Threading;
using Agitech;
using Agitech.Extensions;


namespace ClickerBuddy.Keys
{
	[Serializable()]
	public class KeyStrokeCollection : ObservableCollection<KeyStroke>
	{

		#region Constructor

		public KeyStrokeCollection()
			: base()
		{
			//_serializableDelegates = new List<Delegate>();
		}

		static KeyStrokeCollection()
		{
			Application.Current.Exit += App_Exit;
		}

		private static void App_Exit(object sender, ExitEventArgs e)
		{
			_stp.Shutdown();
		}

		#endregion

		#region PropertyChanges

		//[field: NonSerializedAttribute()]
		//public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.
		// The CallerMemberName attribute that is applied to the optional propertyName
		// parameter causes the property name of the caller to be substituted as an argument.
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
			//if (this.PropertyChanged != null)
			//{
			//	this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			//}
		}

		#endregion

		#region Serialize

		//private readonly List<Delegate> _serializableDelegates;

		[OnDeserialized()]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			_mode = KsiOperation.Stopped;
			
			if(!_windowName.IsNullOrEmpty())
				Window = AgtWindow.FindWindow(_windowName);
			
			//WiG.QueueWorkItem(new WorkItemCallback(OnDeserializedDelegate), this);

			Application.Current.Exit += App_Exit;

			//foreach (var invocation in _serializableDelegates)
			//{
			//	PropertyChanged += (PropertyChangedEventHandler)invocation;
			//}
		}

		//private object OnDeserializedDelegate(object state)
		//{
		//	return null;
		//}

		//[OnSerializing]
		//public void OnSerializing(StreamingContext context)
		//{
		//	_serializableDelegates.Clear();
		//	var handler = PropertyChanged;

		//	if (handler != null)
		//	{
		//		foreach (var invocation in handler.GetInvocationList())
		//		{
		//			if (invocation.Target != null && invocation.Target.GetType().IsSerializable)
		//			{
		//				_serializableDelegates.Add(invocation);
		//			}
		//		}
		//	}
		//}

		//[Serializable]
		//private class SerializableHandler
		//{
		//	public void PropertyChanged(object sender, PropertyChangedEventArgs e)
		//	{
		//		Console.WriteLine("  Serializable handler called");
		//	}
		//}

		#endregion

		#region Properties

		private object _lock = new object();

		private KsiOperation _mode;

		public KsiOperation Mode
		{
			get { return _mode; }
			private set { _mode = value; }
		}

		private string _windowName;

		[field: NonSerializedAttribute()]
		private AgtWindow _agtWindow;

		public AgtWindow Window
		{
			get
			{
				return _agtWindow;
			}
			set
			{
				_agtWindow = value;
				_windowName = _agtWindow.Text;
				NotifyPropertyChanged();
			}
		}

		private int _maxKeys = 5;

		public int MaxKeys
		{
			get
			{
				return _maxKeys;
			}
			set
			{
				_maxKeys = value;
			}
		}


		[field: NonSerializedAttribute()]
		private static SmartThreadPool _stp = new SmartThreadPool(
			new STPStartInfo
			{
				StartSuspended = false,
				MaxWorkerThreads = 5,
				IdleTimeout = 30 * 1000,
			}
		);

		[field: NonSerializedAttribute()]
		private IWorkItemsGroup _wig;

		public IWorkItemsGroup WiG
		{
			get
			{
				if (_wig == null)
					CreateWiG();
				return _wig;
			}
			set
			{
				_wig = value;
			}
		}


		private void CreateWiG()
		{
			WIGStartInfo wsi = new WIGStartInfo
			{
				FillStateWithArgs = true
			};
			_wig = _stp.CreateWorkItemsGroup(5, wsi);
		}

		public void Start()
		{
			if (Mode == KsiOperation.Work)
				return;
			lock (_lock)
			{
				Mode = KsiOperation.Work;
				WiG.QueueWorkItem(new WorkItemCallback(Worker), this, WorkItemStoped);
				Trace.Write("WorkItem started;");
			}
		}

		public void Stop()
		{
			if (Mode == KsiOperation.Stopped)
				return;
			lock (_lock)
			{
				Mode = KsiOperation.Stopped;
			}
		}

		private void WorkItemStoped(IWorkItemResult wir)
		{
			StringBuilder msg = new StringBuilder();

			msg.AppendFormat("WorkItem stopped;{0}", Environment.NewLine);

			try
			{
				object result = wir.Result;
			}
			// Catch the exception that Result threw
			catch (WorkItemResultException e)
			{
				// Dump the inner exception which DoDiv threw
				msg.AppendFormat("{0};{1}", e.InnerException.Message, Environment.NewLine);
			}

			OnRaiseStoppedEvent(new KeyStrokeCollectionStopped(msg.ToString()));

		}

		#endregion

		#region Methods

		public void Add(KeyStroke ksi, EventHandler<KeyPressedEventArgs> handler)
		{
			if (Items.Count < MaxKeys)
			{
				base.Add(ksi);
				if (handler != null)
					ksi.KeyPressedEvent += handler;
			}
		}

		public new void Add(KeyStroke ksi)
		{
			Add(ksi, null);
		}

		public void RemoveLast()
		{
			if (Items.Count > 0)
				RemoveAt(Count - 1);
		}


		private object Worker(object state)
		{
			KeyStrokeCollection ksc = state as KeyStrokeCollection;
			if (ksc == null)
				return null;

			while (ksc.Mode == KsiOperation.Work)
			{
				foreach (KeyStroke ksi in ksc.Items)
				{
					Stopwatch swKey = new Stopwatch();
					swKey.Start();
					Thread.MemoryBarrier();
					Trace.Write(Window.Text + " - " + ksi + " - " + ksi.Delay);
					ksi.OnRaiseKeyPressedEvent(new KeyPressedEventArgs());
					while (ksc.Mode == KsiOperation.Work)
					{
						Window.PressKey(ksi);
						if (swKey.Elapsed.TotalMilliseconds >= ksi.PressTime)
							break;
					}
					swKey.Stop();
					swKey = null;

					Thread.Sleep(ksi.Delay);
				}
			}
			return null;
		}

		#endregion

		#region Events

		[field: NonSerializedAttribute()]
		public event EventHandler<KeyStrokeCollectionStopped> StoppedEvent;

		public void OnRaiseStoppedEvent(KeyStrokeCollectionStopped e)
		{
			if (StoppedEvent != null)
			{
				StoppedEvent(null, e);
			}
		}

		#endregion

	}







	public class KeyStrokeCollectionStopped : EventArgs
	{

		public KeyStrokeCollectionStopped(string msg)
		{
			Message = msg;
		}

		public string Message;
	}



	public enum KsiOperation
	{
		Stopped,
		Work
	}
}
