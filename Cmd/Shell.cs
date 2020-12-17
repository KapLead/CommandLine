using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary> Результат выполнения комманды </summary>
        public string CmdResult { get; private set; }

        /// <summary> Ошибки в ходе выполнения последней комманды </summary>
        public string CmdError { get; private set; }

        /// <summary> Последняя комманда </summary>
        public string LastCommand { get; private set; }

        /// <summary> Буфер выходного потока </summary>
        public string OutputResult { get; private set; }

        /// <summary> Буфер потока ошибок </summary>
        public string ErrorResult { get; private set; }

        public event ShellHandleLine OnOutputLine, OnErrorLine;
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
                WindowStyle = !_startup.Visible ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
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

        private string RessiveDate(DataReceivedEventArgs e)
        {
            string msg = e.Data.Replace(Application.StartupPath, null) + Environment.NewLine;
            _dt = DateTime.Now.AddMilliseconds(800);
            return msg + Environment.NewLine;
        }
        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = RessiveDate(e);
            OnErrorLine?.Invoke(this,line);
            CmdError = line + Environment.NewLine;
            ErrorResult += CmdResult;
        }
        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = RessiveDate(e);
            OnOutputLine?.Invoke(this, line);
            CmdResult = line + Environment.NewLine;
            OutputResult += CmdResult;
        }

        public void Command(string command)
        {
            LastCommand = command;
            _process.StandardInput.WriteLine(command);
        }

        public void Dispose()
        {
            _process?.Dispose();
            _info = null;
        }
    }
}
