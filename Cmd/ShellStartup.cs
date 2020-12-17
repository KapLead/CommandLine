namespace Cmd
{
    /// <summary> Параметры запуска коммандной строки </summary>
    public struct ShellStartup
    {
        /// <summary> Пустая дефолтная структура параметров запуска коммандной строки </summary>
        public static ShellStartup Empty = new ShellStartup();
      
        /// <summary> Дефолтное предопределенное значение для открытия оболочки и управление средствами csharp </summary>
        public static ShellStartup Default = new ShellStartup
        {
            Argument = "/C",
            Command = "cmd.exe",
            AllMessageInOutput = true,
            Redirect = true,
            ErrorDialog = true,
            Runas = false,
            Visible = false
        };

        /// <summary> Комманда для запуска </summary>
        public string Command;
        /// <summary> Аргумент комманды </summary>
        public string Argument;
        /// <summary> Запуск от имени локального администратора </summary>
        public bool Runas;
        /// <summary> Видимость процесса выполнения команды </summary>
        public bool Visible;
        /// <summary> Следует ли отображать диалоговое окно сообщения об ошибке в случае невозможности запуска приложения </summary>
        public bool ErrorDialog;
        /// <summary> Выводить все сообщения в канал вывода </summary>
        public bool AllMessageInOutput;
        /// <summary> Перехватывать ввод вывод данных </summary>
        public bool Redirect;
    }
}
