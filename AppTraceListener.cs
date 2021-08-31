using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace Clicker
{

	public class AppTraceListener : TraceListener
	{
		private RichTextBox output;

		public AppTraceListener(RichTextBox output)
		{
			this.Name = "Trace";
			this.output = output;
		}

		public override void Write(string message)
		{
			if (!output.Dispatcher.CheckAccess())
			{
				output.Dispatcher.InvokeAsync(new System.Action(() => Write(message)));
				return;
			}
	
			//output.AppendText(string.Format("[{0}] ", DateTime.Now.ToString()));
			output.AppendText(message + System.Environment.NewLine);
		}

		public override void WriteLine(string message)
		{
			Write(message + Environment.NewLine);
		}
	} 
}
