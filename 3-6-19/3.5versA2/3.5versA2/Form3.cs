using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _3._5versA2
{
    public partial class Form3 : Form
    {
        string deviceNumber="";
        public Form3()
        {
            InitializeComponent();
        }
 

        public string ReadDeviceNumber()
        {
            return deviceNumber;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            deviceNumber = textBox1.Text;
            
            this.Hide();
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (deviceNumber == "") {
                MessageBox.Show("Imbecile! You must enter the device number!","Enter Device Number", MessageBoxButtons.RetryCancel);
                if(MessageBox.Show("Imbecile! You must enter the device number!","Enter Device Number", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                {

                    e.Cancel = true;
                }
                else
                {

                    deviceNumber= "Nothing";
                }
            }
          
        }
    }
}
