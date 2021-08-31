using Agitech;
using Agitech.Native;
using Agitech.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Clicker
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{

		#region Constructor & Properties

		public MainWindow()
		{
			var fp = @"C:\Windows\System32\ApplicationFrameHost.exe";
			var fpb = File.Exists(fp);

			InitializeComponent();
			UiDispatcher = Dispatcher.CurrentDispatcher;

			Binding binding = new Binding("ShowInTaskBar");
			binding.Source = Config.Instance;
			//binding. = "OnlyInTray";
			binding.Mode = BindingMode.OneWay;
			this.SetBinding(Window.ShowInTaskbarProperty, binding);

			this.Title = App.AppTitle;


			AppTraceListener mtl = new AppTraceListener(txtLog);
			//Debug.Listeners.Add(mtl);

			Trace.Listeners.Add(mtl);

			AgtSysMenuItem[] smArr = {
															 new AgtSysMenuItem(5, AgtNative.MenuFlags.MF_BYPOSITION | AgtNative.MenuFlags.MF_SEPARATOR, string.Empty),
															 new AgtSysMenuItem(6, AgtNative.MenuFlags.MF_BYPOSITION , "Settings...", smSettings),
															 new AgtSysMenuItem(6, AgtNative.MenuFlags.MF_BYPOSITION , "About...", smAbout)
														 };
			_smh = new AgtSysMenuHelper(this, smArr);
			ProgressVisibility = false;
		}

		AgtSysMenuHelper _smh;

		private Dispatcher UiDispatcher { get; set; }

		private readonly object SyncObj = new object();
		private bool stop = true;

		private bool Stop
		{
			get
			{
				lock (SyncObj)
				{
					return stop;
				}
			}
			set
			{
				lock (SyncObj)
				{
					stop = value;
				}
			}
		}

		public bool ProgressVisibility { get; set; }

		#endregion


		#region Events

		private void btnApps_Click(object sender, RoutedEventArgs e)
		{

		}

		private void txtLog_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox tbl = sender as TextBox;
			tbl?.ScrollToEnd();
		}

		private void wndMain_Closed(object sender, EventArgs e)
		{
			tb.Dispose();
		}

		private void wndMain_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private void wndMain_SourceInitialized(object sender, EventArgs e)
		{
			IntPtr hwnd = new WindowInteropHelper(this).Handle;
			try
			{
				App.Instance?.WriteHwnd(hwnd);
			}
			catch (System.Runtime.InteropServices.SEHException ex)
			{
				MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}\n\n", "Exception thrown");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}\n\n", "Exception thrown");
			}

			HwndSource source = HwndSource.FromHwnd(hwnd);
			source?.AddHook(new HwndSourceHook(WndProc));
		}

		private void wndMain_Initialized(object sender, EventArgs e)
		{

		}

		private void wndMain_Activated(object sender, EventArgs e)
		{
			Application.Current.MainWindow?.Show();
		}

		private void wndMain_GotFocus(object sender, RoutedEventArgs e)
		{
			Application.Current.MainWindow?.Show();
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			System.Windows.Forms.Message m = System.Windows.Forms.Message.Create(hwnd, msg, wParam, lParam);
			if (m.Msg == AgtNative.WM_COPYDATA)
			{
				// Get the COPYDATASTRUCT struct from lParam.
				AgtNative.COPYDATASTRUCT cds = (AgtNative.COPYDATASTRUCT)m.GetLParam(typeof(AgtNative.COPYDATASTRUCT));

				// If the size matches
				if (cds.cbData == Marshal.SizeOf(typeof(MsgStruct)))
				{
					// Marshal the data from the unmanaged memory block to a 
					// MsgStruct managed struct.
					MsgStruct myStruct = (MsgStruct)Marshal.PtrToStructure(cds.lpData,
							typeof(MsgStruct));

					// Display the MsgStruct data members.
					if (myStruct.Message == "Show Up")
					{
						this.ShowTop();
					}
				}
			}
			return IntPtr.Zero;
		}

		private void wndMain_Closing(object sender, CancelEventArgs e)
		{
			if (Config.Instance.MinimizeToTray && !trayClose)
			{
				Hide();
				e.Cancel = true;
				return;
			}
			Stop = true;
		}

		private void keyPanelA_Copy_GotFocus(object sender, RoutedEventArgs e)
		{

		}

		#endregion

		#region SysMenu

		public void smSettings()
		{
			Settings setWindow = new Settings();
			setWindow.Owner = this;
			setWindow.ShowDialog();
		}

		private void smAbout()
		{

		}

		#endregion

		#region TrayIcon

		private void TbIconColorChange(TbIconColor color)
		{

			Uri icoUri;

			switch (color)
			{
				case Clicker.TbIconColor.Reg:
					icoUri = new Uri("pack://application:,,,/Media/gearReg.ico");
					break;
				case Clicker.TbIconColor.Yellow:
					icoUri = new Uri("pack://application:,,,/Media/reagYellow.ico");
					break;
				case Clicker.TbIconColor.Green:
				default:
					icoUri = new Uri("pack://application:,,,/Media/gearGreen.ico");
					break;
			}

			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.UriSource = icoUri;
			bi.EndInit();
			tb.IconSource = bi;
		}

		private void ShowBallon(string title, string message, TbIconColor color = TbIconColor.Green)
		{
			Icon ic;
			Uri icoUri;
			switch (color)
			{
				case Clicker.TbIconColor.Reg:
					icoUri = new Uri("pack://application:,,,/Media/gearRegB.ico");
					break;
				case Clicker.TbIconColor.Yellow:
					icoUri = new Uri("pack://application:,,,/Media/reagYellowB.ico");
					break;
				case Clicker.TbIconColor.Green:
				default:
					icoUri = new Uri("pack://application:,,,/Media/gearGreenB.ico");
					break;
			}

			using (Stream iconStream = Application.GetResourceStream(icoUri).Stream)
			{
				ic = new System.Drawing.Icon(iconStream);
			}
			tb.ShowBalloonTip(title, message, ic);
		}


		private void tbShowHideMI_Click(object sender, RoutedEventArgs e)
		{
			ShowHide();
		}

		bool trayClose = false;

		private void ShowTop()
		{
			this.Show();
			this.Activate();
			this.Topmost = true;
			this.Topmost = false;
			this.Focus();
		}

		private void ShowHide()
		{
			if (Visibility == System.Windows.Visibility.Hidden)
			{
				ShowTop();
			}
			else if (Visibility == System.Windows.Visibility.Visible)
			{
				Hide();
			}
		}

		private void tbCloseMI_Click(object sender, RoutedEventArgs e)
		{
			trayClose = true;
			this.Close();
		}

		private void tb_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			ShowHide();
		}


		#endregion

	}

	enum TbIconColor
	{
		Green,
		Yellow,
		Reg
	}

}
