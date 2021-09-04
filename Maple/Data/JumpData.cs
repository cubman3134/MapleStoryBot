using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maple.Data
{
    public enum JumpTypes
    {
        //ArrowUpJumpJump,
        ArrowJumpUpUp,
        ArrowJump,
        ArrowJumpJump,
        ArrowFlashJump
    }

    public class JumpData
    {
        public static List<int> MilliDelays = new List<int>() { 10, 20, 50, 100, 200 };
        // int is intercharacter delay
        public Dictionary<Tuple<JumpTypes, List<int>>, List<double>> EquationCoefficients { get; set; }

        public JumpData()
        {
            EquationCoefficients = new Dictionary<Tuple<JumpTypes, List<int>>, List<double>>();
        }

        public static List<double> GenerateEquationCoefficients(JumpTypes jumpType, List<int> millisecondDelays)
        {
            PhotoTaker.StartTakingImages(0);
            Thread.Sleep(100);
            switch (jumpType)
            {
                case JumpTypes.ArrowFlashJump:
                    Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                    Input.StartInput('e');
                    Thread.Sleep(millisecondDelays[0]);
                    Input.StopInput('e');
                    Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                    break;
                case JumpTypes.ArrowJump:
                    Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                    Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[0]);
                    Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                    break;
                case JumpTypes.ArrowJumpJump:
                    Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                    Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[0]);
                    Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[1]);
                    Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[2]);
                    Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                    break;
                case JumpTypes.ArrowJumpUpUp:

                    break;
            }
            Thread.Sleep(100);
            PhotoTaker.StopTakingImages();
        }
    }
}
