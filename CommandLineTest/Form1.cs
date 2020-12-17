using System;
using System.Windows.Forms;
using Cmd;

namespace CommandLineTest
{
    public partial class Form1 : Form
    {
        private Cmd.Shell shell = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            shell = new Shell(ShellStartup.Default);
            shell.OnOutputLine+= ShellOnOnOutputLine;
            shell.OnErrorLine+= ShellOnOnOutputLine;
        }

        private void ShellOnOnOutputLine(Shell sender, string line)
        {
            
        }
    }
}
