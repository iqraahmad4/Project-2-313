using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.DAQmx;
using NationalInstruments;
using System.Windows.Forms;

namespace _3._5versA2
{
    class Functions
    {
        Task functions = new Task();
        //Form2 f2 = new Form2();                                                                                //*    Specifications for thermistors     *\\
                                                                                                                                       //*     [sensor 0, sensor1, sensor2]      *\\
        int[] r = { 10000, 5000, 100000 };                                                                                             //*     ~~ Resistance @ 25 degrees ~~     *\\
        int[] B = { 3380, 3960, 4380 };                                                                                                //*            ~~ B constant ~~           *\\
      /*                                                                                                                 
       Helper Function: ReadTemperature                                                                                     
       Reads voltage from a sensor and applies filter parameters                                                            
       to rolled averages of voltage to output a weighted average.                                                          
           Input: aIn ~ Analog Channel for Sensor.                                                                          
           Output: voltWeighted ~  Weighted Aaverage Voltage.                                                               
       */                                                                                                                   
        public double ReadTemperaturet(AnalogI aIn, List<double> coefficients, int windowSize)                                                  //*                                                           *\\
        {                                                                                                                   //*                                                           *\\
            double voltWeighted = 0;                                                                                        //*                                                           *\\
            List <double> volt = aIn.ReadData(windowSize);                                                                                 //*       Read Rolled Average Voltages from Analog File       *\\
            for (int i = 0; i < 11-windowSize; i++)                                                                                     //*        For each element in the Rolled Voltage Array       *\\
            {                                                                                                               //*                                                           *\\
                volt[i] = volt[i] * coefficients[i];                                                                        //* Multiply average wth the corresponding weight coefficient *\\
                voltWeighted += volt[i];
                /*Console.WriteLine("volt[i] " + volt[i]);                                                                    //*              Sum up weighted voltage averages             *\\
                Console.WriteLine("coefficients[i] " + coefficients[i]);
                Console.WriteLine("voltWeighted " + voltWeighted);*/
            }                                                                                                               //*                                                           *\\
            return voltWeighted;                                                                                            //*              Return weighted voltage average              *\\
        }                                                                                                                   //*                                                           *\\
        /*                                                                                                                
         Helper Function: CalcTemp                                                                                          
         Calculates temperature for a sensor based on its beta constant                                                     
         and its value of resistance at 25 degrees Celcius and voltage                                                      
         of the sensor.                                                                                                    
         Uses equation T = B / (ln( (R0 * V/(5-V)) / R0* e^(-B/T0))).                                                   
         Here, T0 = 25 degrees Celcius = 298 degrees Kelvin.                                                                
             Input: sensorNumber ~ Sensor Index in Resistance Array                                                         
                                   and B-Constant Array;                                                                    
                    volt ~ Voltage at the Sensor.                                                                           
             Output: temp ~  Calculated Temperature.                                                                        
        */                                                                                                               

        public double CalcTemp(int sensorNumber, double volt)                                                               //*                                                           *\\
        {                                                                                                                   //*           Substitute all variables into equation          *\\
            double temp = B[sensorNumber] / (Math.Log((r[sensorNumber] * (volt / (5 - volt))) /                             //*                                                           *\\
                (r[sensorNumber] * Math.Exp(-B[sensorNumber] / 298.15))));                                                  //*                                                           *\\
            temp = temp - 273.15;                                                                                           //*         Convert from Kelvin to Celcius: C=K-273.15        *\\
            return temp;                                                                                                    //*                                                           *\\
        }                                                                                                                   //*                                                           *\\
        public List<double> ReadParameters(string filepath)                                             //*                                                           *\\
        {                                                                                                                   //*                                                           *\\
            List<double> array = new List<double>();
            using (System.IO.StreamReader parameters = new System.IO.StreamReader(filepath))                                //*            Read Filter Parameters from text file          *\\
            {                                                                                                               //*                                                           *\\                                                                                       //*                                                           *\\
                string line;                                                                                                //*                                                           *\\
                while ((line = parameters.ReadLine()) != "Filter Coefficients: ")                                                              //*               Reading each line in text file              *\\
                {
                    //*                                                           *\\
                }


                while ((line = parameters.ReadLine()) != "")                                                              //*               Reading each line in text file              *\\
                {

                    array.Add(Convert.ToDouble(line));                                                         //*   Enter each weight coefficient into Coefficient Array    *\\                                                                               //*                                                           *\\

                }        //*                                                           *\\
                parameters.Close();                                                                                         //*                     Close parameter file                  *\\     
               
            }                                                                                                           //*                                                           *\\
            return array;                                                                                                   //*                   Return coefficient array                *\\
        }                                                                                                                   //*                                                           *\\

