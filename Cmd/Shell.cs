using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace Cmd
{
    public class Shell : IDisposable
    {
        private ShellStartup _startup;
        private Process _process = null;
        private ProcessStartInfo _info = null;
        private DateTime _dt;
        private int _delayTime = 10000;
        private string _cmdResult, _cmdError;
       
        /// <summary> Буфер выходного потока </summary>
        public string OutputResult { get; private set; } = null;

        /// <summary> Буфер потока ошибок </summary>
        public string ErrorResult { get; private set; } = null;

        public Shell(ShellStartup startup=default)
        {
            Init(startup);
        }

        public Shell Init(ShellStartup startup)
        {
            _startup = startup;
            _info = new ProcessStartInfo(_startup.Command, _startup.Argument)
            {
                CreateNoWindow = !_startup.Visible,
                UseShellExecute = !_startup.Redirect,
                WindowStyle = _startup.Visible ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                RedirectStandardOutput = _startup.Redirect,
                RedirectStandardError = _startup.Redirect,
                RedirectStandardInput = _startup.Redirect,
                ErrorDialog = _startup.ErrorDialog,
                StandardOutputEncoding = Encoding.UTF8,
                Verb = _startup.Runas ? "runas" : ""
            };
            if (_startup.Redirect && _process != null)
            {
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                _process.OutputDataReceived += ProcessOnOutputDataReceived;
                _process.ErrorDataReceived += ProcessOnErrorDataReceived;
            }
            _process = Process.Start(_info);
            return this;
        }
        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string msg = e.Data.Replace(Application.StartupPath, null) + Environment.NewLine;
            _dt = DateTime.Now.AddMilliseconds(800);
            _cmdError += msg + Environment.NewLine;
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string msg = e.Data.Replace(Application.StartupPath, null) + Environment.NewLine;
            _dt = DateTime.Now.AddMilliseconds(800);
            _cmdResult += msg + Environment.NewLine;
        }

        public void Dispose()
        {
            _process?.Dispose();
            _info = null;
        }
    }
}
