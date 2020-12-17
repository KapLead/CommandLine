using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
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

        public bool IsActive { get; private set; } = false;

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
        public Shell(ShellStartup startup = default)
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
                StandardErrorEncoding = Encoding.UTF8,
                Verb = _startup.Runas ? "runas" : "",
            };
            _process = new Process { StartInfo = _info };
            if (_startup.Redirect)
            {
                IsActive = _process.Start();
                _process.OutputDataReceived += ProcessOnOutputDataReceived;
                _process.ErrorDataReceived += ProcessOnErrorDataReceived;
                _process.Exited += (sender, args) => IsActive = false;
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
            else
            {
                OutputResult = CmdResult = _process.StandardOutput.ReadToEnd();
                ErrorResult = CmdResult = _process.StandardOutput.ReadToEnd();
            }
            //Command("chcp 866");
            //Command("ver");
            //Command("echo (c) Корпорация Майкрософт (Microsoft Corporation), 2020. Все права защищены.");
            return this;
        }

        private string RessiveDate(DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string msg = e.Data.Replace(Application.StartupPath, null);
                _dt = DateTime.Now.AddSeconds(1);
                return msg!="" ? msg + Environment.NewLine : string.Empty;
                //Encoding.Default.GetString(Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage).GetBytes(msg))
            }
            return string.Empty;
        }
        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = RessiveDate(e);
            if (line != null)
            {
                OnErrorLine?.Invoke(this, line);
                CmdError = line;
                ErrorResult += CmdResult;
                if (_startup.AllMessageInOutput)
                    CmdResult += line + Environment.NewLine;
            }
        }
        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = RessiveDate(e);
            if (line != null)
            {
                OnOutputLine?.Invoke(this, line);
                CmdResult = line + Environment.NewLine;
            }

            OutputResult += CmdResult;
        }

        public void Command(string command)
        {
            LastCommand = command;
            _process.StandardInput.WriteLine(command);
            _dt = DateTime.Now.AddSeconds(3);
            while (_dt>DateTime.Now)
            {
               Application.DoEvents(); 
            }
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}
