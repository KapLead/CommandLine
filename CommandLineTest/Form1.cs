using System;
using System.Windows.Forms;
using Cmd;

namespace CommandLineTest
{
    public partial class Form1 : Form
    {
        private ShellAsync shell = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            shell = new ShellAsync(new ShellStartup
            {
                AllMessageInOutput = true, 
                Redirect = true, ErrorDialog = false, Visible = false, Runas = false,
                Argument = "/K chcp 65001", Command = "cmd.exe"
            });
            shell.OnOutputLine+= ShellOnOnOutputLine;
            shell.OnErrorLine+= ShellOnOnOutputLine;
        }

        private void ShellOnOnOutputLine(Shell sender, string line)
        {
            textBox1.Invoke(new Action((() =>{
                textBox1.Text += line;
                textBox1.SelectionStart = textBox1.TextLength - 1;
                textBox1.SelectionLength = 0;
                textBox1.ScrollToCaret();
                Application.DoEvents();
            })));
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                shell.CommandAsync(textBox2.Text);
                textBox2.Text = "";
                textBox2.Focus();
            }
        }
    }
}
