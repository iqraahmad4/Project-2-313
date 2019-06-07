using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _3._5versA2;
using System.Threading;





namespace _3._5versA2
{
    public partial class Form1 : Form
    {                                                                           //\/\/\// Global Variables //\/\/\/\//
                                                                            //*    Specifications for thermistors     *\\
                                                                            //*     [sensor 0, sensor1, sensor2]      *\\
        int[] r = { 10000, 5000, 100000 };                                  //*     ~~ Resistance @ 25 degrees ~~     *\\
        int[] B = { 3380, 3960, 4380 };                                     //*            ~~ B constant ~~           *\\
        double count = 0;                                                      //*       Data Collected Counter          *\\
        int timerticks = 0;                                                 //*              Timer Counter            *\\
        int windowSize=0;                                                   //*               Window Size             *\\

        double High=0;                                                      //*          Controller High Band         *\\
        double Low=0;                                                       //*           Controller Low Band         *\\
        double userTemp;                                                    //*           Desired Temperature         *\\
        double temp;                                                        //*           Current Temperature         *\\
        double roomTemp;                                                    //*        Initial Room Temperature       *\\
        double volt0Weighted = 0;                                           //*       Filtered Voltage: Sensor 0      *\\
        double volt1Weighted = 0;                                           //*       Filtered Voltage: Sensor 1      *\\
        double volt2Weighted = 0;                                           //*       Filtered Voltage: Sensor 2      *\\

        List<double> coefficients = new List<double>();                     //*           Filter Coefficients         *\\

        bool On;                                                            //*               System On/Off           *\\                    
        bool sensor0;                                                       //*              Sensor 0 On/Off          *\\
        bool sensor1;                                                       //*              Sensor 1 On/Off          *\\
        bool sensor2;                                                       //*              Sensor 2 On/Off          *\\

        string dev;                                                         //*               Device Number           *\\     
                                                                            //*      Channels for each thermistor     *\\
        AnalogI aIn0 = new AnalogI();                                       //*             Sensor 0 Channel          *\\
        AnalogI aIn1 = new AnalogI();                                       //*             Sensor 1 Channel          *\\
        AnalogI aIn2 = new AnalogI();                                       //*             Sensor 2 Channel          *\\
   
        DigitalO dOut = new DigitalO();                                         //\/\/\// Global Variables //\/\/\/\//
        int closingcount;
        string ParameterPath = @"H:\Project-2-313\3-6-19\3.5versA2\Parameters.txt";
        string tempPath = @"H:\Project-2-313\3-6-19\3.5versA2\Temperatures.txt";
        Functions func = new Functions();

                ///DEBUGGING\\\


 bool reseting=false;

   

        
                                                                                                                                        //\/\/\/\/\/\/\/\/\/\// Helper Functions //\/\/\/\/\/\/\/\/\/\\


