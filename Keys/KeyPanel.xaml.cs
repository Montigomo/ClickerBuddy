using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using Agitech;
using Agitech.Native;
using Agitech.Extensions;

namespace Clicker.Keys
{
	/// <summary>
	/// Interaction logic for KeyPanel.xaml
	/// </summary>
	public partial class KeyPanel : UserControl
	{

		public KeyPanel()
		{
			InitializeComponent();

			Keys = new KeyStrokeCollection();
			Keys.Add(new KeyStroke());
		}

		private void SetBindings()
		{
			Binding binding = new Binding();
			binding.Source = this.Keys;
			binding.Path = new PropertyPath("Window");
			binding.Converter = new WindowTextConverter();
			binding.Mode = BindingMode.TwoWay;
			lblAppName.SetBinding(Label.ContentProperty, binding);
		}

		private enum Action
		{
			Toggle,
			Stop,
			Start
		}

		private KeyStrokeCollection _keys;

		public KeyStrokeCollection Keys
		{
			get
			{
				return _keys;
			}
			set
			{
				if (_keys != null && _keys.Count > 0)
				{
					KeysChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _keys));
					_keys.CollectionChanged -= KeysChanged;
					_keys.StoppedEvent -= KsiStopped;
					_keys.Stop();
				}
				_keys = value;
				_keys.CollectionChanged += KeysChanged;
				_keys.StoppedEvent += KsiStopped;
				KeysChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _keys));
				SetBindings();
			}
		}

		private Window ParentWindow
		{
			get
			{
				return this.FindParent<Window>();
			}
		}

		private void SelectApp()
		{
			Tasks wndSelector = new Tasks();
			wndSelector.Owner = ParentWindow;
			wndSelector.ShowDialog();
			if (wndSelector.SelectedWindow != null)
			{
				Keys.Window = wndSelector.SelectedWindow;
			}
		}

		private void KeysChanged(Object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (KeyStroke ksi in e.NewItems)
					{
						KeyControl kctrl = new KeyControl(ksi);
						lstKeys.Items.Add(kctrl);
						ksi.KeyPressedEvent += kctrl.KeyPressedEventHandler;
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (KeyStroke ksi in e.OldItems)
					{
						for (int i = lstKeys.Items.Count - 1; i >= 0; i--)
						{
							if (((KeyControl)lstKeys.Items[i]).KeyStroke == ksi)
								lstKeys.Items.Remove(lstKeys.Items[i]);
						}
					}
					break;
				default:
					break;
			}
		}


		private Nullable<bool> LoadSave(KslMode mode)
		{
			KeyStrokeLoader KeyStrokeLoaderDlg = new KeyStrokeLoader();
			KeyStrokeLoaderDlg.Owner = ParentWindow;
			KeyStrokeLoaderDlg.KslMode = mode;
			KeyStrokeLoaderDlg.Ksl = this.Keys;
			Nullable<bool> result = KeyStrokeLoaderDlg.ShowDialog();
			if (result == true && mode == KslMode.Load)
			{
				Keys = KeyStrokeLoaderDlg.Ksl;
			}
			return result;
		}

		private void KsiStopped(object sender, KeyStrokeCollectionStopped e)
		{
			Trace.Write(e.Message);
			StartStop(Action.Stop);
		}

		private void StartStop(Action action)
		{
			if (Keys.Window == null)
			{
				
				return;
			}
			if(!Dispatcher.CheckAccess())
			{
				Dispatcher.InvokeAsync(new System.Action(() => StartStop(action)));
				return;
			}
			switch (action)
			{
				case Action.Start:
					Keys.Start();
					btnStart.Content = "[Stop]";
					break;
				case Action.Stop:
					Keys.Stop();
					btnStart.Content = "[Start]";
					break;
				case Action.Toggle:
				default:
					if (Keys.Mode == KsiOperation.Work)
					{
						Keys.Stop();
						btnStart.Content = "[Start]";
					}
					else if (Keys.Mode == KsiOperation.Stopped)
					{
						Keys.Start();
						btnStart.Content = "[Stop]";
					}
					break;
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			LoadSave(KslMode.Save);
		}

		private void btnLoad_Click(object sender, RoutedEventArgs e)
		{
			LoadSave(KslMode.Load);
		}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			StartStop(Action.Toggle);
		}

		private void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			Keys.Add(new KeyStroke());
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (Keys.Count > 1)
				Keys.RemoveLast();
		}

		private void lblAppName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			SelectApp();
		}
	}


	[ValueConversion(typeof(AgtWindow), typeof(String))]
	public class WindowTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value == null ? "Double click to select application" : ((AgtWindow)value).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException("The method or operation is not implemented.");
		}
	}
}
