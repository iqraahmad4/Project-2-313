using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _3._5versA2;
using System.IO;


namespace _3._5versA2
{

    public partial class Form2 : Form
    {
        Functions func = new Functions();
        
        List <double> coefficients = new List<double>();
        int w = 10;
        int numOfSamples = 10;
        double H=0;
        double L=0;
        Form1 f1;

        public  void Values (List <double> cs, int window, double Hi, double Lo)
        {
          
        }


        public List<double> GetNewCoeff()
        {
            return coefficients;
        }

        public int GetNewWindow()
        {
            return w;
        }

        public double GetNewHigh()
        {
            return H;
        }

        public double GetNewLow()
        {
            return H;
        }

      

        public Form2(Form1 form1handel, List<double> cs, int window, double Hi, double Lo)
        {
      
            InitializeComponent();
            this.coefficients = cs;
            this.w = window;
            this.H = Hi;
            this.L = Lo;
            numericUpDown1.Value = w;
            textBox2.Text = H.ToString();
            textBox3.Text = L.ToString();
          
            for (int i = 0; i < coefficients.Count; i++)
            {
                listBox1.Items.Add(coefficients.ElementAt(i));
            }

            this.f1 = form1handel;

        }



        private void button1_Click(object sender, EventArgs e)
        {
           
            //REPLACE BUTTON
            //reads new coefficient entered
           
  
            int selectedIndex = listBox1.SelectedIndex;
            string newCoeff = textBox1.Text;
            try
            {
                listBox1.Items[selectedIndex] = newCoeff;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                MessageBox.Show("Why are you trying to replace nothing? Selct a coefficient.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            double sum = 0;
            bool redo = false;
            //write line using streamwriter
           for (int i = 0; i < listBox1.Items.Count; i++)
            {
                sum += Convert.ToDouble(listBox1.Items[i]);
                
            }
            if(sum != 1)
            {
                MessageBox.Show("I say, can't you add? Change the filter coefficients to equal 1.");
                 redo = true;
            }

            if (!redo)
            {
                H = Convert.ToDouble(textBox2.Text); L = Convert.ToDouble(textBox3.Text);

                List<double> endCoeff = new List<double>();
                for (int i=0; i < listBox1.Items.Count; i++)
                {
                    endCoeff.Add(Convert.ToDouble(listBox1.Items[i]));
                }

                f1.SetParameters(endCoeff, this.w, this.H, this.L);
                this.Hide();
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int newWin = Convert.ToInt32(numericUpDown1.Value);
            int listNum = listBox1.Items.Count;
            if (newWin < w)
            {
                //delete
                for (int i = listNum - 1; i < ((listNum -1) + (w - newWin)); i++)
                {
                    //Console.WriteLine(i);
                    listBox1.Items.Add(0);

                }
                w = newWin;

            }
            else if(newWin>w)
            {

                for (int i = (listNum - 1); i > (listNum - (newWin - w)-1); i--)
                {
                    listBox1.Items.RemoveAt(i);
                }
                w = newWin;

            }

        }
    }
}
