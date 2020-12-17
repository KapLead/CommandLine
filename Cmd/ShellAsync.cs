using System;
using System.Threading;
using System.Windows.Forms;

namespace Cmd
{
    public class ShellAsync : Shell
    {
        public ShellAsync(ShellStartup startup) : base(startup)
        {
        }

        /// <summary> Асинхронное выполнение комманды </summary>
        /// <param name="command"> Команда </param>
        /// <returns> Результат выполнения команды </returns>
        public string CommandAsync(string command)
        {
            base._dt = DateTime.Now.AddSeconds(5);
            new Thread((() => base.Command(command))) { IsBackground = true}.Start();
            base.WhileResult();
            return CmdResult;
        }
    }
}