        public Form1()
        {
                                                          //\/\/\/\/\/\/\/\\ User Input //\/\/\/\/\/\/\//
            Form3 input = new Form3();                 //*   Open User Form to input the device number   *\\
            input.ShowDialog();                        //*        for the temperature chamber            *\\
            dev = input.ReadDeviceNumber();            //*       Read Device Number from form            *\\
                                                       //\/\/\/\/\/\/\/\\ User Input //\/\/\/\/\/\/\//
                                                                                                                                               //\/\/\/\/\/\/\/\/\/\/\// Initialization //\/\/\/\/\/\/\/\/\/\/\\
            InitializeComponent();                                                                                                                  //*                    Initializing all components                 *\\
            
            try
            {
                //*               Opening Channels for Thermistor sensors          *\\
                aIn0.OpenChannel("0", "Ainport0", dev);                                                                                                  //*                          Sensor 0 Channel                      *\\
                aIn1.OpenChannel("1", "Ainport1", dev);                                                                                                  //*                          Sensor 1 Channel                      *\\
                aIn2.OpenChannel("2", "Ainport2", dev);                                                                                                  //*                          Sensor 2 Channel                      *\\        
                dOut.OpenChannel(dev);                                                                                                                  //*                        Open Digital Channel                    *\\
                On = false;
                //*                                                                 *\\

                //*                    Set System initially to 'Off'               *\\
                //*                                                                *\\
                dOut.WriteData(0);                                                                                                                      //*                Set Fan and Heater initally to  'Off'           *\\  
                GUISettings(false, textBox5, "Fan"); GUISettings(false, textBox4, "Heat");                                                              //*        Change GUI to reflect status of the fan and heater      *\\
                                                                    //*                                                                *\\
                sensor0 = true; sensor1 = true; sensor2 = true;                                                                                         //*                   Set all sensors initally to 'On'             *\\
                coefficients = func.ReadParameters(ParameterPath);                                                                                    //*                Read Filter Parameters from text file           *\\
                windowSize = func.ReadWindowSize(ParameterPath);
                High = func.ReadHighBand(ParameterPath);
                Low = func.ReadLowBand(ParameterPath);

                for( int i=0; i<coefficients.Count; i++)
                {
                    listBox1.Items.Add(coefficients.ElementAt(i).ToString());
                }
                textBox10.Text = windowSize.ToString(); textBox11.Text = High.ToString(); textBox12.Text = Low.ToString();                                                                                                                                                          //*                                                                *\\
                                                                                                                                                                          //*                  Calculate Room Temperature                    *\\
                roomTemp =   (func.CalcTemp(0, func.ReadTemperaturet(aIn0, coefficients, windowSize)) + 
                                func.CalcTemp(1, func.ReadTemperaturet(aIn1,coefficients, windowSize)) + 
                                func.CalcTemp(2, func.ReadTemperaturet(aIn2,coefficients, windowSize))) / 3;
                //*                                                                *\\
                userTemp = Convert.ToDouble(numericUpDown1.Value) + roomTemp;                                                                           //* Set desired temperature as User Input (2-5) + room temperature *\\
                textBox7.Text = userTemp.ToString(); textBox13.Text = roomTemp.ToString();                                                              //*                                                                *\\

            }
            catch (NationalInstruments.DAQmx.DaqException){
                if (dev == "Nothing")
                {
                    Form1_Load(this, null);
                    this.Close();

                }
            }

        }    
                        //*                                                                *\\
    private void GUISettings(bool Item, TextBox textbox, string item)
        {

            if (textbox.InvokeRequired)
            {
                if (Item)
                {

                    textbox.Invoke((MethodInvoker)delegate { textbox.Text = item + " On"; });
                    textbox.Invoke((MethodInvoker)delegate { textbox.BackColor = Color.DarkSeaGreen; });
                    
                }
                else
                {
                    textbox.Invoke((MethodInvoker)delegate { textbox.Text = item + " Off"; });
                    textbox.Invoke((MethodInvoker)delegate { textbox.BackColor = Color.Firebrick; });
                }
            }
            else
            {
                if (Item)
                {
                    textbox.Text = item + " On"; textbox.BackColor = Color.DarkSeaGreen;
                }
                else
                {
                    textbox.Text = item + " Off"; textbox.BackColor = Color.Firebrick;
                }
            }
        }                

