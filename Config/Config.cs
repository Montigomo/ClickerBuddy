using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Windows;
using System.Runtime.CompilerServices;

using Agitech;

namespace ClickerBuddy
{
	public class Config : INotifyPropertyChanged
	{

		#region Service

		private static Config _instance;
		private static string _configPath;
		private static XmlSerializer _serializer;

		[XmlIgnore]
		public static Config Instance
		{
			get
			{
				if (_instance == null)
				{
					if (String.IsNullOrEmpty(_configPath))
					{
						string fld1 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
						string fn1 = Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location);
						_configPath = fld1 + Path.DirectorySeparatorChar + fn1 + ".config.xml";
					}
					try
					{
						using (FileStream fs = new FileStream(_configPath, FileMode.Open))
							_instance = (Config)Serializer.Deserialize(fs);
					}
					catch (Exception)
					{
						_instance = new Config();
					}
					Application.Current.Exit += App_Exit;
				}
				return _instance;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public string ConfigPath
		{
			get
			{
				return _configPath;
			}
			set
			{
				_configPath = value;
			}
		}

		private static XmlSerializer Serializer
		{
			get
			{
				if (_serializer == null)
					_serializer = new XmlSerializer(typeof(Config));
				return _serializer;
			}
		}

		public void Save()
		{
			//try
			//{
			using (TextWriter twriter = new StreamWriter(_configPath))
			{
				Serializer.Serialize(twriter, Config.Instance);
				twriter.Close();
			}
			//}
			//catch { }
		}

		private static void App_Exit(object sender, ExitEventArgs e)
		{
			if(_instance != null)
				Config.Instance.Save();
		}


		#endregion

		#region PropertyChanges

		public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.
		// The CallerMemberName attribute that is applied to the optional propertyName
		// parameter causes the property name of the caller to be substituted as an argument.
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion


		[Category("Preferences"), DisplayName("Run At Startup"), Description("Run application at startup.")]
		[XmlElementAttribute("run-at-startup", IsNullable = false)]
		public bool RunAtStartup { get; set; }

		[Browsable(false)]
		private bool _showInTaskBar;

		[Category("Appearence"), DisplayName("Show in taskbar"), Description("Main window not displaed in taskbar.")]
		[XmlElementAttribute("only-in-tray", IsNullable = false)]
		public bool ShowInTaskBar
		{
			get
			{
				return _showInTaskBar;
			}
			set
			{
				if (_showInTaskBar != value)
				{
					_showInTaskBar = value;
					NotifyPropertyChanged();
				}
			}
		}

		

		[Category("Appearence"), DisplayName("Minimize/Close to tray"), Description("Close to tray.")]
		[XmlElementAttribute("min-to-tray", IsNullable = false)]
		public bool MinimizeToTray { get; set; }
	}
}
