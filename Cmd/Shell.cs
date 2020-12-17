using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using ThreadState = System.Threading.ThreadState;

namespace Cmd
{
    public class Shell : IDisposable
    {
        private ShellStartup _startup;
        private Process _process = null;
        private ProcessStartInfo _info = null;
        internal DateTime _dt;
        private int _delayTime = 10000;
        private bool _finish = false;

            
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

        /// <summary> Инициализация оболочки command line с указанными параметрами </summary>
        /// <param name="startup"> Параметры запуска оболочки </param>
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
            return this;
        }

        /// <summary> Обработка части ответа и его форматирование </summary>
        /// <param name="e"> Параметр события вывода результата команды </param>
        /// <returns> Результирующая строка ответа (или его части если ответ многострочный) </returns>
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

        /// <summary> Выполнить указанную команду </summary>
        /// <param name="command"> Команда </param>
        public void Command(string command)
        {
            _finish = false;
            LastCommand = command;
            _process.StandardInput.WriteLine(command);
            _dt = DateTime.Now.AddSeconds(3);
        }

        /// <summary> Ожидание выполнения комманды </summary>
        public void WhileResult()
        {
            while (_dt>DateTime.Now && !_finish)
            {
               Application.DoEvents(); 
            }
        }

        /// <summary> Остановка ожидания завершения комманды </summary>
        public void CommandFinish()
        {
            _finish = true;
        }

        /// <summary> Выполнить указанную команду и дождаться результата </summary>
        /// <param name="command"> Команда </param>
        /// <param name="delay"> Интервал ожидания </param>
        /// <returns> Результат выполнения команды </returns>
        public string CommandWhile(string command, int delay = 5)
        {
            Command(command);
            _dt=_dt.AddSeconds(5);
            WhileResult();

            return CmdResult;
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}