        public void SetParameters(List<double>cs, int wind, double Hi, double Lo)
        {
            this.coefficients = cs;
            this.windowSize = wind; this.High = Hi; this.Low = Lo;
            listBox1.Items.Clear();
            for (int i = 0; i < coefficients.Count; i++)
            {
                listBox1.Items.Add(coefficients.ElementAt(i).ToString());
            }
            textBox10.Text = windowSize.ToString(); textBox11.Text = High.ToString(); textBox12.Text = Low.ToString();

            for (int i=0; i<coefficients.Count; i++)
            {
                Console.Write(" " + coefficients[i]);
            }
            Console.WriteLine(" window: " + windowSize + " high: " + High + " Low: " + Low);
        }
                                                                                                                                                    //\/\/\/\/\/\/\/\/\/\/\// Initialization //\/\/\/\/\/\/\/\/\/\/\\
                                                                                                                                              /*
                                                                                                                                              Button 1: Turn System On/Off
                                                                                                                                              Turns system on and off. Bool 'On' toggles between true/false when button is pressed 
                                                                                                                                              by the user in the GUI. True--> On; False--> Off.
                                                                                                                                              When true, the system reads the parameters file and stores weight coefficient values 
                                                                                                                                              in the Coefficient Array.
                                                                                                                                              When false, the heater is turned off, the fan is turned on and the system cools 
                                                                                                                                              back to room temperature, before turning the fan off.
                                                                                                                                              */
        private void button1_Click(object sender, EventArgs e)                                                                                              //*                                                                    *\\
        {                                                                                                                                                   //*                                                                    *\\
            DigitalO dOut = new DigitalO();                                                                                                                 //*                                                                    *\\                                                                                         //*                                                                    *\\
            dOut.OpenChannel(dev);                                                                                                                          //*                                                                    *\\
            On = !On;                                                                                                                                       //*                   Toggle the button  'On' or 'Off'                 *\\
                                                                                                                                                            //*                                                                    *\\                     
            if (On)                                                                                                                                         //*                       If turning the system on                     *\\
            {
                backgroundWorker1.RunWorkerAsync();                                                                                                     //*                                                                    *\\
            }                                                                                                                                               //*                                                                    *\\
            else                                                                                                                                            //*                      If turning the system off                     *\\
            {     
                backgroundWorker3.RunWorkerAsync();  
                On = false;                                                                                                                                 //*                                                                    *\\
            }                                                                                                                                               //*                                                                    *\\
        }                                                                                                                                                   //*                                                                    *\\
                                                                                                                                                            
