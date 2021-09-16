using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    public enum MapPieceTypes
    {
        Rope = 0,
        Ledge = 1
    }

    public enum MapNames
    {
        KerningTower2FCafe1,
    }

    /*public enum LevelRanges
    {
        Level145 = 145, // kerning tower
        Level150 = 150, // kerning tower
        Level155 = 155, // kerning tower
        Level160 = 160, // kerning tower
        Level170 = 170, // kritias
        Level175 = 175, // kritias
        Level180 = 180, // kritias
        Level190 = 190, // twilight perion
        Level195 = 195, // twilight perion
    }*/

    public class JoiningJumpDataPoint
    {
        public JumpTypes JumpType;
        public Vector2 JumpLocation;
        public Vector2 LandLocation;
        public bool TurnedLeft;
        public List<int> MillisecondDelays;

        public JoiningJumpDataPoint(JumpTypes jumpType, Vector2 jumpLocation, bool turnedLeft, List<int> millisecondDelays, Vector2 landLocation)
        {
            JumpType = jumpType;
            JumpLocation = jumpLocation;
            TurnedLeft = turnedLeft;
            MillisecondDelays = millisecondDelays;
            LandLocation = landLocation;
        }
    }

    public class MapPieceLink
    {
        public MapPiece JoiningMapPiece;
        public List<JoiningJumpDataPoint> JoiningJumpDataList;

        public double MinimumDistance
        {
            get
            {
                return JoiningJumpDataList.Select(x => MapleMath.PixelCoordinateDistance(x.JumpLocation, x.LandLocation)).OrderBy(x => x).First();
            }
        }

        public MapPieceLink(MapPiece joiningMapPiece)
        {
            JoiningMapPiece = joiningMapPiece;
            JoiningJumpDataList = new List<JoiningJumpDataPoint>();
        }

        public void AddJoiningJumpData(JumpTypes jumpType, Vector2 jumpLocation, bool turnedLeft, List<int> millisecondDelays, Vector2 landLocation)
        {
            JoiningJumpDataList.Add(new JoiningJumpDataPoint(jumpType, jumpLocation, turnedLeft, millisecondDelays, landLocation));
        }
    }

    public class MapPiece
    {
        public MapPieceTypes MapPieceType { get; set; }
        public Vector2 Beginning { get; set; }
        public Vector2 End { get; set; }

        public Vector2 Center { get { return new Vector2(Beginning.X + (End.X - Beginning.X / 2), Beginning.Y); } }

        public bool Visited;

        public List<MapPieceLink> MapPieceLinkDataList;

        public static Vector2 CurrentMinimapLocation { get; set; }
        public static Vector2 CurrentPixelLocation { get; set; }

        

        public MapPiece()
        {
            MapPieceLinkDataList = new List<MapPieceLink>();
        }

        public MapPiece(MapPieceTypes mapPieceType, Vector2 beginning, Vector2 end)
        {
            MapPieceType = mapPieceType;
            Beginning = beginning;
            End = end;
            MapPieceLinkDataList = new List<MapPieceLink>();
        }

        public MapPiece(MapPieceTypes mapPieceType)
        {
            MapPieceType = mapPieceType;
            MapPieceLinkDataList = new List<MapPieceLink>();
        }

        public void SetBeginningOrEnd(Vector2 value)
        {
            if (Beginning == null)
            {
                Beginning = value;
            }
            else if (Beginning.Y > value.Y)
            {
                End = Beginning;
                Beginning = value;
            }
            else if (value.Y > Beginning.Y)
            {
                End = value;
            }
            else if (Beginning.X > value.X)
            {
                End = Beginning;
                Beginning = value;
            }
            else
            {
                End = value;
            }
        }

        public bool GetYValueAtXValue(double xValue, out double yValue)
        {
            yValue = 0;
            if (!(xValue >= Beginning.X && xValue <= End.X))
            {
                return false;
            }
            double changeVal = xValue - Beginning.X;
            double bigXChangeVal = End.X - Beginning.X;
            double bigYChangeVal = End.Y - Beginning.Y;
            double yAngleRadians = Math.Tan(bigYChangeVal / bigXChangeVal);
            yValue = (Math.Tanh(yAngleRadians) * changeVal) + Beginning.Y;
            return true;
        }
        // detolerance allows within landData.x - detolerance 
        public bool LocationWithinBounds(Vector2 locationData, out Vector2 landData, int detolerance)
        {
            landData = null;
            /*if (!(locationData.X >= Beginning.X + detolerance && locationData.X <= End.X - detolerance))
            {
                return false;
            }*/
            if (!GetYValueAtXValue(locationData.X, out double mapDataYValue))
            {
                return false;
            }
            if (locationData.Y < mapDataYValue)
            {
                return false;
            }
            if (Math.Abs(locationData.Y - mapDataYValue) > 1.0)
            {
                return false;
            }
            landData = new Vector2(locationData.X, mapDataYValue);
            return true;
        }
    }

    public class MapData
    {
        public MapNames MapName { get; set; }
        public SerializableBitmapImageWrapper MapNameImage { get; set; }
        public static Dictionary<int, MapNames> LevelRangesToMapNames = new Dictionary<int, MapNames>()
        {
            { 145, MapNames.KerningTower2FCafe1 }
        };

        public static Dictionary<MapNames, List<MobNames>> MapNamesToMobNames = new Dictionary<MapNames, List<MobNames>>()
        {
            { MapNames.KerningTower2FCafe1, new List<MobNames>() { MobNames.BlueRaspberryJellyJuice, MobNames.EnragedEspressoMachine } }
        };

        public List<MapPiece> MapPieceDataList { get; set; }

        public static double MinimapToPixelRatio = 0.043478260869565216;

        public void SetupConjoiningMapPiecesBasedOnJumpData(JumpData characterJumpData)
        {
            double curY;
            List<Vector2> enumeratedJumpValues;
            Vector2 leftCheck, rightCheck, landData;
            bool successful, turnedLeft = false;
            double maxMapXValue = MapPieceDataList.Select(x => x.End.X).OrderByDescending(x => x).First();
            double minMapXValue = MapPieceDataList.Select(x => x.Beginning.X).OrderBy(x => x).First();
            double minMapYValue = Math.Min(MapPieceDataList.Select(x => x.Beginning.Y).OrderBy(x => x).First(), MapPieceDataList.Select(x => x.End.Y).OrderBy(x => x).First());
            double maxMapYValue = Math.Min(MapPieceDataList.Select(x => x.Beginning.Y).OrderByDescending(x => x).First(), MapPieceDataList.Select(x => x.End.Y).OrderByDescending(x => x).First()) + 10;
            characterJumpData.JumpInformationDataList.Add(new JumpInformation(JumpTypes.JumpDown));
            foreach (var curMapPiece in MapPieceDataList)
            {
                foreach (var checkMapPiece in MapPieceDataList)
                {
                    if (checkMapPiece.Beginning.X == curMapPiece.Beginning.X && checkMapPiece.Beginning.Y == curMapPiece.Beginning.Y)
                    {
                        continue;
                    }
                    MapPieceLink curMapPieceLink = new MapPieceLink(checkMapPiece);
                    if (curMapPiece.MapPieceType == MapPieceTypes.Rope)
                    {
                        if (checkMapPiece.LocationWithinBounds(curMapPiece.End, out landData, 0))
                        {
                            curMapPieceLink.AddJoiningJumpData(JumpTypes.ArrowJump, curMapPiece.End, turnedLeft, new List<int>() { 50 }, landData);
                        }
                    }
                    else
                    {
                        // exhaustive
                        foreach (var curX in Enumerable.Range((int)curMapPiece.Beginning.X, (int)curMapPiece.End.X - (int)curMapPiece.Beginning.X))
                        {
                            if (curMapPiece.GetYValueAtXValue(curX, out curY))
                            {
                                
                                foreach (var curJumpInformationData in characterJumpData.JumpInformationDataList)
                                {
                                    if (curJumpInformationData.JumpType == JumpTypes.JumpDown)
                                    {
                                        double highestYValue = 0;
                                        MapPiece highestMapPiece = null;
                                        foreach (var curInnerCheckMapPiece in MapPieceDataList)
                                        {
                                            if (curInnerCheckMapPiece.GetYValueAtXValue(curX, out double curInnerCheckMapPieceY))
                                            {
                                                if (curInnerCheckMapPieceY < curY)
                                                {
                                                    if (highestMapPiece == null || highestYValue < curInnerCheckMapPieceY)
                                                    {
                                                        highestMapPiece = curInnerCheckMapPiece;
                                                        highestYValue = curInnerCheckMapPieceY;
                                                    }
                                                }
                                            }
                                        }
                                        if (highestMapPiece != null && highestMapPiece.Beginning.X == checkMapPiece.Beginning.X && highestMapPiece.Beginning.Y == checkMapPiece.Beginning.Y)
                                        {
                                            curMapPieceLink.AddJoiningJumpData(JumpTypes.JumpDown, new Vector2(curX, curY), true, new List<int>() { 50, 50, 50 }, new Vector2(curX, highestYValue));
                                        }
                                    }
                                    else
                                    {
                                        foreach (var curMillisecondDelaysToEquationCoefficients in curJumpInformationData.MillisecondDelaysToEquationCoefficients)
                                        {
                                            enumeratedJumpValues = curMillisecondDelaysToEquationCoefficients.EnumerateValues();
                                            bool descending = false;
                                            Vector2 previousJumpValue = null;
                                            bool rightValueStillInPlay = true;
                                            bool leftValueStillInPlay = true;
                                            //foreach (var curEnumeratedJumpValue in enumeratedJumpValues)
                                            int curEnumerateValue = 0;
                                            while (rightValueStillInPlay || leftValueStillInPlay)
                                            {
                                                var curEnumeratedJumpValue = new Vector2(curEnumerateValue, curMillisecondDelaysToEquationCoefficients.GetYValueFromXValue(curEnumerateValue++)); 
                                                leftCheck = new Vector2(curX - curEnumeratedJumpValue.X, curY + curEnumeratedJumpValue.Y);
                                                if (leftCheck.Y < minMapYValue || leftCheck.Y > maxMapYValue || leftCheck.X < minMapXValue || leftCheck.X > maxMapXValue)
                                                {
                                                    leftValueStillInPlay = false;
                                                }
                                                rightCheck = new Vector2(curX + curEnumeratedJumpValue.X, curY + curEnumeratedJumpValue.Y);
                                                if (rightCheck.Y < minMapYValue || rightCheck.Y > maxMapYValue || rightCheck.X < minMapXValue || rightCheck.X > maxMapXValue)
                                                {
                                                    rightValueStillInPlay = false;
                                                }
                                                if (previousJumpValue == null || curEnumeratedJumpValue.Y > previousJumpValue.Y)
                                                {
                                                    previousJumpValue = curEnumeratedJumpValue;
                                                    descending = false;
                                                    continue;
                                                }
                                                foreach (var curCheckMapPiece in MapPieceDataList)
                                                {
                                                    if (leftValueStillInPlay && curCheckMapPiece.LocationWithinBounds(leftCheck, out landData, 0))
                                                    {
                                                        leftValueStillInPlay = false;
                                                        if (curCheckMapPiece.Beginning.X == checkMapPiece.Beginning.X && curCheckMapPiece.Beginning.Y == checkMapPiece.Beginning.Y)
                                                        {
                                                            curMapPieceLink.AddJoiningJumpData(curJumpInformationData.JumpType, new Vector2(curX, curY), true, curMillisecondDelaysToEquationCoefficients.MillisecondDelays, landData);
                                                        }
                                                    }
                                                    if (rightValueStillInPlay && curCheckMapPiece.LocationWithinBounds(rightCheck, out landData, 0))
                                                    {
                                                        rightValueStillInPlay = false;
                                                        if (curCheckMapPiece.Beginning.X == checkMapPiece.Beginning.X && curCheckMapPiece.Beginning.Y == checkMapPiece.Beginning.Y)
                                                        {
                                                            curMapPieceLink.AddJoiningJumpData(curJumpInformationData.JumpType, new Vector2(curX, curY), false, curMillisecondDelaysToEquationCoefficients.MillisecondDelays, landData);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    if (curMapPieceLink.JoiningJumpDataList.Any())
                    {
                        curMapPiece.MapPieceLinkDataList.Add(curMapPieceLink);
                    }
                }
            }
        }

        public MapData()
        {
            MapPieceDataList = new List<MapPiece>();
        }

        public MapPiece GetCurrentMapPiece(Vector2 objectLocation)
        {
            double bestYDistance = 0;
            MapPiece bestMapPiece = null;
            foreach (var curMapPiece in MapPieceDataList.Where(x => x.MapPieceType == MapPieceTypes.Rope))
            {
                // end y greater than bc at the top you're on a new platform
                if (curMapPiece.Beginning.X == objectLocation.X && curMapPiece.Beginning.Y <= objectLocation.Y && curMapPiece.End.Y > objectLocation.Y)
                {
                    return curMapPiece;
                }
            }
            foreach (var curMapPiece in MapPieceDataList.Where(x => x.MapPieceType == MapPieceTypes.Ledge))
            {
                if (!curMapPiece.GetYValueAtXValue(objectLocation.X, out double yValue))
                {
                    continue;
                }
                if (objectLocation.Y < yValue)
                {
                    continue;
                }
                double curYDistance = Math.Abs(objectLocation.Y - yValue);
                if (bestMapPiece == null || curYDistance < bestYDistance)
                {
                    bestMapPiece = curMapPiece;
                    bestYDistance = curYDistance;
                }
            }
            return bestMapPiece;
        }
    }
}