        public int ReadWindowSize(string filepath)                                             //*                                                           *\\
        {
            int size;//*                                                           *\\
            using (System.IO.StreamReader parameters = new System.IO.StreamReader(filepath))                                //*            Read Filter Parameters from text file          *\\
            {                                                                                                               //*                                                           *\\
                                                                                                     //*                                                           *\\
                string line;                                                                                                //*                                                           *\\
                while ((line = parameters.ReadLine()) != "Window Size: ")                                                              //*               Reading each line in text file              *\\
                {
                                                                                                    //*                                                           *\\
                }
                line = parameters.ReadLine();
                    //*                                                           *\\

                    size = Convert.ToInt32(line);                                                         //*   Enter each weight coefficient into Coefficient Array    *\\                                                             //*                                                           *\\
                                                                                                                       //*                                                           *\\
                parameters.Close();                                                                                         //*                     Close parameter file                  *\\                                                                                         //*                                                           *\\
            }                                                                                                               //*                                                           *\\
            return size;                                                                                                   //*                   Return coefficient array                *\\
        }


        public double ReadHighBand(string filepath)                                             //*                                                           *\\
        {
            double high;//*                                                           *\\
            using (System.IO.StreamReader parameters = new System.IO.StreamReader(filepath))                                //*            Read Filter Parameters from text file          *\\
            {                                                                                                               //*                                                           *\\
                                                                                                     //*                                                           *\\
                string line;                                                                                                //*                                                           *\\
                while ((line = parameters.ReadLine()) != "High Band: ")                                                              //*               Reading each line in text file              *\\
                {
                    //*                                                           *\\
                }
                line = parameters.ReadLine();
                //*                                                           *\\

                high = Convert.ToDouble(line);                                                         //*   Enter each weight coefficient into Coefficient Array    *\\                                                             //*                                                           *\\
                                                                                                      //*                                                           *\\
                parameters.Close();                                                                                         //*                     Close parameter file                  *\\                                                                                         //*                                                           *\\
            }                                                                                                               //*                                                           *\\
            return high;                                                                                                   //*                   Return coefficient array                *\\
        }

        public double ReadLowBand(string filepath)                                             //*                                                           *\\
        {                                                                                                                   //*                                                           *\\
            double low;
            using (System.IO.StreamReader parameters = new System.IO.StreamReader(filepath))                                //*            Read Filter Parameters from text file          *\\
            {                                                                                                               //*                                                           *\\
                int lineCounter = 0;                                                                                        //*                                                           *\\
                string line;                                                                                                //*                                                           *\\
                while ((line = parameters.ReadLine()) != "Low Band: ")                                                              //*               Reading each line in text file              *\\
                {
                    //*                                                           *\\
                }
                line = parameters.ReadLine();
                //*                                                           *\\

                low = Convert.ToDouble(line);                                                         //*   Enter each weight coefficient into Coefficient Array    *\\                                                             //*                                                           *\\
                                                                                                      //*                                                           *\\
                parameters.Close();                                                                                         //*                     Close parameter file                  *\\                                                                                         //*                                                           *\\
            }                                                                                                               //*                                                           *\\
            return low;                                                                                                   //*                   Return coefficient array                *\\
        }

        public string[] sensorCalc(double volt, int sensornumber, string[] data)                                            //*                                                           *\\
        {                                                                                                                   //*                                                           *\\
            double temp = CalcTemp(sensornumber, volt);                                                                                //*                  Calculate sensor temperature             *\\                                                       
         
            data[sensornumber] = temp.ToString();                                                                           //*               Store sensor temperature in array           *\\
      
            return data;                                                                                                    //*                Return sensor temperature array            *\\
        }                                                                                                                   //*                                                           *\\

