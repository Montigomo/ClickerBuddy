using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using Agitech;
using Agitech.Wpf;

namespace Clicker
{
	/// <summary>
	/// Interaction logic for Tasks.xaml
	/// </summary>
	public partial class Tasks : Window
	{

		#region Constructors & Properties

		public Tasks()
		{
			InitializeComponent();

			CommandBinding customCommandBinding = new CommandBinding(CmdActions, CmdRefreshExecuted, CmdRefreshCanExecute);

			// attach CommandBinding to root window
			this.CommandBindings.Add(customCommandBinding);

			_bWorker.DoWork += GetWindows;
		}

		public AgtWindowsCollection DesktopWindows = AgtWindowsCollection.DesktopWindows;

		private BackgroundWorker _bWorker = new BackgroundWorker();

		public AgtWindow SelectedWindow = null;

		#endregion

		#region Commands

		public static RoutedCommand CmdActions = new RoutedCommand();

		private void CmdRefreshExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			string parameter = e.Parameter != null ? e.Parameter.ToString() : "default";
			switch (parameter)
			{
				case "Select":
					CmdSelect();
					break;
				case "Cancel":
					CmdCancel();
					break;
				case "Refresh":
					CmdRefresh();
					break;
				default:
					break;
			}
		}

		private void CmdRefreshCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CmdRefresh()
		{
			_bWorker.RunWorkerAsync();
		}

		private void CmdSelect()
		{
			TreeListView tlv = trvMain;

			if (tlv.SelectedItem != null)
			{
				SelectedWindow = (AgtWindow)tlv.SelectedItem;
				this.Close();
			}
		}

		private void CmdCancel()
		{
			SelectedWindow = null;
			this.Close();
		}

		#endregion

		private void GetWindows(object sender, DoWorkEventArgs e)
		{
			AgtWindowsCollection.DesktopWindowsRefresh();
			Dispatcher.Invoke(new Action(SetItemsSource));
		}

		private void SetItemsSource()
		{
			trvMain.ItemsSource = AgtWindowsCollection.DesktopWindows;
		}

		private void WndTasks_Loaded(object sender, RoutedEventArgs e)
		{
			if (this != null)
				_bWorker.RunWorkerAsync();
			if (Owner != null)
			{
				Left = (Owner.Left + (Owner.Width / 2)) - Width / 2;
				Left = Left > 0 ? Left : 30;
				Top = (Owner.Top);
			}
		}

		private void WndTasks_Closed(object sender, EventArgs e)
		{

		}

		private void CommandBinding_Executed_Refresh(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnSelect_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{

		}

	}

	public class ItemClass
	{
		public string Name { get; set; }
		public bool IsAbstract { get; set; }
		public string Namespace { get; set; }
	}

}
