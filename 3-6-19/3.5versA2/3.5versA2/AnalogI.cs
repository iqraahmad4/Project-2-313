using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.DAQmx;
using NationalInstruments;

namespace _3._5versA2
{
    class AnalogI
    {
        Task analogIn = new Task();
        AnalogSingleChannelReader reader;
        NationalInstruments.AnalogWaveform<double> data;
        int samplesPerChannel = 11;
        int sampleIndex = 0;
        double summedSamples=0;
        double[] samplesAvg = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        //double[] samplesAvg = new double[4];


        public void OpenChannel(string AIChannel, string channelName, string device)
        {
            analogIn.AIChannels.CreateVoltageChannel("dev"+device+"/ai"+AIChannel,
                channelName,
                AITerminalConfiguration.Rse,
                -10.0, 10.0,
                AIVoltageUnits.Volts);

            analogIn.Timing.ConfigureSampleClock("",
                100.0,
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                samplesPerChannel);

            reader = new AnalogSingleChannelReader(analogIn.Stream);
        }

       public double ReadData_O()
        {
            data = reader.ReadWaveform(samplesPerChannel);
            double txtbox = data.Samples[1].Value;
            return txtbox;
        }

        public double[] ReadData()
        {
            data = reader.ReadWaveform(samplesPerChannel);
            //digital filter shit you take 10 samples then staring fro first sample you take the next 4 samples(5 in total ) and calculate the average-> 1+2+3+4+5/5
            //then move on to 2nd sample->2+3+4+5+6/5 and keep going untill there are no more groups of 5 samples
            //each sample needs a coefficient multiplying it and since we want the latest data to have more importance the biggest coeffiecnt is used onthe last sample
            // need to indicate to user to put in correct order
            //weighted moving average pg 16 370 chap 4 dsp3

            //here we return the 5 averaged samples and later we convert them

            //first check if index has enough samples to continue
            for (int i = 0; i < 6; i++)
                {
                summedSamples = 0;

                for (int j = i; j < (i + 5); j++) 
                {
                    summedSamples += data.Samples[j].Value;
                }

                samplesAvg[i] = summedSamples / 5;
            }
           // Console.WriteLine(samplesAvg[0] + " "+samplesAvg[1]+" "+ samplesAvg[2] + " "+samplesAvg[3] + " "+samplesAvg[4] + " " + samplesAvg[5]);
             //  samplesAvg =  { 26.0; 26.0; 26.0; 26.0; 26.0; };
             //double[] test=[1.0,1.0]
            //  "Sample{0}=>time={1}, value{2}", i + 1, data.Samples[i].TimeStamp, data.Samples[i].Value);
            return samplesAvg;

           
        }
    }
}
