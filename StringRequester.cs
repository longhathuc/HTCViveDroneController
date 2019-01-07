using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HTCViveDroneController
{
    public partial class StringRequester : Form
    {
        public string Prompt
        {
            get { return label1.Text; }
            set { label1.Text = value; textBox1.Text = ""; }
        }
        public string Response
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }
        public StringRequester(string prompt)
        {
            InitializeComponent();

            Prompt = prompt;
        }
    }
}
