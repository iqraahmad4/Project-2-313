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
        double[] coefficentArray = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };                                //*               Filter Parameters                   *\\
        double userTemp;                                                                                         //*              Desired Temperature             *\\
        double temp;                                                                                                //*               Current Temperature            *\\
        double roomTemp;                                                                                       //*          Initial Room Temperature          *\\
        double volt0Weighted = 0;                                                                          //*          Filtered Voltage: Sensor 0          *\\
        double volt1Weighted = 0;                                                                          //*          Filtered Voltage: Sensor 1          *\\
        double volt2Weighted = 0;                                                                          //*          Filtered Voltage: Sensor 2          *\\
        bool On;                                                                                                       //*                   System On/Off                  *\\                    
        bool sensor0;                                                                                               //*                  Sensor 0 On/Off                 *\\
        bool sensor1;                                                                                               //*                  Sensor 1 On/Off                 *\\
        bool sensor2;                                                                                               //*                  Sensor 2 On/Off                 *\\
        string dev;;                                                                                                   //*                   Device Number                 *\\     
                                                                                                                            //*       Channels for each thermistor        *\\
        AnalogI aIn0 = new AnalogI();                                                                    //*                 Sensor 0 Channel                 *\\
        AnalogI aIn1 = new AnalogI();                                                                    //*                 Sensor 1 Channel                 *\\
        AnalogI aIn2 = new AnalogI();                                                                    //*                 Sensor 2 Channel                 *\\


                                                                                                                           //*       Specifications for thermistors        *\\
                                                                                                                           //*         [sensor 0, sensor1, sensor2]         *\\
        int[] r = { 10000, 5000, 100000 };                                                                //*     ~~ Resistance @ 25 degrees ~~     *\\
        int[] B = { 3380, 3960, 4380 };                                                                     //*                 ~~ B constant ~~                *\\
        int count = 0;                                                                                              //*            Data Collected Counter            *\\
        int timerticks = 0;                                                                                       //*                    Timer Counter                   *\\
        DigitalO dOut = new DigitalO();                                                                         //\/\/\// Global Variables //\/\/\/\//
                                                          
                ///DEBUGGING\\\
bool clicked;
int ticker = 0;
 double temp0s0 = 0;
 double temp1s1 = 0;
 double temp2s2 = 0;
 double [] diff0 = {0,0,0,0,0 };
 double[] diff1 ={0,0,0,0,0 };
 double [] diff2 = { 0, 0, 0, 0, 0 };
 bool reseting=false;
 bool Off = false;

                                                                                                                                        //\/\/\// Helper Functions //\/\/\/\//
/*
Helper Function: ReadTemperature 
Reads voltage from a sensor and applies filter parameters 
to rolled averages of voltage to output a weighted average.
    Input: aIn ~ Analog Channel for Sensor. 
    Output: voltWeighted ~  Weighted Aaverage Voltage.
*/
        private double ReadTemperaturet(AnalogI aIn)
        {
            double[] volt = aIn.ReadData();                                                 //*               Read Rolled Average Voltages from Analog File            *\\
            for (int i = 0; i < 6; i++)                                                             //*                 For each element in the Rolled Voltage Array              *\\
            {
                volt[i] = volt[i] * coefficentArray[i];                                       //*    Multiply average wth the corresponding weight co-efficient    *\\
                voltWeighted += volt[i];                                                       //*                       Sum up weighted voltage averages                        *\\
            }
            return voltWeighted;
        }