 /*
Timer 1: Reading Voltages & Calculating Temperatures
Timer ticks ever 0.1 seconds, a frequency of 10 Hz. At each tick, the voltage data is read
as an array of 6 rolled averages and the data is weighted against ,per sensor.
Every 5 ticks, or 0.5 seeconds, the temperature for each sensor is calculated, provided
that sensor is activated. Sensors whose temperatures were calculated are output into 
their corresponding textboxes. The individual sensor temperatures, as well as an average
of all the calculated sensor temperatures are written into a text file 'Temperatures'
The timer counter resets to 0 after 5 ticks to recount another period of 0.5 seconds.
*/
        private void timer1_Tick(object sender, EventArgs e)                                                                                //*                                                        *\\
        {                                                                                                                                   //*                                                        *\\
                                                                                                                                            //*    Change GUI to reflect status of the fan and heater  *\\
            volt0Weighted = func.ReadTemperaturet(aIn0,coefficients, windowSize);
            volt1Weighted = func.ReadTemperaturet(aIn1,coefficients, windowSize);
            volt2Weighted = func.ReadTemperaturet(aIn2,coefficients, windowSize);          //*     Get weighted average of voltage for each sensor    *\\
            if (On || reseting)                                                                                                    //*                     If system is on                    *\\
            {                                                                                                                               //*                                                        *\\                                                                                                                            //*                                                        *\\
                timerticks += 1;                                                                                                            //*                  Increment timerticks                  *\\
                int tally = 0;                                                                                                              //*                                                        *\\
                
                count += 0.1;
                string[] data = { "", "", "" };                                                                         //*                  Reset timerticks to 0                 *\\
                if (sensor0) { }                                                                                                          //*                    If sensor activate                  *\\
                {                                                                                                                       //*                                                        *\\
                    data = func.sensorCalc(volt0Weighted, 0, data);                                                                       //*               Calculate sensor temperature             *\\
                                                                                                  //*           Store sensor temperature in textbox          *\\
                    tally += 1;                                                           //*            Store sensor temperature in array           *\\
                }                                                                                                                       //*                                                        *\\
                if (sensor1)                                                                                                            //*                    If sensor activate                  *\\
                {                                                                                                                       //*                                                        *\\
                    data = func.sensorCalc(volt0Weighted, 1, data);                                                                      //*               Calculate sensor temperature             *\\
                                                                                                                                         //*                                                        *\\
                                                                                                  //*           Store sensor temperature in textbox          *\\
                    tally += 1;                                                                                   //*            Store sensor temperature in array           *\\
                }                                                                                                                       //*                                                        *\\
                if (sensor2)                                                                                                            //*                    If sensor activate                  *\\
                {                                                                                                                       //*                                                        *\\
                    data = func.sensorCalc(volt0Weighted, 2, data);                                                                        //*               Calculate sensor temperature             *\\
                                                                                                                                           //double temp2 = 24.67488596;                                                                                                             //*                                                        *\\
                                                                                               //*           Store sensor temperature in textbox          *\\
                    tally += 1;                                                                                //*            Store sensor temperature in array           *\\
                }                                                                                                                       //*                                                        *\\
                                                                                                                                        //*                                                        *\\

                //*      Average temperature = sum of data divided by      *\\
                temp = func.CalcAvgTemp(data, tally);                                                                             //*                 Number of active sensors               *\\
                                                                                             //*                                                        *\\
                func.WriteTemperature(tempPath, temp, data, count);


                if (timerticks == 5)                                                                                                        //*                     If 5 ticks pass                    *\\
                {                                                                                             //*               Increment data record number             *\\
                    timerticks = 0;
                    if (sensor0) { }                                                                                                          //*                    If sensor activate                  *\\
                    {                                                                                                                       //*                                                        *\\
                        
                        textBox1.Text = data[0];                                                                                   //*           Store sensor temperature in textbox          *\\
                       
                    }                                                                                                                       //*                                                        *\\
                    if (sensor1)                                                                                                            //*                    If sensor activate                  *\\
                    {                                                                                                                       //*                                                        *\\
                       
                                                                                                                                             //*                                                        *\\
                        textBox2.Text = data[1];                                                                                   //*           Store sensor temperature in textbox          *\\

                    }                                                                                                                       //*                                                        *\\
                    if (sensor2)                                                                                                            //*                    If sensor activate                  *\\
                    {                                                                                                 //*                                                        *\\
                        textBox3.Text = data[2];                                                                                   //*           Store sensor temperature in textbox          *\\
                       
                    }                                                                                                                       //*                                                        *\\
                                                                                                                                            //*                                                        *\\

                    //*      Average temperature = sum of data divided by      *\\
                                                                                         //*                 Number of active sensors               *\\
                    if (tally == 0)                                                                                                         //*                                                        *\\
                    {                                                                                                                       //*   Display 'No Sensors Read' if no sensors are active   *\\ 
                        textBox6.Text = "No Sensors Read";                                                                                  //*                                                        *\\
                    }                                                                                                                       //*                                                        *\\
                    else                                                                                                                    //*                                                        *\\
                    {                                                                                                                       //*     Otherwise, display average temperature in GUI      *\\
                                                                                                                                            //*                                                        *\\
                        textBox6.Text = temp.ToString();                                                                                    //*                                                        *\\
                    }    
                    //*                                                        *\\
                }                                                                                                                           //*                                                        *\\
            }                                                                                                                               //*                                                        *\\
        }                                                                                                                                   //*                                                        *\\

        // When pressed, the corresponding sensor will toggle from being on or off. 
        // This will determine which sensor values will be considered according to 
        // the user interface. Sensor variables are defaulted to true and can be 
        // changed by pressing the corresponding button.
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            sensor0 = checkBox1.Checked;
            if (checkBox1.Checked == true)
            {
                textBox1.BackColor = Color.Honeydew;
                checkBox1.Text = "Active";
            }

