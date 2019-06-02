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
    {                                              //\/\/\// Global //\/\/\/\//
        double userTemp;                          //***desired temperature***\\
        double[] coefficentArray = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        double volt0Weighted = 0;
        double volt1Weighted = 0;
        double volt2Weighted = 0;
        //***Boolean for button pressed on/off***\\
        bool On;
 bool clicked;
        bool sensor0;
        bool sensor1;
        bool sensor2;
        string dev;

        DigitalO dOut = new DigitalO();
        //***Channels for each thermistor***\\
        AnalogI aIn0 = new AnalogI();           /**Sensor 0**/
        AnalogI aIn1 = new AnalogI();           /**Sensor 1**/
        AnalogI aIn2 = new AnalogI();           /**Sensor 2**/
 /*double[] sensor0reading = {2.4938971044,
2.5159185788,
2.4783781328,
2.4521162885,
2.4288716362,
2.4075779807,
2.4337822977,
2.4673335185,
2.4810056047,
2.5437641690,
 };
 double[] sensor1reading = {2.4313007943,
2.4417858576,
2.4283583747,
2.4436416907,
2.4425631201,
2.5180824422,
2.4798205561,
2.4036755791,
2.4560132292,
2.4360713194,
        };
 double[] sensor2reading = { 2.5321972045,
2.5055118872,
2.5249082119,
2.4897940094,
2.4272520015,
2.5164960523,
2.4636127445,
2.4104448390,
2.4829308082,
2.52712710988 }; */


        int count = 0;
        int timerticks = 0;
 int ticker = 0;
 double temp0s0 = 0;
 double temp1s1 = 0;
 double temp2s2 = 0;
 double [] diff0 = {0,0,0,0,0 };
 double[] diff1 ={0,0,0,0,0 };
 double [] diff2 = { 0, 0, 0, 0, 0 };
        
        //Specifications for thermistors\\
        int[] r = { 10000, 5000, 100000 };       //~~Resistance @ 25 degrees~~\\
                                                 /**Sensor 0**/
                                                 /**Sensor 1**/
                                                 /**Sensor 2**/
        int[] B = { 3380, 3960, 4380 };        //~~B constant~~\\
                                                 /**Sensor 0**/
                                                 /**Sensor 1**/
                                                 /**Sensor 2**/

        double temp;                            //***Current Temperature***\\
        double roomTemp;                        //***Initial Room temperature***\\

        private double ReadTemperaturet(AnalogI aIn)
 //private double ReadTemperaturet(double[] sensor)
        {
            double[] volt = aIn.ReadData();

  //double[] volt = sensor;
            double voltWeighted = 0;
            for (int i = 0; i < 6; i++)
            {
                volt[i] = volt[i] * coefficentArray[i];
                voltWeighted += volt[i];
            }
            return voltWeighted;
        }

        private double CalcTemp(int sensorNumber, double voltWeighted)
        {
            double temp = B[sensorNumber] / (Math.Log((r[sensorNumber]*(voltWeighted / (5 - voltWeighted))) / (r[sensorNumber] * Math.Exp(-B[sensorNumber] / 298.15))));
            temp = temp - 273.15;
            textBox6.Text = temp.ToString();
            return temp;
        }


        public Form1()
        {
            // *** Initializing all components and opening each channel once for each thermistor sensor.
            // Setting 'On' to false inside the  so that system is switched off when it starts.
            Form3 input = new Form3();
            input.ShowDialog();
            dev = input.ReadDeviceNumber();
            InitializeComponent();
                                                                                                                                      
                                                                                                                                                  
              Console.WriteLine(dev);
 // dev = "7";
            aIn0.OpenChannel("0", "Ainport0",dev);
            aIn1.OpenChannel("1", "Ainport1",dev);
            aIn2.OpenChannel("2", "Ainport2",dev);
            dOut.OpenChannel(dev);
            On = false;
 //Console.WriteLine("Heat&Fan Off");
            dOut.WriteData(0);
            textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off";
            textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
            sensor0 = true;
 clicked = false;
            sensor1 = true;
            sensor2 = true;
            using (System.IO.StreamReader parameters = new System.IO.StreamReader(@"H:\Project-2-313\3.5versA2\Parameters.txt"))
            {
                int lineCounter = 0;
                string line;

                while ((line = parameters.ReadLine()) != null)
                {
                    coefficentArray[lineCounter] = Convert.ToDouble(line);
                    lineCounter++;
                }
                parameters.Close();
                lineCounter = 0;
            }
            roomTemp = (CalcTemp(0, ReadTemperaturet(aIn0)) + CalcTemp(1, ReadTemperaturet(aIn1)) + CalcTemp(2, ReadTemperaturet(aIn2))) / 3;
 //roomTemp = 25;
            userTemp = Convert.ToDouble(numericUpDown1.Value) + roomTemp;
            textBox7.Text = userTemp.ToString();
        }

        // Turning System on/off. On varriable is toggled between its two settings, true/false. 
        // True--> turn system on. False--> turn system off.
        // When true, the system measures the values of the three thermistors at that interval, 
        // and calculates the average current room temperature using the algorithm 
        // T = B / (Ln ( (V / (5-V)) / (R * e ^ (-B / 25))).
        // User input is read and verified if it falls within the right range, 2 - 5 degrees above room temperature.
        // Heater is turned on and dystem reaches the desired temperature and then continues to maintain within a 0.25 range 
        // of the desired temperature through turning the heater and fan on and off until the system is switch off.
        // When false, heater is turned off, fan is turn back on until the temperature read is at room temperature again. 
        // System then turns fan off and is officially off.
        private void button1_Click(object sender, EventArgs e)
        {
            DigitalO dOut = new DigitalO();
            dOut.OpenChannel(dev);
            On = !On;

            if (On)
            {
                using (System.IO.StreamReader parameters = new System.IO.StreamReader(@"H:\Project-2-313\3.5versA2\Parameters.txt"))
                {

                    int lineCounter = 0;
                    string line;
                    while ((line = parameters.ReadLine()) != null)
                    {
                        coefficentArray[lineCounter] = Convert.ToDouble(line);
                       // Console.WriteLine(coefficentArray[lineCounter]);
                        lineCounter++;
                    }
                    parameters.Close();
                    lineCounter = 0;
                }
            }
            else
            {
 Console.WriteLine("Heat & Fan Off");
                dOut.WriteData(0);
                textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off";
                textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
                while (temp > roomTemp)
                {
                    temp = (CalcTemp(0, ReadTemperaturet(aIn0)) + CalcTemp(1, ReadTemperaturet(aIn1)) + CalcTemp(2, ReadTemperaturet(aIn2))) / 3;
 //temp = (CalcTemp(0, ReadTemperaturet(sensor0reading)) + CalcTemp(1, ReadTemperaturet(sensor1reading)) + CalcTemp(2, ReadTemperaturet(sensor2reading))) / 3;
// Console.WriteLine("Fan on");
                    dOut.WriteData(1);
                    textBox5.Text = "Fan On";
                    textBox5.BackColor = Color.DarkSeaGreen;
                }
 //Console.WriteLine("Heat & Fan Off");
                dOut.WriteData(0);
                textBox5.Text = "Fan Off"; textBox4.Text = "Heater Off";
                textBox4.BackColor = Color.Firebrick; textBox5.BackColor = Color.Firebrick;
                On = false;
            }
        }



        // Timer ticks every 0.1 seconds, frequency of 10 Hz. At each tick, the voltage data is read.
        // Every 5 ticks, or 0.5 seconds, the temperature for each voltage reading from the sensors is calculated
        // usung the algorithm written above, for the sensors the user activates, all sensors are on by default
        // Sensors whose temperatures were calculated is outputted into the corresponding textbox for that sensor 
        // and data is written into the .txt file 'Temperatures'. timerticks is reset to 0 to recount 5 ticks for 0.5 seconds.
        private void timer1_Tick(object sender, EventArgs e)
        {
            //here we need to get array of 5 and for each value we apply the coeffiecent the after applying coefficient we average them again to get the single volt measurement
            volt0Weighted = ReadTemperaturet(aIn0);
            volt1Weighted = ReadTemperaturet(aIn1);
            volt2Weighted = ReadTemperaturet(aIn2);
 //volt0Weighted = ReadTemperaturet(sensor0reading);
 //volt1Weighted = ReadTemperaturet(sensor1reading);
 //volt2Weighted = ReadTemperaturet(sensor2reading);
            if (On) 
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
                        Console.Write(" " + data[i]);

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
                        int indexT = temp.ToString().LastIndexOf(".");
                        Console.WriteLine(" temp is:-" + temp);
                        if (temp != 0)
                        {
                               temp = Convert.ToDouble(temp.ToString().Substring(0, indexT + 5));
                            Console.WriteLine(temp);
                        }
                    }
                    
                    Console.WriteLine("tally: " + tally + ": " + data[0] + " " + data[1] + " " + data[2]);
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
                dOut.WriteData(2);

            }
            else
            {
                dOut.WriteData(0);
            }
        }
    }
}