/*
Helper Function: CalcTemp 
Calculates temperature for a sensor based on its beta constant
and its value of resistance at 25 degrees Celcius and voltage
of the sensor. 
Uses equation T = B / (ln( (R0 * V/(5-V)) / R0* e^(-B/T0))).
Here, T0 = 25 degrees Celcius = 298 degrees Kelvin.
    Input: sensorNumber ~ Sensor Index in Resistance Array and B-Constant Array,
              volt ~ Voltage at the Sensor.
    Output: temp ~  Calculated Temperature.
*/
        private double CalcTemp(int sensorNumber, double volt)
        {                                                                                                                    //*        Substitute all variables into equation       *\\
            double temp = B[sensorNumber] / (Math.Log((r[sensorNumber]*(volt / (5 - volt))) / (r[sensorNumber] * Math.Exp(-B[sensorNumber] / 298.15))));
            temp = temp - 273.15;                                                                            //*   Convert from Kelvin to Celcius: C=K-273.15  *\\
            return temp;
        }


        public Form1()
        {
            
            // Setting 'On' to false inside the  so that system is switched off when it starts.
            Form3 input = new Form3();                                                                        //*   Open User Form to input the device number   *\\
            input.ShowDialog();                                                                                     //*              for the temperature chamber                 *\\
            dev = input.ReadDeviceNumber();                                                             //*             Read Device Number from form               *\\

            InitializeComponent();                                                                               //*                   Initializing all components                    *\\
                                                                                                                              //*      Opening Channels for Thermistor sensors         *\\
            aIn0.OpenChannel("0", "Ainport0",dev) ;aIn1.OpenChannel("1", "Ainport1",dev);  aIn2.OpenChannel("2", "Ainport2",dev);                 
            dOut.OpenChannel(dev);                                                                          //*                        Open Digital Channel                       *\\
            On = false;                                                                                                //*                    Set System initially to 'Off'                    *\\
 //Console.WriteLine("Heat&Fan Off");
            dOut.WriteData(0);                                                                                   //*               Set Fan and Heater initally to  'Off'             *\\
                                                                                                                             //* Change GUI to reflect status of the fan andheater   *\\
            textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off"; textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
 clicked = false;                                                                                                   
            sensor0 = true; sensor1 = true; sensor2 = true;                                       //*                   Set all sensors initally to 'On'                  *\\
                                                                                                                             //* Read Filter Parameters from text file   *\\
            using (System.IO.StreamReader parameters = new System.IO.StreamReader(@"H:\Project-2-313\3.5versA2\Parameters.txt"))
            {
                int lineCounter = 0;
                string line;

                while ((line = parameters.ReadLine()) != null)
                {
                    coefficentArray[lineCounter] = Convert.ToDouble(line);              //* Enter each weightage co-efficient into Coefficient Array *\\
                    lineCounter++;
                }
                parameters.Close();
                lineCounter = 0;
            }                                                               
                                                                                                                                 //*                         Calculate Room Temperature                                 *\\
            roomTemp = (CalcTemp(0, ReadTemperaturet(aIn0)) + CalcTemp(1, ReadTemperaturet(aIn1)) + CalcTemp(2, ReadTemperaturet(aIn2))) / 3; 
 //roomTemp = 25;
            userTemp = Convert.ToDouble(numericUpDown1.Value) + roomTemp;   //* Set desired temperature as User Input (2-5) + room temperature  *\\
            textBox7.Text = userTemp.ToString();
        }

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
            On = !On;                                                                                                //*                Toggle the button  'On' or 'Off'                *\\

            if (On)                                                                                                      //*                    If turning the system on                        *\\
            {                                                                                                               //*             Read Filter Parameters from text file            *\\
                using (System.IO.StreamReader parameters = new System.IO.StreamReader(@"H:\Project-2-313\3.5versA2\Parameters.txt"))
                {

                    int lineCounter = 0;
                    string line;
                    while ((line = parameters.ReadLine()) != null)
                    {
                        coefficentArray[lineCounter] = Convert.ToDouble(line);           //*        Enter each weight coefficient into Coefficient Array         *\\
                        lineCounter++;
                    }
                    parameters.Close();
                    lineCounter = 0;
                }
            }
            else                                                                                                        //*                     If turning the system off                                       *\\
            {
                //Console.WriteLine("Heat & Fan Off");
                Off = true;
                                                                                                                         //*                            Calculate current temperature                         *\\
                temp = (CalcTemp(0, ReadTemperaturet(aIn0)) + CalcTemp(1, ReadTemperaturet(aIn1)) + CalcTemp(2, ReadTemperaturet(aIn2))) / 3;
                Console.WriteLine("temp:" + temp + "> RT:" + roomTemp);
                while (temp > roomTemp)                                                              //* While  current temperature is higher than  room temperature   *\\
                {
                    dOut.WriteData(1);                                                                     //*                                          Keep  fan on                                          *\\
                    reseting = !reseting;
                                                                                                                         //*        Change GUI to reflect status of the fan andheater               *\\
                    textBox5.Text = "Fan On"; textBox4.Text = "Heater Off"; textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.DarkSeaGreen;
                                                                                                                         //*                            Calculate current temperature                         *\\
                    temp = (CalcTemp(0, ReadTemperaturet(aIn0)) + CalcTemp(1, ReadTemperaturet(aIn1)) + CalcTemp(2, ReadTemperaturet(aIn2))) / 3;
                    Console.WriteLine("temp:" + temp + "> RT:" + roomTemp);        
                }
                

                    //Console.WriteLine("Heat & Fan Off");
                    dOut.WriteData(0);                                                                          //* When current temperature is below Room temperature, turn fan 'Off'  *\\
                                                                                                                              //* Change GUI to reflect status of the fan andheater   *\\
                    textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off";
                    textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
                    reseting = !reseting;


                

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
                                                                                                                                          //* Change GUI to reflect status of the fan andheater   *\\
            //here we need to get array of 5 and for each value we apply the coeffiecent the after applying coefficient we average them again to get the single volt measurement
            volt0Weighted = ReadTemperaturet(aIn0);                                                          //* Get weighted average of voltage for each sensor    *\\
            volt1Weighted = ReadTemperaturet(aIn1); 
            volt2Weighted = ReadTemperaturet(aIn2);
 //volt0Weighted = ReadTemperaturet(sensor0reading);
 //volt1Weighted = ReadTemperaturet(sensor1reading);
 //volt2Weighted = ReadTemperaturet(sensor2reading);
            if (On || reseting) 
            {
                
                timerticks += 1;
                int tally = 0;
                if (timerticks == 5)
                {
                    count += 1;
                    timerticks = 0;
                    string[] data = { "", "", "" };
                    if (sensor0)
                    {
                        double temp0 = CalcTemp(0, volt0Weighted);
  //double temp0 = 25.234546;
                        textBox1.Text = temp0.ToString();
                        data[0] = temp0.ToString();
                    }
                    if (sensor1)
                    {
                        double temp1 = CalcTemp(1, volt1Weighted);
   //double temp1 = 26.12349807;
                        textBox2.Text = temp1.ToString();
                        data[1] = temp1.ToString();
                    }
                    if (sensor2)
                    {
                        double temp2 = CalcTemp(2, volt2Weighted);
  //double temp2 = 24.67488596;
                        textBox3.Text = temp2.ToString();
                        data[2] = temp2.ToString();
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        //Console.Write(" " + data[i]);

                        if (data[i] == "")
                        {
                            data[i] = "0";
                        }
                       
                        else
                        {
                            int index = data[i].LastIndexOf(".");
                           data[i] = data[i].Substring(0, index + 5);
                            tally += 1;
                        }
                    }
                    temp = (Convert.ToDouble(data[0]) + Convert.ToDouble(data[1]) + Convert.ToDouble(data[2])) / tally;
                    if (tally == 0)
                    {
                        textBox6.Text = "No Sensors Read";
                    }
                    else
                    {  
                        textBox6.Text = temp.ToString();
                        int indexT = temp.ToString().LastIndexOf(" ");
                        //Console.WriteLine(" temp is:-" + temp);
                        if (temp.ToString().Length-temp.ToString().IndexOf(".")-1>5)
                        {
                               temp = Convert.ToDouble(temp.ToString().Substring(0, indexT + 5));
                            //Console.WriteLine(temp);
                        }
                    }
                    
                    //Console.WriteLine("tally: " + tally + ": " + data[0] + " " + data[1] + " " + data[2]);
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"H:\Project-2-313\3.5versA2\Temperatures.txt", true))
                    {
                        file.WriteLine ("\n"+ count.ToString() + "\t"+data[0] + "\t"+data[1] + "\t"+ data[2] + "\t"+temp);
                    }
                }
            }
        }

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

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (On)
            {
                ///DEBUGGING\\\
                temp = (CalcTemp(0, ReadTemperaturet(aIn0)) + CalcTemp(1, ReadTemperaturet(aIn1)) + CalcTemp(2, ReadTemperaturet(aIn2))) / 3;
                textBox6.Text = temp.ToString();
                // temp = (CalcTemp(0, ReadTemperaturet(sensor0reading)) + CalcTemp(1, ReadTemperaturet(sensor1reading)) + CalcTemp(2, ReadTemperaturet(sensor2reading))) / 3;
                ///NOT DEBUGGING\\\

                if (temp > userTemp + 0.24 && temp < userTemp + 0.25)
                {
                    // Console.WriteLine("Heat &  Fan On");
                    dOut.WriteData(3);
                    textBox4.Text = "Heat On";
                    textBox4.BackColor = Color.DarkSeaGreen;
                    textBox5.Text = "Fan On";
                    textBox5.BackColor = Color.DarkSeaGreen;
                    /* for (int i=0;i<10; i++)
                     {
                      sensor0reading[i] = sensor0reading[i] + 0.15;
                      sensor1reading[i] = sensor1reading[i] + 0.15;
                      sensor2reading[i] = sensor2reading[i] + 0.15;
                     }
                      Console.WriteLine(sensor0reading[0] + " " + sensor1reading[0] + " " + sensor2reading); */
                }
                if (temp < userTemp - 0.245)
                {

 //Console.WriteLine("Heat On");
                    dOut.WriteData(2);
                    textBox4.Text = "Heat On";
                    textBox4.BackColor = Color.DarkSeaGreen;
 /*for (int i=0;i<10; i++)
 {
  sensor0reading[i] = sensor0reading[i] + 0.35;
  sensor1reading[i] = sensor1reading[i] + 0.35;
  sensor2reading[i] = sensor2reading[i] + 0.35;
 }
 Console.WriteLine(sensor0reading[0]+" " + sensor1reading[0]+" "+sensor2reading[0]);*/
                }
                
                if (temp > userTemp + 0.25)
                {
  //Console.WriteLine("Heat & Fan Off");
                    dOut.WriteData(0);
                    textBox4.Text = "Heat Off";
                    textBox4.BackColor = Color.Firebrick;
                    textBox5.Text = "Fan Off";
                    textBox5.BackColor = Color.Firebrick;
// Console.WriteLine("Fan On");
                    dOut.WriteData(1);
                    textBox5.Text = "Fan On";
                    textBox5.BackColor = Color.DarkSeaGreen;
 /* for (int i=0;i<10; i++)
 {
  sensor0reading[i] = sensor0reading[i] - 0.35;
  sensor1reading[i] = sensor1reading[i] - 0.35;
  sensor2reading[i] = sensor2reading[i] - 0.35;
 }
   Console.WriteLine(sensor0reading[0] + " " + sensor1reading[0] + " " + sensor2reading); */
                }
            }
            else
            {
               /* if (Off == true) tried to fix it didn't quite work
                {
                    temp = (CalcTemp(0, ReadTemperaturet(aIn0)) + CalcTemp(1, ReadTemperaturet(aIn1)) + CalcTemp(2, ReadTemperaturet(aIn2))) / 3;
                    if (temp > roomTemp)
                    {

                        //temp = (CalcTemp(0, ReadTemperaturet(sensor0reading)) + CalcTemp(1, ReadTemperaturet(sensor1reading)) + CalcTemp(2, ReadTemperaturet(sensor2reading))) / 3;
                        // Console.WriteLine("Fan on");
                        dOut.WriteData(1);
                        reseting = !reseting;
                        textBox5.Text = "Fan On";
                        textBox4.Text = "Heater Off";
                        textBox4.BackColor = Color.Firebrick;
                        textBox5.BackColor = Color.DarkSeaGreen;
                        temp = (CalcTemp(0, ReadTemperaturet(aIn0)) + CalcTemp(1, ReadTemperaturet(aIn1)) + CalcTemp(2, ReadTemperaturet(aIn2))) / 3;
                    }
                    else
                    {

                        //Console.WriteLine("Heat & Fan Off");
                        dOut.WriteData(0);
                        textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off";
                        textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
                        reseting = !reseting;
                       

                    }
                }*/
            }
        }

        ///DEBUGGING\\\
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (clicked)
            {
                ticker += 1;

                double volt_0 = aIn0.ReadData_O();
 //double volt_0 = sensor0reading[0];
                double temper0 = B[0] / (Math.Log((r[0] * volt_0 / (5 - volt_0)) / (r[0] * Math.Exp(-B[0] / 298))));
                //Console.WriteLine(temper0);
                temper0 = temper0 - 273;
                double volt_1 = aIn1.ReadData_O();
 //double volt_1 = sensor1reading[0];
                double temper1 = B[1] / (Math.Log((r[1] * volt_1 / (5 - volt_1)) / (r[1] * Math.Exp(-B[1] / 298))));
                // Console.WriteLine(temper1);
                temper1 = temper1 - 273;
                double volt_2 = aIn2.ReadData_O();
 //double volt_2 = sensor2reading[0];
                double temper2 = B[2] / (Math.Log((r[2] * volt_2 / (5 - volt_2)) / (r[2] * Math.Exp(-B[2] / 298))));
                // Console.WriteLine(temper2);
                temper2 = temper2 - 273;
                double temper = (temper0 + temper1 + temper2) / 3;
                double volta = (volt_0 + volt_1 + volt_2) / 3;
                textBox9.Text = volta.ToString();
                textBox8.Text = temper.ToString();

                diff0[ticker - 1] = temper0;
                diff1[ticker - 1] = temper1;
                diff2[ticker - 1] = temper2;

                if (ticker == 5)
                {
                    for (int i = 1; i < 5; i++)
                    {
                        temp2s2 += diff2[i] - diff2[i - 1];
                        temp1s1 += diff1[i] - diff1[i - 1];
                        temp0s0 += diff0[i] - diff0[i - 1];

                    }
                    Console.WriteLine("Increment per 0.1s: ");
                    Console.WriteLine("s0: " + temp0s0 + " s1: " + temp1s1 + " s2: " + temp2s2);
                    ticker = 0;
                }

            }
        }

        //manually turn shit off
        private void button5_Click(object sender, EventArgs e)
        {
          
            dOut.WriteData(1);
            textBox6.Text = temp.ToString();
            textBox4.Text = "Heat off";
            textBox4.BackColor = Color.Firebrick;
            textBox5.Text = "Fan off";
            textBox5.BackColor = Color.Firebrick;
            //Console.WriteLine("Heat Fan Off");
        }


        //manually turn shit off/on
        private void button6_Click(object sender, EventArgs e)
        {
            DigitalO dOut = new DigitalO();
            dOut.OpenChannel(dev);
            clicked = !clicked;

            if (clicked)
            {
                dOut.WriteData(3);
            }
            else
            {
                dOut.WriteData(0);
            }
        }
    }
}