        public double CalcAvgTemp(string[] data,int tally)                                                                  //*                                                           *\\
        {                                                                                                                   //*                                                           *\\
            for (int i = 0; i < 3; i++)                                                                                     //*                                                        *\\
            {                                                                                                               //*                                                        *\\
                if (data[i] == "")                                                                                          //*                 If sensor is deactivated               *\\
                {                                                                                                           //*                                                        *\\
                    data[i] = "0";                                                                                          //*                  Temperature value = 0                 *\\
                }                                                                                                           //*                                                        *\\
                else                                                                                                        //*                                                        *\\
                {                                                                                                           //*                                                        *\\
                    int index = data[i].LastIndexOf(".");
                    if (data[i].ToString().Length - data[i].ToString().IndexOf(".") - 1 > 5)
                    {//*                                                        *\\
                        data[i] = data[i].Substring(0, index + 5);
                    }//*          Contract data to upto 4 decimal places        *\\
                }                                                                                                           //*                                                        *\\
            }                                                                                                               //*       Average temperature = sum of data divided by     *\\


          
            double temp = (Convert.ToDouble(data[0]) + Convert.ToDouble(data[1]) + Convert.ToDouble(data[2])) / tally; //*                 Number of active sensors               *\\
    
            return temp;                                                                                                    //*                Return average temperature              *\\
        }                                                                                                                   //*                                                        *\\

        public void  WriteTemperature(string filepath, double temp,string[] data, int count)                                //*                                                        *\\
        {                                                                                                                   //*                                                        *\\
            int indexT = temp.ToString().LastIndexOf(".");                                                                  //*                                                        *\\
            if (temp.ToString().Length - temp.ToString().IndexOf(".") - 1 > 5)                                              //*                                                        *\\
            {                                                                                                               //*                                                        *\\
                temp = Convert.ToDouble(temp.ToString().Substring(0, indexT + 5));                                          //* Contract average temperaature to upto 4 decimal places *\\
            }                                                                                                               //*                                                        *\\
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, true))                                //*      Write sensor data to text file 'Temperatures'     *\\
            {                                                                                                               //*                                                        *\\
                file.WriteLine("\n"+ count.ToString() + "\t"+data[0] + "\t"+data[1] + "\t"+ data[2] + "\t"+temp);           //*                                                        *\\
            }                                                                                                               //*                                                        *\\
        }                                                                                                                   //*                                                        *\\

        public string ClosingForm1(bool resetting, int count)                                                               //*                                                        *\\
        {                                                                                                                   //*                                                        *\\
            string message = "";                                                                                            //*                                                        *\\
                                                                                                                            //* Different messages based on how many times user tries  *\\
            if (resetting) {                                                                                                //*    to close the form while the chamber is cooling.     *\\
                if (count == 1)                                                                                             //*                                                        *\\
                {                                                                                                           //*                                                        *\\
                    message = "Wait! Chamberreturn  is cooling!";                                                           //*                                                        *\\
                }                                                                                                           //*                                                        *\\
                else if (count == 2)                                                                                        //*                                                        *\\
                {                                                                                                           //*                                                        *\\
                    message = "I said, hold your horses! Let it cool down first!";                                          //*                                                        *\\
                }                                                                                                           //*                                                        *\\
                else if (count == 3 || count == 11)
                {
                    message = "I'm starting to think that you don't know what 'Okay' means...";
                }
                else if (count == 4 || count == 12)
                {
                    message = "You're comprehension skills baffle me.";
                }
                else if (count == 5 || count == 13)
                {
                    message = ("You know, there is more to life than pressing buttons.");
                }
                else if (count == 6)
                {
                    message = ("Tell me, do you speak English or must I talk in Binary?");
                }
                else if (count == 7)
                {
                    message = ("01000001 00100000 01100011 01101111 01101111 01101100 01101001 " +
                        "01101110 01100111 00100000 01101101 01110101 01110011 01110100 00100000 " +
                        "01101111 01100011 01100011 01110101 01110010 00100000 01111001 01101111 " +
                        "01110101 00100000 01101110 01101001 01110100 01110111 01101001 01110100");
                }
                else if (count == 8)
                {
                    message = "Perhaps Klingon: boH qoH. loS bIr";
                }
                else if (count == 9)
                {
                    message = "Dothraki???";
                }
                else if (count == 10 || count == 14)
                {
                    message = "Is there any place in the multiverse in which you have some semblence of patience?";
                }
            }
            else
            {
                if (count == 2 || count == 3 || count == 11)
                {
                    MessageBox.Show("You may proceed.");
                }
                else if (count == 4 || count == 12)
                {
                    MessageBox.Show("I hope you are satisfied, it's cool now");
                }
                else if (count == 5 || count == 13)
                {
                    MessageBox.Show("My gosh that was stressful! You may continue now.");
                }
                else if (count== 6 || count == 7 || count == 8 || count == 9)
                {
                    MessageBox.Show("Forget this, I'm out!");
                }
                else if (count == 10 || count == 14)
                {
                    MessageBox.Show("Well, despite you're quirks, it was most fun chatting with you. Goodbye.");
                }
            }
            return message;
        }
    }
}