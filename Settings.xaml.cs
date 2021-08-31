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
using System.Windows.Shapes;
using System.ComponentModel;

using Xceed.Wpf.Toolkit;


namespace ClickerBuddy
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : Window
	{
		public Settings()
		{
			InitializeComponent();
			propSettings.SelectedObject = Config.Instance;
		}

		private void wndSettings_Loaded(object sender, RoutedEventArgs e)
		{
			if (Owner != null)
			{
				Left = (Owner.Left + (Owner.Width / 2)) - Width / 2;
				Left = Left > 0 ? Left : 30;
				Top = (Owner.Top);
			}
		}
	}

}
