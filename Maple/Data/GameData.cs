using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maple.Data
{
    public enum GameStatuses
    {
        Unassigned = -1,
        GameClosed = 0,
        GameOpening = 1,
        ServerSelection = 2,
        ChoosingChannel = 3,
        CharacterSelection = 4,
        Minimized = 5,
        InGame = 6
    }

    public enum InGameStatuses
    {
        Mobbing = 0,
        GrabbingPotions = 1,
        WaitingOnDeathTimer = 2,
        GrabbingMVP = 3,
        WalkingBackToMobMap = 4,
        ChangingChannel = 5
    }

    class GameData
    {
        public static GameStatuses GameStatus;
        public static InGameStatuses InGameStatus;

        public static GameStatuses GetGameStatus(Bitmap fullScreen)
        {
            List<int> locations;
            if (Imaging.FindBitmap(new List<Bitmap>() { Imaging.GetImageFromFile(Imaging.ImageFiles.HP) },
                fullScreen, 1, out locations))
            {
                return GameStatuses.InGame;
            }
            else if (Imaging.FindBitmap(new List<Bitmap>() { Imaging.GetImageFromFile(Imaging.ImageFiles.RebootServerChannelSelect) },
                fullScreen, 1, out locations))
            {
                return GameStatuses.ChoosingChannel;
            }
            else if (Imaging.FindBitmap(new List<Bitmap>() { Imaging.GetImageFromFile(Imaging.ImageFiles.RebootServer) },
                fullScreen, 1, out locations))
            {
                return GameStatuses.ServerSelection;
            }
            else if (Imaging.FindBitmap(new List<Bitmap>() { Imaging.GetImageFromFile(Imaging.ImageFiles.CharacterSelectionCharacterSlot) },
                fullScreen, 1, out locations))
            {
                return GameStatuses.CharacterSelection;
            }
            return GameStatuses.GameClosed;
        }

        public MapData CurrentMapData;
        public CharacterData CurrentCharacterData;
        private Thread _mobFinderThread;
        private Thread _nearbyMobThread;
        private Thread _minimapThread;

        int NumMobsAtNextCluster;
        Vector2 NextMobClusterMinimapLocation;
        bool ReadyForNewCluster;
        bool MobDirectionLeft;
        bool CloseMobsStillAlive;
        bool CharacterDirectionLeft;
        Vector2 CurrentPlayerMinimapLocation;

        private void NewMobFinderThreadWorker()
        {
            Vector2 bestMinimapData;
            int numMobs;
            if (Imaging.GetMobLocation(CurrentMapData, out numMobs, out bestMinimapData))
            {
                if (numMobs > NumMobsAtNextCluster && ReadyForNewCluster)
                {
                    NumMobsAtNextCluster = numMobs;
                    NextMobClusterMinimapLocation = bestMinimapData;
                }
            }
        }

        private void MinimapThreadWorker()
        {
            var playerImage = Imaging.GetImageFromFile(Imaging.ImageFiles.PlayerMiniMap);
            Bitmap fullGameScreen;
            while (true)
            {
                if (!Imaging.GetCurrentGameScreen(out fullGameScreen))
                {
                    continue;
                }
                var miniMapScreen = Imaging.CropImage(fullGameScreen, Imaging.MiniMapRect);
                if (Imaging.FindBitmap(new List<Bitmap>() { playerImage }, miniMapScreen, 20, out List<int> locations))
                {
                    // hope and pray there is only one player location
                    CurrentPlayerMinimapLocation = MapleMath.CorrectImageHeight(MapleMath.PixelToPixelCoordinate(locations[0], miniMapScreen.Width), miniMapScreen.Height);
                }
            }
            
        }

        private void NearbyMobsThreadWorker()
        {
            Bitmap curScreen;
            while (!Imaging.GetCurrentGameScreen(out curScreen))
            {
                Thread.Sleep(10);
            }
            var leftHealthBarImage = Imaging.GetImageFromFile(Imaging.ImageFiles.LeftHealthBar);
            List<int> imageLocations;
            bool isLeft = false;
            double minDistance = double.MaxValue;
            int distanceTolerance = 10;
            int maxFailureAmount = 3;
            int totalFailureAmount = 0;
            double curDistance;
            if (Imaging.FindBitmap(new List<Bitmap>() { leftHealthBarImage }, curScreen, 20, out imageLocations))
            {
                foreach (var curImageLocation in imageLocations)
                {
                    var curGameLocation = MapleMath.CorrectImageHeight(MapleMath.PixelToPixelCoordinate(curImageLocation, curScreen.Width), curScreen.Height);
                    var curMinimapLocation = MapleMath.MapCoordinatesToMiniMapCoordinates(curGameLocation);
                    curDistance = MapleMath.PixelCoordinateDistance(curMinimapLocation, CurrentPlayerMinimapLocation);
                    if (curDistance < minDistance)
                    {
                        minDistance = curDistance;
                        if (curMinimapLocation.X < CurrentPlayerMinimapLocation.X)
                        {
                            isLeft = true;
                        }
                        else
                        {
                            isLeft = false;
                        }
                    }
                }
                if (minDistance < distanceTolerance)
                {
                    totalFailureAmount = 0;
                    CloseMobsStillAlive = true;
                    MobDirectionLeft = isLeft;
                }
                else
                {
                    totalFailureAmount++;
                    if (totalFailureAmount > maxFailureAmount)
                    {
                        CloseMobsStillAlive = false;
                    }
                }
            }
        }

        /*public void WalkBrain(MapPiece mapTarget, Vector2 vectorTarget)
        {
            MapPiece currentMapPiece = CurrentMapData.GetCurrentMapPiece(CurrentPlayerMinimapLocation);
            if (currentMapPiece != mapTarget)
            {
                
            }
            if (mapTarget.MapPieceType == MapPieceTypes.Ledge)
            {
                if (CurrentPlayerMinimapLocation.X < vectorTarget.X)
                {
                    Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                    while (CurrentPlayerMinimapLocation.X < vectorTarget.X)
                    {
                        Thread.Sleep(5);
                    }
                    Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                }
                else
                {
                    Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                    while (CurrentPlayerMinimapLocation.X > vectorTarget.X)
                    {
                        Thread.Sleep(5);
                    }
                    Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                }
            }
            else if (mapTarget.MapPieceType == MapPieceTypes.Rope)
            {
                if (CurrentPlayerMinimapLocation.Y < vectorTarget.Y)
                {
                    Input.StartInput(Input.SpecialCharacters.KEY_UP_ARROW);
                    while (CurrentPlayerMinimapLocation.Y < vectorTarget.Y)
                    {
                        Thread.Sleep(5);
                    }
                    Input.StopInput(Input.SpecialCharacters.KEY_UP_ARROW);
                }
                else
                {
                    Input.StartInput(Input.SpecialCharacters.KEY_DOWN_ARROW);
                    while (CurrentPlayerMinimapLocation.Y > vectorTarget.Y)
                    {
                        Thread.Sleep(5);
                    }
                    Input.StopInput(Input.SpecialCharacters.KEY_DOWN_ARROW);
                }
            }
            
        }*/

        public void GameBrain()
        {
            Vector2 nextMobCluster;
            if (!Imaging.GetCurrentGameScreen(out Bitmap curGameScreen))
            {
                return;
            }
            if (GameStatus == GameStatuses.Unassigned)
            {
                GetGameStatus(curGameScreen);
            }
            // move through the game statuses until we are in game
            if (GameStatus == GameStatuses.GameClosed)
            {

            }
            string fileLocation = System.Configuration.ConfigurationManager.AppSettings["ClassExportLocation"];
            string fullPath = Path.Combine(fileLocation, Jobs.DemonAvenger.ToString());
            string jsonString = File.ReadAllText(fullPath);
            CurrentCharacterData = JsonConvert.DeserializeObject<CharacterData>(jsonString);
            fileLocation = System.Configuration.ConfigurationManager.AppSettings["MapExportLocation"];
            fullPath = Path.Combine(fileLocation, MapNames.KerningTower2FCafe1.ToString());
            jsonString = File.ReadAllText(fullPath);
            CurrentMapData = JsonConvert.DeserializeObject<MapData>(jsonString);
            ReadyForNewCluster = true;
            _mobFinderThread = new Thread(NewMobFinderThreadWorker);
            _mobFinderThread.Start();
            _minimapThread = new Thread(MinimapThreadWorker);
            _minimapThread.Start();
            CurrentMapData.SetupConjoiningMapPiecesBasedOnJumpData(CurrentCharacterData.JumpDataData);
            if (InGameStatus == InGameStatuses.Mobbing)
            {
                
                //_mobFinderThread.Start();
            }
            while (NextMobClusterMinimapLocation == null)
            {
                Thread.Sleep(100);
            }
            Random rand = new Random();
            while (InGameStatus == InGameStatuses.Mobbing)
            {
                while (CloseMobsStillAlive)
                {
                    //if (MobDirectionLeft && !CharacterDirectionLeft)
                    //{

                    //}
                    //else if (!MobDirectionLeft && CharacterDirectionLeft)
                    //{

                    //}
                    CurrentCharacterData.TryToUseSkill();
                }
                //CurrentPlayerMinimapLocation = new Vector2(20, 93);
                //NextMobClusterMinimapLocation = new Vector2(15, 93); // test todo
                ReadyForNewCluster = false;
                NumMobsAtNextCluster = 0;
                nextMobCluster = NextMobClusterMinimapLocation;
                MapPiece targetMapPiece = CurrentMapData.GetCurrentMapPiece(nextMobCluster);
                MapPiece currentMapPiece = CurrentMapData.GetCurrentMapPiece(CurrentPlayerMinimapLocation);
                while (CurrentMapData.GetCurrentMapPiece(CurrentPlayerMinimapLocation).Beginning.X != targetMapPiece.Beginning.X && currentMapPiece.Beginning.Y != targetMapPiece.Beginning.Y)
                {
                    List<MapPiece> mapPieceDataList = MapleMath.FindRoute(CurrentMapData, currentMapPiece, targetMapPiece);
                    Console.WriteLine("starting!!");
                    double curY = 0;
                    for (int curMapPieceIterator = 0; curMapPieceIterator < mapPieceDataList.Count; curMapPieceIterator++)
                    {
                        while (curY != CurrentPlayerMinimapLocation.Y)
                        {
                            curY = CurrentPlayerMinimapLocation.Y;
                            Thread.Sleep(250);
                        }

                        if (curMapPieceIterator == mapPieceDataList.Count - 1)
                        {
                            // todo
                            if (mapPieceDataList[curMapPieceIterator].MapPieceType == MapPieceTypes.Rope)
                            {
                                Input.StopInput(Input.SpecialCharacters.KEY_UP_ARROW);
                            }
                            if (CurrentPlayerMinimapLocation.X < nextMobCluster.X)
                            {
                                Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                                while (CurrentPlayerMinimapLocation.X < nextMobCluster.X)
                                {
                                    Thread.Sleep(50);
                                }
                                Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                            }
                            else if (CurrentPlayerMinimapLocation.X > nextMobCluster.X)
                            {
                                Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                                while (CurrentPlayerMinimapLocation.X < nextMobCluster.X)
                                {
                                    Thread.Sleep(50);
                                }
                                Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                            }
                            continue;
                        }
                        var curMapPieceLink = mapPieceDataList[curMapPieceIterator].MapPieceLinkDataList.Where(x => x.JoiningMapPiece.Beginning.X == mapPieceDataList[curMapPieceIterator + 1].Beginning.X
                            && x.JoiningMapPiece.Beginning.Y == mapPieceDataList[curMapPieceIterator + 1].Beginning.Y).First();

                        //var jumpDataList = curMapPieceLink.JoiningJumpDataList.Where(x => x.LandLocation.X > (mapPieceDataList[curMapPieceIterator + 1].Beginning.X) && x.JumpLocation.X > (mapPieceDataList[curMapPieceIterator].Beginning.X)
                        //    && x.LandLocation.X < (mapPieceDataList[curMapPieceIterator + 1].End.X) && x.JumpLocation.X < (mapPieceDataList[curMapPieceIterator].End.X)).ToList();
                        int randJumpSelection = rand.Next(0, curMapPieceLink.JoiningJumpDataList.Count);
                        JoiningJumpDataPoint joiningJumpData = curMapPieceLink.JoiningJumpDataList[randJumpSelection];
                        if (mapPieceDataList[curMapPieceIterator].MapPieceType == MapPieceTypes.Rope)
                        {
                            while (CurrentPlayerMinimapLocation.Y < mapPieceDataList[curMapPieceIterator].End.Y)
                            {
                                Thread.Sleep(10);
                            }
                            Input.StopInput(Input.SpecialCharacters.KEY_UP_ARROW);
                        }
                        else if (CurrentPlayerMinimapLocation.X < joiningJumpData.JumpLocation.X)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                            while (CurrentPlayerMinimapLocation.X < joiningJumpData.JumpLocation.X)
                            {
                                Thread.Sleep(50);
                            }
                            Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                        }
                        else if (CurrentPlayerMinimapLocation.X > joiningJumpData.JumpLocation.X)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                            while (CurrentPlayerMinimapLocation.X > joiningJumpData.JumpLocation.X)
                            {
                                Thread.Sleep(50);
                            }
                            Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                        }
                        if (joiningJumpData.TurnedLeft)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                        }
                        else
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                        }
                        Thread.Sleep(10);
                        JumpData.TryToJump(joiningJumpData.JumpType, joiningJumpData.MillisecondDelays);
                        Thread.Sleep(10);
                        if (curMapPieceIterator + 1 < mapPieceDataList.Count() && mapPieceDataList[curMapPieceIterator + 1].MapPieceType == MapPieceTypes.Rope)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_UP_ARROW);
                        }
                        else if (joiningJumpData.TurnedLeft)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                        }
                        else
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                        }
                        Thread.Sleep(10);
                        if (joiningJumpData.TurnedLeft)
                        {
                            Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                        }
                        else
                        {
                            Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                        }
                    }
                    Console.WriteLine("done!!");
                }
                CloseMobsStillAlive = true;
                ReadyForNewCluster = true;
                
            }
            _mobFinderThread.Join();
            
            // game status == in game
            // if in game status == mobbing
            // get mob cluster and store the pixel location
            // convert pixel location to minimap location
            // get closest platform to mob cluster
            // if platform is current platform walk to mob location
            // if platform is within vertical distance or fall distance of jump walk to designated area
            // else check which map objects are in jumpable range/which ropes are climable to the platform
            // if any of these are current platform walk and jump on rope/to the platform
            // this will likely need to be recursion step
            // 

            

        }
    }
}
