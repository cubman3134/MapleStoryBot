using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maple.Data
{
    class Input
    {
        public enum SpecialCharacters
        {
            KEY_LEFT_CTRL = 128,
            KEY_LEFT_SHIFT = 129,
            KEY_LEFT_ALT = 130,
            KEY_LEFT_GUI	=	131,
            KEY_RIGHT_CTRL	=	132,
            KEY_RIGHT_SHIFT	=	133,
            KEY_RIGHT_ALT	=	134,
            KEY_RIGHT_GUI	=	135,
            KEY_UP_ARROW	=	218,
            KEY_DOWN_ARROW	=	217,
            KEY_LEFT_ARROW	=	216,
            KEY_RIGHT_ARROW	=	215,
            KEY_BACKSPACE	=	178,
            KEY_TAB	=	179       ,
            KEY_RETURN	=	176   ,
            KEY_ESC	=	177       ,
            KEY_INSERT	=	209       ,
            KEY_DELETE	=	212       ,
            KEY_PAGE_UP	=	211       ,
            KEY_PAGE_DOWN	=	214   ,
            KEY_HOME	=	210,
            KEY_END	= 213,
            KEY_CAPS_LOCK = 193,
            KEY_F1	=	194,
            KEY_F2	=	195,
            KEY_F3	=	196,
            KEY_F4	=	197,
            KEY_F5	=	198,
            KEY_F6	=	199,
            KEY_F7	=	200,
            KEY_F8	=	201,
            KEY_F9	=	202,
            KEY_F10	=	203,
            KEY_F11	=	204,
            KEY_F12	=	205,
        }

        public static MapleSerialPort MasterArduinoData;
        public static MapleSerialPort KeyboardArduinoData;

        public static void StartInput(char c)
        {
            int asciiVal = (int)c;
            string asciiValString = asciiVal.ToString().PadLeft(3, '0');
            MasterArduinoData.SendData($"KEYDOWN{asciiValString}");
        }

        public static void StartInput(SpecialCharacters specialCharacter)
        {
            int asciiVal = (int)specialCharacter;
            string asciiValString = asciiVal.ToString().PadLeft(3, '0');
            MasterArduinoData.SendData($"KEYDOWN{asciiValString}");
        }

        public static void StopInput(char c)
        {
            int asciiVal = (int)c;
            string asciiValString = asciiVal.ToString().PadLeft(3, '0');
            MasterArduinoData.SendData($"KEYLIFT{asciiValString}");
        }

        public static void StopInput(SpecialCharacters specialCharacter)
        {
            int asciiVal = (int)specialCharacter;
            string asciiValString = asciiVal.ToString().PadLeft(3, '0');
            MasterArduinoData.SendData($"KEYLIFT{asciiValString}");
        }

        /*public static void SetMouseLocation(int locationX, int locationY)
        {
            // MV00000000
            ArduinoPortData.Write($"#MOVX{locationX}\n");
            ArduinoPortData.Write($"#MOVY{locationY}\n");
        }

        public static void ClickMouse()
        {
            ArduinoPortData.Write($"MCLCK");
        }

        public static void DoubleClickMouse()
        {
            ArduinoPortData.Write($"MDCLCK");
        }

        private static void DisconnectFromArduino()
        {
            //IsConnected = false;
            ArduinoPortData.Write("#STOP\n");
            ArduinoPortData.Close();
        }*/
    }
}
