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
using System.Diagnostics;





namespace _3._5versA2
{
    public partial class Form1 : Form
    {                                                                           //\/\/\// Global Variables //\/\/\/\//
                                                                            //*    Specifications for thermistors     *\\
                                                                            //*     [sensor 0, sensor1, sensor2]      *\\
        int[] r = { 10000, 5000, 100000 };                                  //*     ~~ Resistance @ 25 degrees ~~     *\\
        int[] B = { 3380, 3960, 4380 };                                     //*            ~~ B constant ~~           *\\

        int closingcount;                                                   //*  Count for times form tries to close  *\\
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
        double count = 0;                                                   //*         Data Collected Counter        *\\

        List<double> coefficients = new List<double>();                     //*           Filter Coefficients         *\\

        bool On;                                                            //*             System On/Off             *\\                    
        bool sensor0;                                                       //*           Sensor 0 On/Off             *\\
        bool sensor1;                                                       //*           Sensor 1 On/Off             *\\
        bool sensor2;                                                       //*           Sensor 2 On/Off             *\\
        bool reseting = false;                                              //*        System setting Yes/No          *\\

        string dev;                                                         //*             Device Number             *\\    
        string ParameterPath = @"H:\Project-2-313\3-6-19\3.5versA2\Parameters.txt"; //*   Parameter.txt path          *\\
        string tempPath = @"H:\Project-2-313\3-6-19\3.5versA2\Temperatures.txt";    //*  Temperatures.txt path        *\\
                                                                            //*      Channels for each thermistor     *\\
        AnalogI aIn0 = new AnalogI();                                       //*             Sensor 0 Channel          *\\
        AnalogI aIn1 = new AnalogI();                                       //*             Sensor 1 Channel          *\\
        AnalogI aIn2 = new AnalogI();                                       //*             Sensor 2 Channel          *\\
        DigitalO dOut = new DigitalO();                                     //*  Access functions to control chamber  *\\
        Functions func = new Functions();                                   //*         Access helper functions       *\\
                                                                            //\/\/\// Global Variables //\/\/\/\//
        Stopwatch Stopwatch = new Stopwatch();





