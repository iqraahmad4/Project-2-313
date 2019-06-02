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
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ADD BUTTON
            // File Info
            FileInfo file = new FileInfo(@"H:\Project-2-313\3.5versA2\Parameters.txt");
            //write line using streamwriter
            using(StreamWriter writer = file.AppendText())
            {
                writer.WriteLine(textBox1.Text);
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
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //DELETE BUTTON
            string uni_iqra_path = @"H:\Project-2-313\3.5versA2\Parameters.txt";
            string p_to_be_deleted = Convert.ToString(listBox1.SelectedItem);
            var oldLines = System.IO.File.ReadAllLines(uni_iqra_path);
            string[] newLines={"","","","","",""};
            using (System.IO.StreamReader para = new System.IO.StreamReader(@"H:\Project-2-313\3.5versA2\Parameters.txt"))
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
            System.IO.File.WriteAllLines(uni_iqra_path,newLines);
            FileStream obj = new FileStream(uni_iqra_path, FileMode.Append);
            obj.Close();

            // once deleted the selected line and once again read the text file and diplay the new text file in listBox  
            FileInfo fi = new FileInfo(@"H:\Project-2-313\3.5versA2\Parameters.txt");
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
            string[] lines = System.IO.File.ReadAllLines(@"H:\Project-2-313\3.5versA2\Parameters.txt");
            foreach (string v in lines) // Read all line from the text file  
            {
                listBox1.Items.Add(v);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
