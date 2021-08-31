using System;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Security;
using System.ServiceProcess;
using System.IO.MemoryMappedFiles;

using Agitech;
using Agitech.Native;
using Agitech.App;

namespace Clicker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		public static readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

		public static readonly string AppTitle = "Clicker  - " + Version;

		private readonly string _mutexName = "Local\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

		public static OneInstance Instance{ get; private set; }

		#region OnStartUp

		protected override void OnStartup(StartupEventArgs e)
		{
			string[] commandLineArgs = System.Environment.GetCommandLineArgs();
			try
			{
				Instance = new OneInstance(_mutexName);
				if (!Instance.Initialize())
				{
					Application.Current.Shutdown();
					return;
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace + "\n\n", "Exception thrown");
				Application.Current.Shutdown();
				return;
			}
			finally
			{

			}
			base.OnStartup(e);
		}

		#endregion


		#region OnExit

		protected override void OnExit(ExitEventArgs e)
		{
			Instance.Release();
			base.OnExit(e);
		}

		#endregion


		private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			string errorMessage = string.Format("An application error occurred. If this error occurs again there seems to be a serious bug in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n (if you click Yes you will continue with your work, if you click No the application will close)",
																					e.Exception.Message);
			//insert code to log exception here 
			if (MessageBox.Show(errorMessage, "Application UnhandledException Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
			{
				if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
				{
					Application.Current.Shutdown();
				}
			}
			e.Handled = true;
		}

		//public static void InstallService()
		//{
		//	// установить
		//	using (ProjectInstaller pi = new ProjectInstaller())
		//	{
		//		IDictionary savedState = new Hashtable();
		//		try
		//		{
		//			pi.Context = new InstallContext();
		//			pi.Context.Parameters.Add("assemblypath", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\" -service");
		//			foreach (Installer i in pi.Installers)
		//				i.Context = pi.Context;
		//			pi.Install(savedState);
		//			pi.Commit(savedState);
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine(ex.Message);
		//			pi.Rollback(savedState);
		//		}
		//	}
		//}

		//public static void RemoveService()
		//{
		//	// удалить
		//	using (ProjectInstaller pi = new ProjectInstaller())
		//	{
		//		try
		//		{
		//			pi.Context = new InstallContext();
		//			pi.Context.Parameters.Add("assemblypath", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\" -service");
		//			foreach (Installer i in pi.Installers)
		//				i.Context = pi.Context;
		//			pi.Uninstall(null);
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine(ex.Message);
		//		}
		//	}
		//}

	}
}
