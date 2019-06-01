using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.DAQmx;

namespace _3._5versA2
{
    class DigitalO
    {
        Task digitalOut = new Task();
        DigitalSingleChannelWriter writer;
        public void OpenChannel(string device)
        {
            digitalOut.DOChannels.CreateChannel("dev"+device+"/port0",
                "DigitalChn0",
                ChannelLineGrouping.OneChannelForAllLines);
            writer = new DigitalSingleChannelWriter(digitalOut.Stream);

        }

        public void WriteData(int length)
        {
            if (writer != null)
                writer.WriteSingleSamplePort(true, (UInt32)length);
        }
        
    }
}