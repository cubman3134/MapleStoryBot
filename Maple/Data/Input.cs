using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    class Input
    {
        //private static bool IsConnected;
        private static SerialPort ArduinoPortData;

        public static void ConnectToArduino()
        {
            //IsConnected = true;
            string selectedPort = ConfigurationManager.AppSettings["ArduinoComNumber"];
            ArduinoPortData = new SerialPort(selectedPort, 9600, Parity.None, 8, StopBits.One);
            ArduinoPortData.Open();
            ArduinoPortData.Write("#STAR\n");
        }

        public static void StartInput(char c)
        {
            int asciiVal = (int)c;
            ArduinoPortData.Write($"#KEYD{asciiVal}\n");
        }

        public static void StopInput(char c)
        {
            int asciiVal = (int)c;
            ArduinoPortData.Write($"#KEYU{asciiVal}\n");
        }

        public static void SetMouseLocation(int locationX, int locationY)
        {
            ArduinoPortData.Write($"#MOVX{locationX}\n");
            ArduinoPortData.Write($"#MOVY{locationY}\n");
        }

        public static void ClickMouse()
        {
            ArduinoPortData.Write($"#CLCK\n");
        }

        public static void DoubleClickMouse()
        {
            ArduinoPortData.Write($"#DCLCK\n");
        }

        private static void DisconnectFromArduino()
        {
            //IsConnected = false;
            ArduinoPortData.Write("#STOP\n");
            ArduinoPortData.Close();
        }
    }
}
