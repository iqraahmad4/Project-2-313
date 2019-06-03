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
        double[] coefficentArray = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };        //*            Filter Parameters          *\\
        double userTemp;                                                    //*           Desired Temperature         *\\
        double temp;                                                        //*           Current Temperature         *\\
        double roomTemp;                                                    //*        Initial Room Temperature       *\\
        double volt0Weighted = 0;                                           //*       Filtered Voltage: Sensor 0      *\\
        double volt1Weighted = 0;                                           //*       Filtered Voltage: Sensor 1      *\\
        double volt2Weighted = 0;                                           //*       Filtered Voltage: Sensor 2      *\\
        bool On;                                                            //*               System On/Off           *\\                    
        bool sensor0;                                                       //*              Sensor 0 On/Off          *\\
        bool sensor1;                                                       //*              Sensor 1 On/Off          *\\
        bool sensor2;                                                       //*              Sensor 2 On/Off          *\\
        string dev;                                                        //*               Device Number           *\\     
                                                                            //*      Channels for each thermistor     *\\
        AnalogI aIn0 = new AnalogI();                                       //*             Sensor 0 Channel          *\\
        AnalogI aIn1 = new AnalogI();                                       //*             Sensor 1 Channel          *\\
        AnalogI aIn2 = new AnalogI();                                       //*             Sensor 2 Channel          *\\
                                                                            //*    Specifications for thermistors     *\\
                                                                            //*     [sensor 0, sensor1, sensor2]      *\\
        int[] r = { 10000, 5000, 100000 };                                  //*     ~~ Resistance @ 25 degrees ~~     *\\
        int[] B = { 3380, 3960, 4380 };                                     //*            ~~ B constant ~~           *\\
        int count = 0;                                                      //*       Data Collected Counter          *\\
        int timerticks = 0;                                                 //*              Timer Counter            *\\
        DigitalO dOut = new DigitalO();                                         //\/\/\// Global Variables //\/\/\/\//
        int closingcount;
        string ParameterPath = @"H:\Project-2-313\3-6-19\3.5versA2\Parameters.txt";
        string tempPath = @"H:\Project-2-313\3-6-19\3.5versA2\Temperatures.txt";
        Functions func = new Functions();

                ///DEBUGGING\\\
