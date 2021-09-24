using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maple.Data
{
    public class MapleSerialPort
    {
        private SerialPort ArduinoPortData;
        private string _portName;
        public MapleSerialPort(string portName, string comNumber)
        {
            ArduinoPortData = new SerialPort(comNumber, 9600);
            ArduinoPortData.RtsEnable = true;
            ArduinoPortData.Open();
            _portName = portName;
            //ArduinoPortData.DataReceived += ArduinoDataReceived;
            ReadThread = new Thread(Read) { Name = "Maple Serial Port REad Thread" };
            ReadThread.Start();
        }

        Thread ReadThread;

        private void Read()
        {
            while (true)
            {
                string s = ArduinoPortData.ReadExisting();
                if (s.Length > 0)
                {
                    //Console.WriteLine($"Data Received from {_portName} [{s}]");
                }
                Thread.Sleep(100);
            }
        }

        public void SendData(string message)
        {
            ArduinoPortData.RtsEnable = false;
            char endString = '#';
            ArduinoPortData.Write($"{message} {endString}");
            ArduinoPortData.RtsEnable = true;
        }

        ~MapleSerialPort()
        {
            ReadThread.Join();
            ArduinoPortData.Close();
        }
    }
}
