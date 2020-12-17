# CommandLine
Враппер интерпритатора коммандной строки 

// Создать оболочку коммандной строки

Cmd.Shell shell = new Shell(new ShellStartup
{

     AllMessageInOutput = true, 
		 
     Redirect = true, ErrorDialog = false, 
		 
     Visible = false, Runas = false,
		 
     Argument = "/K chcp 65001", // установим кодировку UTF-8
		 
     Command = "cmd.exe" // запустим command line
		 
});

// подпишемся на выходной буффер

shell.OnOutputLine+= ShellOnOnOutputLine;


// Так может выглядеть вывод стро результатов в TextBox 

private void ShellOnOnOutputLine(Shell sender, string line)

{

    textBox1.Invoke(new Action((() =>
		
    {
		
        textBox1.Text += line;
				
        textBox1.SelectionStart = textBox1.TextLength - 1;
				
        textBox1.SelectionLength = 0;
				
        textBox1.ScrollToCaret();
				
        Application.DoEvents();
				
    })));
		
}
