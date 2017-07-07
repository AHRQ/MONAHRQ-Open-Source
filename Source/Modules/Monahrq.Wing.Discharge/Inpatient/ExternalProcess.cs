using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Wing.Discharge.Inpatient {
	/// <summary>
	/// Executes an external .exe that generates the bulk of the discharage report data.
	/// </summary>
	public class ExternalProcess {

		/// <summary>
		/// Gets a value indicating whether this instance has exited.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has exited; otherwise, <c>false</c>.
		/// </value>
		public Boolean HasExited
        {
            get;

            private set;
        }
		/// <summary>
		/// The process
		/// </summary>
		private Process process;
		/// <summary>
		/// Executes the file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		public void ExecFile(string fileName) {
            HasExited = false;
            process = new Process();
            var psi = new ProcessStartInfo();
            if (!File.Exists(fileName)) return;
            psi.FileName = fileName;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            process.StartInfo = psi;
            process.Start();
            do {
                var e = new ProcessEventArgs();
                e.Message = process.StandardOutput.ReadLineAsync().Result;
                OnMessageReceived(e);
            } while (!process.HasExited);
            process.Close();
            HasExited = true;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
		public delegate void ProcessEventHandler(object sender, ProcessEventArgs e);
		/// <summary>
		/// Occurs when [message received].
		/// </summary>
		public event ProcessEventHandler MessageReceived;
		/// <summary>
		/// Raises the <see cref="E:MessageReceived" /> event.
		/// </summary>
		/// <param name="e">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
		protected virtual void OnMessageReceived(ProcessEventArgs e) {
            if (MessageReceived != null) MessageReceived(this, e);
        }
    }
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.EventArgs" />
	public class ProcessEventArgs : EventArgs {
		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		/// <value>
		/// The message.
		/// </value>
		public string Message { get; set; }
    }
}
