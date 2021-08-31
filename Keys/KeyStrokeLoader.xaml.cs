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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Agitech.Extensions;

namespace ClickerBuddy.Keys
{
	/// <summary>
	/// Interaction logic for KeyStrokeLoader.xaml
	/// </summary>
	public partial class KeyStrokeLoader : Window
	{
		public KeyStrokeLoader()
		{
			RefreshNames();
			InitializeComponent();
		}

		#region Load Save


		private static string _fileExtension = ".ksc";

		private static string _storeFolder;

		public static string StoreFolder
		{
			get
			{
				if (String.IsNullOrEmpty(_storeFolder))
				{
					_storeFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Backup";
					if (!System.IO.Directory.Exists(_storeFolder))
						System.IO.Directory.CreateDirectory(_storeFolder);
				}
				return _storeFolder;
			}
			set
			{
				_storeFolder = value;
			}
		}

		private ObservableCollection<string> _fileNames;

		public ObservableCollection<string> FileNames
		{
			get
			{
				return _fileNames;
			}
			set
			{
				_fileNames = value;
			}
		}

		private void RefreshNames()
		{
			DirectoryInfo di = new DirectoryInfo(StoreFolder);
			FileNames = new ObservableCollection<string>(di.GetFiles("*" + _fileExtension).Select(item => item.Name).ToList());
		}

		public static void Save(KeyStrokeCollection ksc, string name)
		{
			string fileName = System.IO.Path.Combine(StoreFolder, name);
			FileStream fileStreamObject = new FileStream(fileName, FileMode.Create);

			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(fileStreamObject, ksc);
			}
			finally
			{
				fileStreamObject.Close();
			}
		}

		public static KeyStrokeCollection Load(string name)
		{
			string fileName = System.IO.Path.Combine(StoreFolder, name);
			FileStream fileStreamObject = new FileStream(fileName, FileMode.Open); ;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				return (KeyStrokeCollection)(binaryFormatter.Deserialize(fileStreamObject));
			}
			finally
			{
				fileStreamObject.Close();
			}

		}

		private void SetMode()
		{
			cmbName.IsEditable = KslMode == KslMode.Save;
			btnAction.Content = KslMode.ToString();
			cmbName.ItemsSource = FileNames;
			cmbName.SelectedIndex = KslMode == KslMode.Save ? -1 : 0;
		}

		#endregion

		private KslMode _kslMode = KslMode.Load;

		internal KslMode KslMode
		{
			get
			{
				return _kslMode;
			}
			set
			{
				_kslMode = value;
			}
		}

		private KeyStrokeCollection _ksl;

		public KeyStrokeCollection Ksl
		{
			get
			{
				return _ksl;
			}
			set
			{
				_ksl = value;
			}
		}

		private void ShowInfo(string message)
		{
			lblInfo.Content = message;
		}

		private string ToFileName(string name)
		{
			return System.IO.Path.HasExtension(name) ? name : (name.CleanFileName("") + _fileExtension);
		}

		private void btnAction_Click(object sender, RoutedEventArgs e)
		{
			if (KslMode == Keys.KslMode.Save)
			{
				if (String.IsNullOrEmpty(cmbName.Text))
				{
					ShowInfo("Enter name");
					return;
				}
				string name = ToFileName(cmbName.Text);
				if (FileNames.Contains(name))
				{
					MessageBoxResult res = MessageBox.Show("File with the same name exist. Overwrite?", "Action", MessageBoxButton.YesNo);
					if (res == MessageBoxResult.No)
						return;
				}
				Save(Ksl, name);
				this.DialogResult = true;
				this.Close();
			}
			else
			{
				if (String.IsNullOrEmpty(cmbName.Text))
				{
					ShowInfo("Select name");
					return;
				}
				string name = ToFileName(cmbName.Text);
				Ksl = Load(name);
				this.DialogResult = true;
				this.Close();
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SetMode();
			if (Owner != null)
			{
				Left = Owner.Left + Owner.Width / 2 - Width / 2;
				Left = Left > 0 ? Left : 30;
				Top = Owner.Top + Owner.Height / 2 - Height / 2;
			}
		}
	}


	internal enum KslMode
	{
		Load,
		Save
	}


}
