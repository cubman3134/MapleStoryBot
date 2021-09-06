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

    public class DelaysAndEquationCoefficients
    {
        public List<int> MillisecondDelays { get; set; }
        public List<double> EquationCoefficients { get; set; }

        public DelaysAndEquationCoefficients(List<int> millisecondDelays, List<double> equationCoefficients)
        {
            MillisecondDelays = millisecondDelays;
            EquationCoefficients = equationCoefficients;
        }
    }

    public class JumpInformation
    {
        public JumpTypes JumpType { get; set; }

        public List<DelaysAndEquationCoefficients> MillisecondDelaysToEquationCoefficients { get; set; }

        public JumpInformation(JumpTypes jumpType)
        {
            JumpType = jumpType;
            MillisecondDelaysToEquationCoefficients = new List<DelaysAndEquationCoefficients>();
        }

        public void AddToDelaysAndEquationCoefficients(Tuple<List<int>, List<double>> newData)
        {
            MillisecondDelaysToEquationCoefficients.Add(new DelaysAndEquationCoefficients(newData.Item1, newData.Item2));
        }
    }

    public class JumpData
    {
        public static Dictionary<JumpTypes, int> JumpTypesToNumberOfPauses = new Dictionary<JumpTypes, int>()
        {
            { JumpTypes.ArrowJumpUpUp, 5 },
            { JumpTypes.ArrowJump, 1 },
            { JumpTypes.ArrowJumpJump, 3 },
            { JumpTypes.ArrowFlashJump, 1 }
        };

        public static List<int> MilliDelays = new List<int>() { 20, 50, 100 };
        // int is intercharacter delay
        //public Dictionary<Tuple<JumpTypes, List<int>>, List<double>> JumpTypeAndDelayToEquationCoefficients { get; set; }
        public List<JumpInformation> JumpInformationDataList;


        public JumpData()
        {
            JumpInformationDataList = new List<JumpInformation>();
            //JumpTypeAndDelayToEquationCoefficients = new Dictionary<Tuple<JumpTypes, List<int>>, List<double>>();
        }

        private enum GenerateEquationCoefficientsStatuses
        {
            Unstarted = 5,
            FindingInitialYMovement = 1,
            FindingYMovementStop = 2,
            Finished = 3
        }

        public static JumpData GenerateJumpData()
        {
            JumpData jumpDataData = new JumpData();
            foreach (var curJumpType in (JumpTypes[]) Enum.GetValues(typeof(JumpTypes)))
            {
                jumpDataData.JumpInformationDataList.Add(new JumpInformation(curJumpType));
                List<Tuple<List<int>, List<double>>> equationCoefficientData = GenerateEquationCoefficientsForJumpType(curJumpType);
                foreach (var curEquationCoefficientData in equationCoefficientData)
                {
                    jumpDataData.JumpInformationDataList.Last().AddToDelaysAndEquationCoefficients(curEquationCoefficientData);
                    //jumpDataData.JumpTypeAndDelayToEquationCoefficients[new Tuple<JumpTypes, List<int>>(curJumpType, curEquationCoefficientData.Item1)] = curEquationCoefficientData.Item2;
                }
            }
            return jumpDataData;
        }

        private static bool IncrementIterators(ref List<int> iterators, int max)
        {
            iterators.Reverse();
            bool ableToIncrement = false;
            for (int i = 0; i < iterators.Count; i++)
            {
                if (iterators[i] == max)
                {
                    continue;
                }
                for (int j = 0; j < i; j++) 
                {
                    iterators[j] = 0;
                }
                iterators[i]++;
                ableToIncrement = true;
                break;
            }
            iterators.Reverse();
            return ableToIncrement;
        }

        private static List<Tuple<List<int>, List<double>>> GenerateEquationCoefficientsForJumpType(JumpTypes jumpType)
        {
            int numberOfDelays = JumpTypesToNumberOfPauses[jumpType];
            List<int> curDelays = new List<int>();
            var returnData = new List<Tuple<List<int>, List<double>>>();
            for (int i = 0; i < numberOfDelays; i++)
            {
                curDelays.Add(0);
            }
            do
            {
                var millisecondDelays = curDelays.Select(x => { return MilliDelays[x]; }).ToList();
                Console.WriteLine($"Generating equation coefficients for jump type [{jumpType.ToString()}] and iterators [{string.Join(", ", curDelays)}]");
                var equationCoefficients = GenerateEquationCoefficients(jumpType, millisecondDelays);
                if (equationCoefficients == null || equationCoefficients.Count == 0)
                {
                    continue;
                }
                returnData.Add(new Tuple<List<int>, List<double>>(millisecondDelays, equationCoefficients));
            } 
            while (IncrementIterators(ref curDelays, MilliDelays.Count - 1));
            return returnData;
        }

        private static void TryToJump(JumpTypes jumpType, List<int> millisecondDelays)
        {
            switch (jumpType)
            {
                case JumpTypes.ArrowFlashJump:
                    Input.StartInput('e');
                    Thread.Sleep(millisecondDelays[0]);
                    Input.StopInput('e');
                    break;
                case JumpTypes.ArrowJump:
                    Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[0]);
                    Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    break;
                case JumpTypes.ArrowJumpJump:

                    Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[0]);
                    Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[1]);
                    Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[2]);
                    Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    break;
                case JumpTypes.ArrowJumpUpUp:
                    Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    Thread.Sleep(millisecondDelays[0]);
                    Input.StartInput(Input.SpecialCharacters.KEY_UP_ARROW);
                    Thread.Sleep(millisecondDelays[1]);
                    Input.StopInput(Input.SpecialCharacters.KEY_UP_ARROW);
                    Thread.Sleep(millisecondDelays[2]);
                    Input.StartInput(Input.SpecialCharacters.KEY_UP_ARROW);
                    Thread.Sleep(millisecondDelays[3]);
                    Input.StopInput(Input.SpecialCharacters.KEY_UP_ARROW);
                    Thread.Sleep(millisecondDelays[4]);
                    Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ALT);
                    break;
            }
        }

        private static List<double> GenerateEquationCoefficients(JumpTypes jumpType, List<int> millisecondDelays)
        {
            PhotoTaker.StartTakingImages(75);
            Thread.Sleep(100);
            Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
            TryToJump(jumpType, millisecondDelays);
            Thread.Sleep(2000);
            Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
            PhotoTaker.StopTakingImages();
            var photosWithTimestampDataList =  PhotoTaker.GetPhotosAndDeleteFromMemory();
            Vector2 previous = null;
            Vector2 beginning = null;
            List<Vector2> dataPoints = null;
            GenerateEquationCoefficientsStatuses status = GenerateEquationCoefficientsStatuses.Unstarted;
            var playerImage = Imaging.GetImageFromFile(Imaging.ImageFiles.PlayerMiniMap);
            foreach (var curPhoto in photosWithTimestampDataList)
            {
                var curMinimap = Imaging.CropImage(curPhoto.Photo, Imaging.MiniMapRect);
                if (!Imaging.FindBitmap(new List<System.Drawing.Bitmap>() { playerImage }, curMinimap, 20, out List<int> locations))
                {
                    continue;
                }
                Vector2 cur = MapleMath.PixelToPixelCoordinate(locations[0], curMinimap.Width);
                switch (status)
                {
                    case GenerateEquationCoefficientsStatuses.Unstarted:
                        beginning = cur;
                        status = GenerateEquationCoefficientsStatuses.FindingInitialYMovement;
                        break;
                    case GenerateEquationCoefficientsStatuses.FindingInitialYMovement:
                        previous = cur;
                        if (cur.Y == beginning.Y && cur.X == beginning.X)
                        {
                            continue;
                        }
                        dataPoints = new List<Vector2>() { previous, cur };
                        status = GenerateEquationCoefficientsStatuses.FindingYMovementStop;
                        break;
                    case GenerateEquationCoefficientsStatuses.FindingYMovementStop:
                        
                        if (cur.Y == dataPoints[0].Y)
                        {
                            status = GenerateEquationCoefficientsStatuses.Finished;
                        }
                        else
                        {
                            dataPoints.Add(cur);
                        }
                        previous = cur;
                        break;
                }
                if (status == GenerateEquationCoefficientsStatuses.Finished)
                {
                    break;
                }
            }
            photosWithTimestampDataList.Clear();
            // attempt to reset location
            Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
            Thread.Sleep(3000);
            Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
            Thread.Sleep(100);
            Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
            //TryToJump(jumpType, millisecondDelays);
            Thread.Sleep(100);
            Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
            if (dataPoints == null || dataPoints.Count == 0)
            {
                return new List<double>();
            }
            var originalDataPoint = dataPoints[0];
            // normalize
            dataPoints = dataPoints.Select(x => { return new Vector2(x.X - originalDataPoint.X, x.Y - originalDataPoint.Y); }).ToList();
            if (!dataPoints.Where(x => x.Y > 0).Any())
            {
                return new List<double>();
            }
            return MapleMath.PolynomialRegressionCoefficients(dataPoints);
        }
    }
}
