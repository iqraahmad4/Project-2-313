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
        string filepath = @"H:\Project-2-313\3-6-19\3.5versA2\Parameters.txt";
        Functions func = new Functions();
        double[] coefficentArray = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        public Form2()
        {
            InitializeComponent();
            coefficentArray = (func.ReadParameters(filepath, coefficentArray));
            listBox1.Items.AddRange(new object[] { coefficentArray[0], coefficentArray[1], coefficentArray[2], coefficentArray[3], coefficentArray[4], coefficentArray[5]});
            int items = listBox1.Items.Count;

            if (listBox1.Items.Count < 6)
            {
                for (int i = 0; i < 6 - items; i++)
                {
                    Console.WriteLine(i);
                    listBox1.Items.Add(0);
               
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ADD BUTTON
            //reads new coefficient entered
            
  
            int selectedIndex = listBox1.SelectedIndex;
            string newCoeff = textBox1.Text;
            listBox1.Items[selectedIndex] = newCoeff;
            Console.WriteLine(listBox1.Items[selectedIndex]);
          /*  // File Info
            FileInfo file = new FileInfo(@filepath);
            //write line using streamwriter
            using(StreamWriter writer = file.AppendText())
            {
                writer.WriteLine(newCoeff);
            }
            // Read the content using streamreader from text file
            using(StreamReader reader = file.OpenText())
            {
                string l = "";
                while((l = reader.ReadLine()) != null)
                {
                    listBox1.Text = l; //display line added
                }
                reader.Close();
            } */
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //DELETE BUTTON
            string uni_iqra_path = filepath;
            string p_to_be_deleted = Convert.ToString(listBox1.SelectedItem);
            var oldLines = System.IO.File.ReadAllLines(filepath);
            string[] newLines={"","","","","",""};
            using (System.IO.StreamReader para = new System.IO.StreamReader(filepath))
            {

                int counter = 0;
                string current_line;
                while ((current_line = para.ReadLine()) != null)
                {
                    if(current_line != p_to_be_deleted)
                    {
                        newLines[counter] = current_line;
                        counter++;
                    }
                }
                para.Close();
                counter = 0;
            }
            System.IO.File.WriteAllLines(filepath,newLines);
            FileStream obj = new FileStream(filepath, FileMode.Append);
            obj.Close();

            // once deleted the selected line and once again read the text file and diplay the new text file in listBox  
            FileInfo fi = new FileInfo(filepath);
            using (StreamReader sr = fi.OpenText())
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    listBox1.Text = s;
                }
                sr.Close();
            }
            FileStream obj1 = new FileStream(uni_iqra_path, FileMode.Append);
            obj1.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //view button
            string[] lines = System.IO.File.ReadAllLines(filepath);
            foreach (string v in lines) // Read all line from the text file  
            {
                listBox1.Items.Add(v);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            FileInfo file = new FileInfo(filepath);
            //write line using streamwriter
            using (StreamWriter writer = new StreamWriter(filepath, false))
            {
                for (int i = 0; i <6; i++)
                {
                    writer.WriteLine(listBox1.Items[i]);
                }
            }
            this.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            //replace changed value
        }
    }
}
