using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.ComponentModel;

using Agitech;
using Agitech.Native;
using Agitech.Wpf;
using Agitech.Extensions;

namespace Clicker
{

	/// <summary>
	/// Interaction logic for KeyPanel.xaml
	/// </summary>
	public partial class KeyControl : UserControl , INotifyPropertyChanged
	{

		#region Constructor & Properties

		public KeyControl(KeyStroke ksi)
		{

			KeyStroke = ksi;

			InitializeComponent();

			Binding binding = new Binding();
			binding.Source = this;
			binding.Path = new PropertyPath("KeyStroke");
			binding.Converter = new KeyStrokeConverter();
			binding.Mode = BindingMode.TwoWay;
			txtKey.SetBinding(TextBox.TextProperty, binding);

			binding = new Binding();
			binding.Source = this.KeyStroke;
			binding.Path = new PropertyPath("Delay");
			binding.Mode = BindingMode.TwoWay;
			txtTimeout.SetBinding(TextBox.TextProperty, binding);


			binding = new Binding();
			binding.Source = this.KeyStroke;
			binding.Path = new PropertyPath("PressTime");
			binding.Mode = BindingMode.TwoWay;
			txtPressTime.SetBinding(TextBox.TextProperty, binding);
		}

		private KeyStroke _keyStroke;

		public KeyStroke KeyStroke
		{
			get
			{
				return _keyStroke;
			}
			set
			{
				if (KeyStroke != null)
				{
					KeyStroke.KeyPressedEvent -= KeyPressedEventHandler;
				}
				_keyStroke = value;
				_keyStroke.KeyPressedEvent += KeyPressedEventHandler;

			}
		}



		#endregion

		#region PropertyChanges

		[field: NonSerializedAttribute()]
		public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.
		// The CallerMemberName attribute that is applied to the optional propertyName
		// parameter causes the property name of the caller to be substituted as an argument.
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Methods

		private void ProcessKey(KeyEventArgs e)
		{
			if (e == null)
				return;

			if (e.Key == Key.Scroll || e.Key == Key.CapsLock || e.Key == Key.NumLock)
			{
				return;
			}
			else if (e.Key == Key.Escape)
			{
				KeyStroke.Clear();
				NotifyPropertyChanged("KeyStroke");
			}
			else if (!KeyStroke.IsComplete())
			{
				KeyStroke.Add(e.Key);
				NotifyPropertyChanged("KeyStroke");
			}
			e.Handled = true;
		}

		public void KeyPressedEventHandler(object sender, KeyPressedEventArgs e)
		{
			this.Dispatcher.Invoke(new System.Action(() =>
			{
				ListBox parent = this.FindParent<ListBox>();
				if (parent.Items.Contains(this))
					parent.SelectedItem = this;
				this.OnMouseLeftButtonDown(null);
			}
			));
		}

		#endregion

		private void txtKey_KeyDown(object sender, KeyEventArgs e)
		{
			if (!e.IsRepeat)
				ProcessKey(e);
		}

	}


	[ValueConversion(typeof(KeyStroke), typeof(String))]
	public class KeyStrokeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value == null ? String.Empty : ((KeyStroke)value).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException("The method or operation is not implemented.");
		}
	}
}
