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
        public static MapleSerialPort MouseArduinoData;

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

        private static void MoveMouse(Vector2 location)
        {
            // MV00000000
            string movXString = location.X.ToString().PadLeft(4, '0');
            string movYString = location.Y.ToString().PadLeft(4, '0');
            MasterArduinoData.SendData($"MV{movXString}{movYString}");
            _mouseLocation = new Vector2(Math.Max(_mouseLocation.X + location.X, 0), Math.Max(_mouseLocation.Y + location.Y, 0));
        }

        public static void SetMouseLocation(Vector2 location)
        {
            Random rand = new Random();
            /*int sleepAmount = 1;
            int randX;
            int randY;
            int numPolys = rand.Next(1, 4);*/
            if (_mouseLocation.X == location.X)
            {
                MoveMouse(new Vector2(rand.Next(3, 10), rand.Next(1, 5)));
            }
            List<Vector2> locations = new List<Vector2>() { location, _mouseLocation };
            /*for (int i = 0; i < numPolys; i++)
            {
                randX = location.X - _mouseLocation.X;
                randY = location.Y - _mouseLocation.Y;
                int moveAmountX = rand.Next(Math.Min(randX, 0), Math.Max(randX + 1, 0));
                int moveAmountY = rand.Next(Math.Min(randY, 0), Math.Max(randY + 1, 0));
                locations.Add(new Vector2(moveAmountX, moveAmountY));
            }*/
            var moveLocations = MapleMath.PolynomialLeastSquares(locations.OrderBy(x => x.X).ToList(), 2);
            int originalIterator = 0;
            int iteratorChangeAmount = 1;
            int maxVal = moveLocations.Count;
            if (_mouseLocation.X > location.X)
            {
                originalIterator = moveLocations.Count - 1;
                iteratorChangeAmount = -1;
                maxVal = -1;
            }
            for (int i = originalIterator; i != maxVal; i += iteratorChangeAmount)
            {
                Vector2 curLocation = moveLocations[i];
                MoveMouse(new Vector2(curLocation.X - _mouseLocation.X, curLocation.Y - _mouseLocation.Y));
            }
            /*while (_mouseLocation.X != location.X || _mouseLocation.Y != location.Y)
            {
                
                sleepAmount = rand.Next(5, 15);
                MoveMouse(new Vector2(moveAmountX, moveAmountY));
                Thread.Sleep(sleepAmount);
            }*/
            //MoveMouse(new Vector2(location.X, location.Y));
        }

        private static Vector2 _mouseLocation;


        public static void ClickMouse()
        {
            MasterArduinoData.SendData($"MOUSELCLCK");
        }

        public static void RightClickMouse()
        {
            MasterArduinoData.SendData($"MOUSERCLCK");
        }

        public static void ReleaseMouse()
        {
            MasterArduinoData.SendData($"MOUSELRLAX");
        }

        public static void RightReleaseMouse()
        {
            MasterArduinoData.SendData($"MOUSERRLAX");
        }

        public static void InitializeInputs()
        {
            Input.KeyboardArduinoData = new MapleSerialPort("Keyboard", ConfigurationManager.AppSettings["ArduinoKeyboardComNumber"]);
            Input.MasterArduinoData = new MapleSerialPort("Master", ConfigurationManager.AppSettings["ArduinoMasterComNumber"]);
            Input.MouseArduinoData = new MapleSerialPort("Mouse", ConfigurationManager.AppSettings["ArduinoMouseComNumber"]);
            _mouseLocation = new Vector2(0, 0);
            for (int i = 0; i < 10; i++)
            {
                Input.MoveMouse(new Vector2(10, 100));
            }
            for (int i = 0; i < 100; i++)
            {
                Input.MoveMouse(new Vector2(-100, -50));
            }
            ClickMouse();
            Thread.Sleep(11);
            ReleaseMouse();
        }

        /*public static void DoubleClickMouse()
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