bool clicked; bool clicked6;
int ticker = 0;
 double temp0s0 = 0;
 double temp1s1 = 0;
 double temp2s2 = 0;
 double [] diff0 = {0,0,0,0,0 };
 double[] diff1 ={0,0,0,0,0 };
 double [] diff2 = { 0, 0, 0, 0, 0 };
 bool reseting=false;
 bool Off = false;
   

                                                                                                                                                                 //*                                                                 *\\
        private void GUISettings(bool  Item, TextBox text, string item)
        {
            if (Item)
            {
                text.Text = item+" On";  text.BackColor = Color.DarkSeaGreen;
            }
            else
            {
                text.Text = item+" Off"; text.BackColor = Color.DarkSeaGreen;
            }
        }

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
                                                                                                                                                    //*               Opening Channels for Thermistor sensors          *\\
            aIn0.OpenChannel("0", "Ainport0",dev);                                                                                                  //*                          Sensor 0 Channel                      *\\
            aIn1.OpenChannel("1", "Ainport1",dev);                                                                                                  //*                          Sensor 1 Channel                      *\\
            aIn2.OpenChannel("2", "Ainport2",dev);                                                                                                  //*                          Sensor 2 Channel                      *\\        
            dOut.OpenChannel(dev);                                                                                                                  //*                        Open Digital Channel                    *\\
            On = false;                                                                                                                             //*                    Set System initially to 'Off'               *\\
                                                                                                                                                    //*                                                                *\\
            dOut.WriteData(0);                                                                                                                      //*                Set Fan and Heater initally to  'Off'           *\\  
            //GUISettings(false, textBox5, "Fan"); GUISettings(false, textBox4, "Heat");                                                              //*        Change GUI to reflect status of the fan and heater      *\\
   clicked = false; clicked6 = false;                                                                                                      //*                                                                *\\
            sensor0 = true; sensor1 = true; sensor2 = true;                                                                                         //*                   Set all sensors initally to 'On'             *\\
            coefficentArray = func.ReadParameters(ParameterPath, coefficentArray);                                                                                    //*                Read Filter Parameters from text file           *\\
                                                                                                                                                    //*                                                                *\\
                                                                                                                                               //*                  Calculate Room Temperature                    *\\
            roomTemp =  (func.CalcTemp(0, func.ReadTemperaturet(aIn0, coefficentArray)) + 
                func.CalcTemp(1, func.ReadTemperaturet(aIn1,coefficentArray)) + 
                func.CalcTemp(2, func.ReadTemperaturet(aIn2,coefficentArray))) / 3;                                                                 //*                                                                *\\
            userTemp = Convert.ToDouble(numericUpDown1.Value) + roomTemp;                                                                           //* Set desired temperature as User Input (2-5) + room temperature *\\
            textBox7.Text = userTemp.ToString(); Console.WriteLine("RT: " + roomTemp);                                                              //*                                                                *\\
        }                                                                                                                                           //*                                                                *\\
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
            {                                                                                                                                               //*                                                                    *\\
                coefficentArray = func.ReadParameters(ParameterPath, coefficentArray);                                                                                    //*                Read Filter Parameters from text file           *\\   
                backgroundWorker1.RunWorkerAsync();                                                                                                     //*                                                                    *\\
            }                                                                                                                                               //*                                                                    *\\
            else                                                                                                                                            //*                      If turning the system off                     *\\
            {                                                                                                                                               //*                                                                    *\\
                                                                                                                   //*                                                                    *\\
                Off = true;                                                                                                                                 //*                                                                    *\\
              
                backgroundWorker3.RunWorkerAsync();                                                                                                                  //*                                                                    *\\
               
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
            volt0Weighted = func.ReadTemperaturet(aIn0,coefficentArray);
            volt1Weighted = func.ReadTemperaturet(aIn1,coefficentArray);
            volt2Weighted = func.ReadTemperaturet(aIn2,coefficentArray);          //*     Get weighted average of voltage for each sensor    *\\
            if (On || reseting||clicked||!clicked)                                                                                                    //*                     If system is on                    *\\
            {                                                                                                                               //*                                                        *\\                                                                                                                            //*                                                        *\\
                timerticks += 1;                                                                                                            //*                  Increment timerticks                  *\\
                int tally = 0;                                                                                                              //*                                                        *\\
                if (timerticks == 5)                                                                                                        //*                     If 5 ticks pass                    *\\
                {                                                                                                                           //*                                                        *\\
                    count += 1;                                                                                                             //*               Increment data record number             *\\
                    timerticks = 0;                                                                                                         //*                  Reset timerticks to 0                 *\\
                    string[] data = { "", "", "" };                                                                                         //*                                                        *\\
                    if (sensor0){}                                                                                                          //*                    If sensor activate                  *\\
                    {                                                                                                                       //*                                                        *\\
                        data = func.sensorCalc(volt0Weighted, 0, data);                                                                       //*               Calculate sensor temperature             *\\
                        textBox1.Text = data[0];                                                                                   //*           Store sensor temperature in textbox          *\\
                        tally += 1;                                                           //*            Store sensor temperature in array           *\\
                    }                                                                                                                       //*                                                        *\\
                    if (sensor1)                                                                                                            //*                    If sensor activate                  *\\
                    {                                                                                                                       //*                                                        *\\
                        data = func.sensorCalc(volt0Weighted, 1, data);                                                                      //*               Calculate sensor temperature             *\\
                                                                                                                                                                                                                                            //*                                                        *\\
                        textBox2.Text = data[1];                                                                                   //*           Store sensor temperature in textbox          *\\
                        tally += 1;                                                                                   //*            Store sensor temperature in array           *\\
                    }                                                                                                                       //*                                                        *\\
                    if (sensor2)                                                                                                            //*                    If sensor activate                  *\\
                    {                                                                                                                       //*                                                        *\\
                        data = func.sensorCalc(volt0Weighted, 2, data);                                                                        //*               Calculate sensor temperature             *\\
                                                                                                                                               //double temp2 = 24.67488596;                                                                                                             //*                                                        *\\
                        textBox3.Text = data[2];                                                                                   //*           Store sensor temperature in textbox          *\\
                        tally += 1;                                                                                //*            Store sensor temperature in array           *\\
                    }                                                                                                                       //*                                                        *\\
                                                                                                                                            //*                                                        *\\

                                                                                                                         //*      Average temperature = sum of data divided by      *\\
                    temp = func.CalcAvgTemp(data, tally);                                                                             //*                 Number of active sensors               *\\
                    if (tally == 0)                                                                                                         //*                                                        *\\
                    {                                                                                                                       //*   Display 'No Sensors Read' if no sensors are active   *\\ 
                        textBox6.Text = "No Sensors Read";                                                                                  //*                                                        *\\
                    }                                                                                                                       //*                                                        *\\
                    else                                                                                                                    //*                                                        *\\
                    {                                                                                                                       //*     Otherwise, display average temperature in GUI      *\\
                                                                                                                                            //*                                                        *\\
                        textBox6.Text = temp.ToString();                                                                                    //*                                                        *\\
                    }                                                                                                                       //*                                                        *\\
                                                                                                                                            //*                                                        *\\
                    func.WriteTemperature(tempPath, temp, data, count);                                                                     //*                                                        *\\
                }                                                                                                                           //*                                                        *\\
            }                                                                                                                               //*                                                        *\\
        }                                                                                                                                   //*                                                        *\\

        // When pressed, the corresponding sensor will toggle from being on or off. 
        // This will determine which sensor values will be considered according to 
        // the user interface. Sensor variables are defaulted to true and can be 
        // changed by pressing the corresponding button.
        private void button2_Click(object sender, EventArgs e)
        {
            sensor0 = !sensor0;
            if (sensor0)
            {
                textBox1.BackColor = Color.Honeydew;
            }
            else
            {
                textBox1.BackColor = Color.MistyRose;
                textBox1.Clear();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sensor1 = !sensor1;
            if (sensor1)
            {
                textBox2.BackColor = Color.Honeydew;
            }
            else
            {
                textBox2.BackColor = Color.MistyRose;
                textBox2.Clear();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sensor2 = !sensor2;
            if (sensor2)
            {
                textBox3.BackColor = Color.Honeydew;
            }
            else
            {
                textBox3.BackColor = Color.MistyRose;
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
            Form2 popup = new Form2();
            popup.Show();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!double.IsNaN(temp))
               {
                double error = temp - userTemp;
                Console.Write(temp + " " + userTemp + " " + error);
                double high =  0.1;
                double low = - 0.1;


                if (error > high)
                   {
                       dOut.WriteData(0);
                       dOut.WriteData(1);
                    Console.WriteLine(temp);
                    //GUISettings(true, textBox5, "Fan");
                    //GUISettings(false, textBox4, "Heat");
                    //  textBox5.Text = "Fan On"; textBox4.Text = "Heater Off"; 
                    //textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.DarkSeaGreen;
                }
                else if (error < low)
                {
                    dOut.WriteData(0);
                    dOut.WriteData(2);
                    Console.WriteLine(temp);
                    //GUISettings(false, textBox5, "Fan");
                    //GUISettings(true, textBox4, "Heat");
                    //  textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off"; 
                    //textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
                }
                else 
                   {
                       dOut.WriteData(0);
                    Console.WriteLine(temp);
                    //GUISettings(false, textBox5, "Fan");
                    //GUISettings(false, textBox4, "Heaat");
                    //textBox5.Text = "Fan On"; textBox4.Text = "Heater On"; 
                    //textBox4.BackColor = Color.DarkSeaGreen; textBox5.BackColor = Color.DarkSeaGreen;
                }

                if (!On)
                {
                    dOut.WriteData(0);
                    //GUISettings(false, textBox5, "Fan");
                    //GUISettings(false, textBox4, "Heaat");
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




        ///DEBUGGING\\\
        //manually turn shit off
        private void button5_Click(object sender, EventArgs e)
        {
            clicked = !clicked;
            if (clicked)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"H:\313 Project version 2\Project-2-313-master\3.5versA2\Temperatures.txt", true))  //*      Write sensor data to text file 'Temperatures'     *\\
                {                                                                                                                       //*                                                        *\\
                    file.WriteLine("Heat Rate: ");                  //*                                                        *\\
                }
                dOut.WriteData(2);
                textBox6.Text = temp.ToString();
                textBox4.Text = "Heat On";
                textBox4.BackColor = Color.DarkSeaGreen;
                textBox5.Text = "Fan Off";
                textBox5.BackColor = Color.Firebrick;
            }
            if (!clicked)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"H:\313 Project version 2\Project-2-313-master\3.5versA2\Temperatures.txt", true))  //*      Write sensor data to text file 'Temperatures'     *\\
                {                                                                                                                       //*                                                        *\\
                    file.WriteLine("Cooling Rate: ");                  //*                                                        *\\
                }
                dOut.WriteData(3); textBox5.Text = "Fan On"; textBox4.Text = "Heater On"; textBox4.BackColor = Color.DarkSeaGreen; textBox5.BackColor = Color.DarkSeaGreen;
            }
            //Console.WriteLine("Heat Fan Off");
        }


        //manually turn shit off/on
        private void button6_Click(object sender, EventArgs e)
        {
            DigitalO dOut = new DigitalO();
            dOut.OpenChannel(dev);
            clicked6 = !clicked6;

            if (!clicked6)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"H:\313 Project version 2\Project-2-313-master\3.5versA2\Temperatures.txt", true))  //*      Write sensor data to text file 'Temperatures'     *\\
                {                                                                                                                       //*                                                        *\\
                    file.WriteLine("Done: ");                  //*                                                        *\\
                }
                dOut.WriteData(1);
                textBox5.Text = "Fan On"; textBox4.Text = "Heater Off"; textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.DarkSeaGreen;
            }
            else
            {
                dOut.WriteData(0); textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off"; textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
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

        

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
                    //*                                                                    *\\
                           //*                    Calculate current temperature                   *\\                                                                    *\\
            Console.WriteLine("temp:" + temp + "> RT:" + roomTemp);                                                                                     //*                                                                    *\\
                                                                                                                                                   //*                                                                    *\\
                dOut.WriteData(0);
                dOut.WriteData(1);                                                                                                                      //*                           Keep  fan on                             *\\
                reseting = true;
            //GUISettings(true, textBox5, "Fan");  GUISettings(false, textBox4, "Heat");            //*                                                                    *\\
               // textBox5.Text = "Fan On"; textBox4.Text = "Heater Off"; textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.DarkSeaGreen;  //*         Change GUI to reflect status of the fan and heater         *\\
                                                                                                                                                      //*                                                                    *\\
                                                                                                                                                        //*                                                                    *\\                
                                                                                                                                                        //*                                                                    *\\
                                                                                                                                                        //Console.WriteLine("Heat & Fan Off");                                                                                                  //*                                                                    *\\
         
        }

        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            temp = (func.CalcTemp(0, func.ReadTemperaturet(aIn0, coefficentArray)) + 
                func.CalcTemp(1, func.ReadTemperaturet(aIn1,coefficentArray)) + 
                func.CalcTemp(2, func.ReadTemperaturet(aIn2, coefficentArray))) / 3;
            if (temp > roomTemp)
            {
                backgroundWorker3.RunWorkerAsync();
            }
            else
            {
                dOut.WriteData(0);                                                                                                                      //* When current temperature is below Room temperature, turn fan 'Off' *\\
                //GUISettings(false, textBox5, "Fan"); GUISettings(false, textBox4, "Heat");                                                                                                                                                                                                              // textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;                                                             //*                                                                    *\\
                reseting = false;
            }
        }
    }
}