        public Form1()
        {
                                                          //\/\/\/\/\/\/\/\\ User Input //\/\/\/\/\/\/\//
            Form3 input = new Form3();                 //*   Open User Form to input the device number   *\\
            input.ShowDialog();                        //*        for the temperature chamber            *\\
            dev = input.ReadDeviceNumber();            //*       Read Device Number from form            *\\
                                                          //\/\/\/\/\/\/\/\\ User Input //\/\/\/\/\/\/\//
                                                                                         
            InitializeComponent();                                                                             //\/\/\/\/\/\/\/\/\/\/\// Initialization //\/\/\/\/\/\/\/\/\/\/\\             
            try
            {
                                                                                                            //*               Opening Channels for Thermistor sensors          *\\
                aIn0.OpenChannel("0", "Ainport0", dev);                                                     //*                          Sensor 0 Channel                      *\\
                aIn1.OpenChannel("1", "Ainport1", dev);                                                     //*                          Sensor 1 Channel                      *\\
                aIn2.OpenChannel("2", "Ainport2", dev);                                                     //*                          Sensor 2 Channel                      *\\        
                dOut.OpenChannel(dev);                                                                      //*                        Open Digital Channel                    *\\
                On = false;                                                                                 //*                    Set System initially to 'Off'               *\\
                dOut.WriteData(0);                                                                          //*                Set Fan and Heater initally to  'Off'           *\\  
                GUISettings(false, textBox5, "Fan"); GUISettings(false, textBox4, "Heat");                  //*        Change GUI to reflect status of the fan and heater      *\\
                sensor0 = true; sensor1 = true; sensor2 = true;                                             //*                   Set all sensors initally to 'On'             *\\

                coefficients = func.ReadParameters(ParameterPath);                                          //*               Read Filter Coefficients from text file          *\\
                windowSize = func.ReadWindowSize(ParameterPath);                                            //*                Read Filter Window Size from text file
                High = func.ReadHighBand(ParameterPath);                                                    //*               Read Controller High Band from text file         *\\
                Low = func.ReadLowBand(ParameterPath);                                                      //*               Read Controller Low Band from text file          *\\
                                                                                                            //*                    Show initial parameters on GUI              *\\
                for ( int i=0; i<coefficients.Count; i++)
                {
                    listBox1.Items.Add(coefficients.ElementAt(i).ToString());                               //*                          Filter coefficients                   *\\
                }
                textBox10.Text = windowSize.ToString();                                                     //*                             Window size                        *\\
                textBox11.Text = High.ToString();                                                           //*                              High Band                         *\\
                textBox12.Text = Low.ToString();                                                            //*                               Low Band                         *\\             

                roomTemp = (func.CalcTemp(0, func.ReadTemperaturet(aIn0, coefficients, windowSize)) +       //*                      Calculate Room Temperature                *\\
                            func.CalcTemp(1, func.ReadTemperaturet(aIn1,coefficients, windowSize)) + 
                            func.CalcTemp(2, func.ReadTemperaturet(aIn2,coefficients, windowSize))) / 3;
                userTemp = Convert.ToDouble(numericUpDown1.Value) + roomTemp;                               //* Set desired temperature as User Input (2-5) + room temperature *\\
                textBox7.Text = userTemp.ToString(); textBox13.Text = roomTemp.ToString(); 
            }
            catch (NationalInstruments.DAQmx.DaqException){                                                 //*                 If no device number entered by user            *\\
                if (dev == "Nothing")
                {
                    Form1_Load(this, null);                                                                 //*                      Load the form and close it                *\\
                    this.Close();
                }
            }
        }
        //\/\/\/\/\/\/\/\/\/\// Helper Functions //\/\/\/\/\/\/\/\/\/\\
        /*                                                                                                                 
      Helper Function: GUISettings                                                                                    
      Changes the status of the fan and heater on the GUI
      to reflect the current status.
          Input: Item ~ true -> the fan/heater is on
                        false -> the fan/heater is off                                                         
                 textbox ~ the corresponding textbox 
                           for the fan or heater
                 item ~ is the string fan or heater
      */
        private void GUISettings(bool Item, TextBox textbox, string item)
        {
            if (textbox.InvokeRequired)                                                         //* If accessing the GUI from another thread(background worker) *\\
            {
                if (Item)                                                                       //* If on*\\
                {
                    textbox.Invoke((MethodInvoker)delegate { textbox.Text = item + " On"; });   //* Write on in textbox and change textbox to green*\\
                    textbox.Invoke((MethodInvoker)delegate { textbox.BackColor = Color.DarkSeaGreen; });
                }
                else                                                                            //* If off*\\
                {
                    textbox.Invoke((MethodInvoker)delegate { textbox.Text = item + " Off"; });  //* Write off in textbox and change textbox to red*\\
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

          /*                                                                                                                 
      Helper Function: SetParameters                                                                                   
      Sets the parameters for the filter and controller
      every time they are changed and updates GUI.
          Input: cs ~ list of filter coefficients                                                         
                 wind ~ window size
                 Hi ~ Controller high band
                 Lo ~ Controller low band
      */
        public void SetParameters(List<double>cs, int wind, double Hi, double Lo)
        {
            //* Assign new parameters *\\
            this.coefficients = cs; this.windowSize = wind; this.High = Hi; this.Low = Lo;
            //* Write new parameters   *\\
            listBox1.Items.Clear();
            for (int i = 0; i < coefficients.Count; i++)
            {
                listBox1.Items.Add(coefficients.ElementAt(i).ToString());
            }
            textBox10.Text = windowSize.ToString(); textBox11.Text = High.ToString(); textBox12.Text = Low.ToString();
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
        private void button1_Click(object sender, EventArgs e)                            
        {                                                                                                                                                   
            DigitalO dOut = new DigitalO();                                                            
            dOut.OpenChannel(dev);                                                                    
            On = !On;                                       //*   Toggle the button  'On' or 'Off'                 *\\                                                                                                              
            if (On)                                         //*   If turning the system on                     *\\
            {
                backgroundWorker1.RunWorkerAsync();         //* Turn Controller on *\\                                                               
            }                                                                                   
            else                                            //*        If turning the system off                     *\\
            {     
                backgroundWorker3.RunWorkerAsync();         //* Cool down system    *\\
                On = false;                                      
            }                                                                                    
        }                                                                                     
                                                                                                                                                            
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
        private void timer1_Tick(object sender, EventArgs e)                                       
        {
            Stopwatch.Reset();
            Stopwatch.Start();
            volt0Weighted = func.ReadTemperaturet(aIn0,coefficients, windowSize); //*     Get weighted average of voltage for each sensor    *\\
            volt1Weighted = func.ReadTemperaturet(aIn1,coefficients, windowSize);
            volt2Weighted = func.ReadTemperaturet(aIn2,coefficients, windowSize);          
            if (On || reseting)                                                         //*                     If system is on or cooling                    *\\
            {                                                                                                                               //*                                                        *\\                                                                                                                            //*                                                        *\\
                timerticks += 1;                                                   //*                  Increment timerticks                  *\\
                int tally = 0;       
                count += 0.1;
                string[] data = { "", "", "" };                        
                if (sensor0)                                                 //*                    If sensor activate                  *\\
                {                                                             
                    data = func.sensorCalc(volt0Weighted, 0, data);               //*               Calculate sensor temperature and store in array            *\\
                    tally += 1;                                            //*            Store sensor temperature in array           *\\
                }                                                    
                if (sensor1)                                                     //*                    If sensor activate                  *\\
                {                                                 
                    data = func.sensorCalc(volt0Weighted, 1, data);                 //*               Calculate sensor temperature and store in array             *\\
                    tally += 1;                                         
                }                                    
                if (sensor2)                                    //*                    If sensor activate                  *\\
                {                                                              
                    data = func.sensorCalc(volt0Weighted, 2, data);                         //*               Calculate sensor temperature and store in array            *\\
                    tally += 1;                               
                }   
                
                temp = func.CalcAvgTemp(data, tally);                      //*                 Calculate average temperature of active sensors               *\\
                func.WriteTemperature(tempPath, temp, data, count);           //*          Write temperate data in text file          *\\

                if (timerticks == 5)               //*                     If 5 ticks pass                    *\\
                {                       
                    timerticks = 0; //*reset timerticks to 0 *\\
                    if (sensor0)    //*                    If sensor active                  *\\
                    {             
                        textBox1.Text = data[0];                   //*           Store sensor temperature in textbox          *\\
                    }                          
                    if (sensor1)          //*                    If sensor active                  *\\
                    {                       
                        textBox2.Text = data[1];  //*           Store sensor temperature in textbox          *\\
                    }                                        
                    if (sensor2)                       //*                    If sensor activate                  *\\
                    {                                                                                                 //*                                                        *\\
                        textBox3.Text = data[2];                                          //*           Store sensor temperature in textbox          *\\
                    }   
                     
                    if (tally == 0)                  
                    {                            
                        textBox6.Text = "No Sensors Read";    //*   Display 'No Sensors Read' if no sensors are active   *\\  
                    }                                                                                                           
                    else                                        
                    {                                                                                                            
                        textBox6.Text = temp.ToString();     //*     Otherwise, display average temperature in GUI      *\\                            
                    } 
                }
                Stopwatch.Stop();
                TimeSpan ts = Stopwatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
            }
        }                        

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

                if (error > High)
                   {
                       dOut.WriteData(0);
                       dOut.WriteData(1);
                    Console.WriteLine(temp);
                    GUISettings(true, textBox5, "Fan");
                    GUISettings(false, textBox4, "Heat");
                }
                else if (error < Low)
                {
                    dOut.WriteData(0);
                    dOut.WriteData(2);
                    Console.WriteLine(temp);
                    GUISettings(false, textBox5, "Fan");
                    GUISettings(true, textBox4, "Heat");
                }
                else 
                   {
                       dOut.WriteData(0);
                    Console.WriteLine(temp);
                    GUISettings(false, textBox5, "Fan");
                    GUISettings(false, textBox4, "Heat");
                }

                if (!On)
                {
                    dOut.WriteData(0);
                    GUISettings(false, textBox5, "Fan");
                    GUISettings(false, textBox4, "Heat");
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
                if (On)
                {
                    MessageBox.Show("Fool, the controller is still on. Do you want me to get heatstroke?", "S.I.S.T.E.M: ", MessageBoxButtons.OK);
                    e.Cancel = true;
                }
                else
                {
                    closingcount += 1;
                    MessageBox.Show(func.ClosingForm1(reseting, closingcount), "S.I.S.T.E.M: ", MessageBoxButtons.OK);
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
        }


        //fan on after turn off
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            dOut.WriteData(0); //*turn everything off*\\
            dOut.WriteData(1);            //*                           Keep  fan on                             *\\
            reseting = true; //* set resetting to true*\\
            GUISettings(true, textBox5, "Fan"); GUISettings(false, textBox4, "Heat"); //* update GUI*\\
        } 
        
        //cool down
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
                dOut.WriteData(0);                                //* When current temperature is below Room temperature, turn fan 'Off' *\\
                GUISettings(false, textBox5, "Fan"); GUISettings(false, textBox4, "Heat"); //* update GUI*\\
                reseting = false; //* set reset to false *\\
            }
        }

        //load form
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
