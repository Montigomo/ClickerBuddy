using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using Agitech;

namespace Clicker
{
	[Serializable()]
	public class KeyStroke : AgtKeyStroke
	{
		public KeyStroke()
		{
			Delay = 1000;
			PressTime = 0;
		}

		private int _delay;

		public int Delay
		{
			get
			{
				return _delay;
			}

			set
			{
				_delay = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Delay"));
			}
		}

		private int _pressTime;

		/// <summary>
		/// Key holding time in ms
		/// </summary>
		public int PressTime
		{
			get
			{
				return _pressTime;
			}

			set
			{
				_pressTime = value;
				OnPropertyChanged(new PropertyChangedEventArgs("PressTime"));
			}
		}


		[field: NonSerializedAttribute()]
		public event EventHandler<KeyPressedEventArgs> KeyPressedEvent;

		public void OnRaiseKeyPressedEvent(KeyPressedEventArgs e)
		{
			if (KeyPressedEvent != null)
			{
				KeyPressedEvent(null, e);
			}
		}


	}
}