            else 
            {
                textBox1.BackColor = Color.MistyRose;
                checkBox1.Text = "Unactive";
                textBox1.Clear();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            sensor1 = checkBox2.Checked;
            if (checkBox2.Checked == true)
            {
                textBox2.BackColor = Color.Honeydew;
                checkBox2.Text = "Active";
            }

            else
            {
                textBox2.BackColor = Color.MistyRose;
                checkBox2.Text = "Unactive";
                textBox2.Clear();
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            sensor2 = checkBox3.Checked;
            if (checkBox3.Checked == true)
            {
                textBox3.BackColor = Color.Honeydew;
                checkBox3.Text = "Active";
            }

            else
            {
                textBox3.BackColor = Color.MistyRose;
                checkBox3.Text = "Unactive";
                textBox3.Clear();
            }
        }
        
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            userTemp = Convert.ToDouble(numericUpDown1.Value) + roomTemp;
            textBox7.Text = userTemp.ToString();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form2 popup = new Form2(this, coefficients, windowSize, High, Low);
            popup.Show();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!double.IsNaN(temp))
               {
                double error = temp - userTemp;
                Console.Write(temp + " " + userTemp + " " + error);

                if (error > High)
                   {
                       dOut.WriteData(0);
                       dOut.WriteData(1);
                    Console.WriteLine(temp);
                    GUISettings(true, textBox5, "Fan");
                    GUISettings(false, textBox4, "Heat");
                    //  textBox5.Text = "Fan On"; textBox4.Text = "Heater Off"; 
                    //textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.DarkSeaGreen;
                }
                else if (error < Low)
                {
                    dOut.WriteData(0);
                    dOut.WriteData(2);
                    Console.WriteLine(temp);
                    GUISettings(false, textBox5, "Fan");
                    GUISettings(true, textBox4, "Heat");
                    //  textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off"; 
                    //textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
                }
                else 
                   {
                       dOut.WriteData(0);
                    Console.WriteLine(temp);
                    GUISettings(false, textBox5, "Fan");
                    GUISettings(false, textBox4, "Heat");
                    //textBox5.Text = "Fan On"; textBox4.Text = "Heater On"; 
                    //textBox4.BackColor = Color.DarkSeaGreen; textBox5.BackColor = Color.DarkSeaGreen;
                }

                if (!On)
                {
                    dOut.WriteData(0);
                    GUISettings(false, textBox5, "Fan");
                    GUISettings(false, textBox4, "Heat");
                    //textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off"; textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
                }

               } 

        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!On)
            {
                backgroundWorker1.CancelAsync();
            }
            else
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        //closing while fan on
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dev != "Nothing")
            {
                closingcount += 1;
                MessageBox.Show(func.ClosingForm1(reseting, closingcount));
                if (reseting)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }


        //fan on after turn off
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            //*                                                                    *\\
            //*                    Calculate current temperature                   *\\                                                                    *\\
            Console.WriteLine("temp:" + temp + "> RT:" + roomTemp);                                                                                     //*                                                                    *\\
                                                                                                                                                        //*                                                                    *\\
            dOut.WriteData(0);
            dOut.WriteData(1);                                                                                                                      //*                           Keep  fan on                             *\\
            reseting = true;
            GUISettings(true, textBox5, "Fan"); GUISettings(false, textBox4, "Heat");            //*                                                                    *\\
                                                                                                 // textBox5.Text = "Fan On"; textBox4.Text = "Heater Off"; textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.DarkSeaGreen;  //*         Change GUI to reflect status of the fan and heater         *\\
        }                                                                                                                                               //*                                                                    *\\
                                                                                                                                                        //*                                                                    *\\                
                                                                                                                                                        //*                                                                    *\\
                                                                                                                                                        //Console.WriteLine("Heat & Fan Off");                                                                                                  //*                                                                    *\\
         
        
        //fan on after turn off
        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            temp = (func.CalcTemp(0, func.ReadTemperaturet(aIn0, coefficients, windowSize)) + 
                func.CalcTemp(1, func.ReadTemperaturet(aIn1,coefficients, windowSize)) + 
                func.CalcTemp(2, func.ReadTemperaturet(aIn2, coefficients, windowSize))) / 3;
            if (temp > roomTemp)
            {
                backgroundWorker3.RunWorkerAsync();
            }
            else
            {
                dOut.WriteData(0);                                                                                                                      //* When current temperature is below Room temperature, turn fan 'Off' *\\
                GUISettings(false, textBox5, "Fan"); GUISettings(false, textBox4, "Heat");                                                                                                                                                                                                              // textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;                                                             //*                                                                    *\\
                reseting = false;
            }
        }

        //load form
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
