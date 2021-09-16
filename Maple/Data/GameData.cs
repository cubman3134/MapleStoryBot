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
        public bool Closing = false;
        public static GameStatuses GameStatus;
        public static InGameStatuses InGameStatus;
        public List<Vector2> LocationsOfInterest;
        public static GameStatuses GetGameStatus(Bitmap fullScreen)
        {
            List<Vector2> locations;
            if (Imaging.FindBitmap(new List<Bitmap>() { Imaging.GetImageFromFile(Imaging.ImageFiles.HP) },
                fullScreen, out locations))
            {
                return GameStatuses.InGame;
            }
            else if (Imaging.FindBitmap(new List<Bitmap>() { Imaging.GetImageFromFile(Imaging.ImageFiles.RebootServerChannelSelect) },
                fullScreen, out locations))
            {
                return GameStatuses.ChoosingChannel;
            }
            else if (Imaging.FindBitmap(new List<Bitmap>() { Imaging.GetImageFromFile(Imaging.ImageFiles.RebootServer) },
                fullScreen, out locations))
            {
                return GameStatuses.ServerSelection;
            }
            else if (Imaging.FindBitmap(new List<Bitmap>() { Imaging.GetImageFromFile(Imaging.ImageFiles.CharacterSelectionCharacterSlot) },
                fullScreen, out locations))
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
            while (!Closing)
            {
                if (Imaging.GetMobLocation(CurrentMapData, out numMobs, CurrentPlayerMinimapLocation, out bestMinimapData))
                {
                    if (numMobs > NumMobsAtNextCluster)
                    {
                        NumMobsAtNextCluster = numMobs;
                        NextMobClusterMinimapLocation = bestMinimapData;
                    }
                }
            }
        }

        private void MinimapThreadWorker()
        {
            var playerImage = Imaging.GetImageFromFile(Imaging.ImageFiles.PlayerMiniMap);
            Bitmap fullGameScreen;
            while (!Closing)
            {
                if (!Imaging.GetCurrentGameScreen(out fullGameScreen))
                {
                    continue;
                }
                var miniMapScreen = Imaging.CropImage(fullGameScreen, Imaging.MiniMapRect);
                if (Imaging.FindBitmap(new List<Bitmap>() { playerImage }, miniMapScreen, out List<Vector2> locations))
                {
                    // hope and pray there is only one player location
                    CurrentPlayerMinimapLocation = locations[0];
                }
            }
            
        }

        private void NearbyMobsThreadWorker()
        {
            Bitmap curScreen;
            var leftHealthBarImage = Imaging.GetImageFromFile(Imaging.ImageFiles.LeftHealthBar);
            var emptyHealthBarImage = Imaging.GetImageFromFile(Imaging.ImageFiles.HealthBarEmpty);
            List<Vector2> imageLocations;
            bool isLeft = false;
            double minDistance = double.MaxValue;
            int distanceTolerance = 10;
            int maxFailureAmount = 3;
            int totalFailureAmount = 0;
            double titleToPlayerOffsetX = 70;
            double titleToPlayerOffsetY = -50;
            int numMobsLeft = 0;
            int numMobsRight = 0;
            double maxYDistance = 100;
            double maxXDistance = 100;
            Vector2 playerLocationMapPieceStart = null;
            double curDistance;
            Random rand = new Random();
            bool justSleeping = true;
            Bitmap playerTitleImage = Imaging.GetImageFromFile(Imaging.ImageFiles.TheNextLegendTitle); // todo
            while (!Closing)
            {
                while (!CloseMobsStillAlive)
                {
                    justSleeping = true;
                    Thread.Sleep(100);
                    continue;
                }
                if (justSleeping)
                {
                    Thread.Sleep(2000);
                    justSleeping = false;
                }
                while (!Imaging.GetCurrentGameScreen(out curScreen))
                {
                    Thread.Sleep(10);
                }
                // todo using title image here too, havent decided if that's bad yet
                if (Imaging.FindBitmap(new List<Bitmap>() { playerTitleImage }, curScreen, out List<Vector2> titleLocations, ImageFindTypes.Traditional))
                {
                    minDistance = double.MaxValue;
                    if (Imaging.FindBitmap(new List<Bitmap>() { leftHealthBarImage, emptyHealthBarImage }, curScreen, out imageLocations) && (titleLocations.Count == 1 || (titleLocations.Count == 2 && titleLocations[0].X == titleLocations[1].X + 1)))
                    {
                        var playerScreenLocation = new Vector2(titleLocations[0].X + titleToPlayerOffsetX, titleLocations[0].Y + titleToPlayerOffsetY);
                        playerLocationMapPieceStart = CurrentMapData.GetCurrentMapPiece(playerScreenLocation).Beginning;
                        foreach (var curImageLocation in imageLocations)
                        {
                            var curGameLocation = curImageLocation;
                            var curMinimapLocation = MapleMath.MapCoordinatesToMiniMapCoordinates(curGameLocation, new Vector2(playerScreenLocation.X, playerScreenLocation.Y), CurrentPlayerMinimapLocation);
                            var curMobMapPiece = CurrentMapData.GetCurrentMapPiece(curMinimapLocation);
                            if (!(curMobMapPiece.Beginning.X == playerLocationMapPieceStart.X && curMobMapPiece.Beginning.Y == playerLocationMapPieceStart.Y))
                            {
                                continue;
                            }
                            if (curImageLocation.Y < playerScreenLocation.Y)
                            {
                                continue;
                            } 
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
                    }
                    
                    if (minDistance < distanceTolerance)
                    {
                        totalFailureAmount = 0;
                        CloseMobsStillAlive = true;
                        MobDirectionLeft = isLeft;
                        if (MobDirectionLeft && !CharacterDirectionLeft)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                            CharacterDirectionLeft = true;
                            Thread.Sleep(rand.Next(20, 80));
                            Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                        }
                        else if (!MobDirectionLeft && CharacterDirectionLeft)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                            CharacterDirectionLeft = false;
                            Thread.Sleep(rand.Next(20, 80));
                            Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                        }
                    }
                    else
                    {
                        totalFailureAmount++;
                        if (totalFailureAmount > maxFailureAmount)
                        {
                            totalFailureAmount = 0;
                            minDistance = double.MaxValue;
                            CloseMobsStillAlive = false;
                        }
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
            LocationsOfInterest = new List<Vector2>();
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
            _mobFinderThread = new Thread(NewMobFinderThreadWorker) { Name = "Mob Finder Thread" };
            _mobFinderThread.Start();
            _minimapThread = new Thread(MinimapThreadWorker) { Name = "Minimap Thread" };
            _minimapThread.Start();
            _nearbyMobThread = new Thread(NearbyMobsThreadWorker) { Name = "Nearby Mobs Thread" };
            _nearbyMobThread.Start();
            CurrentMapData.SetupConjoiningMapPiecesBasedOnJumpData(CurrentCharacterData.JumpDataData);
            if (InGameStatus == InGameStatuses.Mobbing)
            {
                
                //_mobFinderThread.Start();
            }
            while (NextMobClusterMinimapLocation == null)
            {
                Thread.Sleep(100);
                if (Closing)
                {
                    return;
                }
            }
            Random rand = new Random();
            while (InGameStatus == InGameStatuses.Mobbing)
            {
                while (CloseMobsStillAlive)
                {
                    Thread.Sleep(300);
                    if (Closing)
                    {
                        return;
                    }
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
                MapPiece targetMapPiece = null;

                MapPiece currentMapPiece = null;
                do
                {
                    targetMapPiece = CurrentMapData.GetCurrentMapPiece(nextMobCluster);
                    LocationsOfInterest.Add(targetMapPiece.Center);
                    currentMapPiece = CurrentMapData.GetCurrentMapPiece(CurrentPlayerMinimapLocation);
                    LocationsOfInterest.Add(currentMapPiece.Center);
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
                        // todo
                        if (curMapPieceIterator != 0 && mapPieceDataList[curMapPieceIterator - 1].MapPieceType == MapPieceTypes.Rope)
                        {
                            Input.StopInput(Input.SpecialCharacters.KEY_UP_ARROW);
                        }
                        var actualPlayerLocation = CurrentMapData.GetCurrentMapPiece(CurrentPlayerMinimapLocation);
                        if (!(actualPlayerLocation.Beginning.X == mapPieceDataList[curMapPieceIterator].Beginning.X && actualPlayerLocation.Beginning.Y == mapPieceDataList[curMapPieceIterator].Beginning.Y))
                        {
                            break;
                        }
                        if (curMapPieceIterator == mapPieceDataList.Count - 1)
                        {

                            if (CurrentPlayerMinimapLocation.X < nextMobCluster.X)
                            {
                                Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                                CharacterDirectionLeft = false;
                                while (CurrentPlayerMinimapLocation.X < nextMobCluster.X)
                                {
                                    Thread.Sleep(50);
                                }
                                Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                            }
                            else if (CurrentPlayerMinimapLocation.X > nextMobCluster.X)
                            {
                                Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                                CharacterDirectionLeft = true;
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
                        JoiningJumpDataPoint joiningJumpData = null;
                        int randJumpSelection = 0;
                        if (curMapPieceLink.JoiningJumpDataList.Where(x => x.JumpType == JumpTypes.JumpDown).Any())
                        {
                            Console.WriteLine("Going to jump down!");
                            var downJumpDataList = curMapPieceLink.JoiningJumpDataList.Where(x => x.JumpType == JumpTypes.JumpDown).ToList();
                            randJumpSelection = rand.Next(0, downJumpDataList.Count);
                            joiningJumpData = downJumpDataList[randJumpSelection];
                        }
                        else
                        {
                            randJumpSelection = rand.Next(0, curMapPieceLink.JoiningJumpDataList.Count);
                            joiningJumpData = curMapPieceLink.JoiningJumpDataList[randJumpSelection];
                        }

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
                            CharacterDirectionLeft = false;
                            while (CurrentPlayerMinimapLocation.X < joiningJumpData.JumpLocation.X)
                            {
                                Thread.Sleep(50);
                            }
                            Input.StopInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                        }
                        else if (CurrentPlayerMinimapLocation.X > joiningJumpData.JumpLocation.X)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                            CharacterDirectionLeft = true;
                            while (CurrentPlayerMinimapLocation.X > joiningJumpData.JumpLocation.X)
                            {
                                Thread.Sleep(50);
                            }
                            Input.StopInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                        }
                        if (joiningJumpData.TurnedLeft)
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_LEFT_ARROW);
                            CharacterDirectionLeft = true;
                        }
                        else
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                            CharacterDirectionLeft = false;
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
                            CharacterDirectionLeft = true;
                        }
                        else
                        {
                            Input.StartInput(Input.SpecialCharacters.KEY_RIGHT_ARROW);
                            CharacterDirectionLeft = false;
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
                } while (CurrentMapData.GetCurrentMapPiece(CurrentPlayerMinimapLocation).Beginning.X != targetMapPiece.Beginning.X && currentMapPiece.Beginning.Y != targetMapPiece.Beginning.Y);
                LocationsOfInterest.Remove(targetMapPiece.Center);
                LocationsOfInterest.Remove(currentMapPiece.Center);
                CloseMobsStillAlive = true;
                ReadyForNewCluster = true;
                if (Closing)
                {
                    return;
                }
            }
            
            
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
