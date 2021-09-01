using System;
using System.Collections.Generic;
using System.Drawing;
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

        int NumMobsAtNextCluster;
        Vector2 NextMobClusterMinimapLocation;
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
                if (numMobs > NumMobsAtNextCluster)
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
                    CurrentPlayerMinimapLocation = MapleMath.PixelToPixelCoordinate(locations[0], miniMapScreen.Width);
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
                    var curGameLocation = MapleMath.PixelToPixelCoordinate(curImageLocation, curScreen.Width);
                    var curMinimapLocation = MapleMath.MapCoordinatesToMiniMapCoordinates(curGameLocation, CurrentMapData);
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

        private List<MapPiece> GetMapPiecesWithinJumpableDistance(MapPiece mapPieceData)
        {
            //List<JumpTimestamp>
            //CurrentCharacterData.JumpDataData.JumpTypeToJumpTimestamps
            return null;
        }

        public void WalkBrain(MapPiece mapTarget, Vector2 vectorTarget)
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
            
        }

        public void GameBrain()
        {
            _mobFinderThread = new Thread(NewMobFinderThreadWorker);
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
            if (InGameStatus == InGameStatuses.Mobbing)
            {
                
                _mobFinderThread.Start();
            }
            while (InGameStatus == InGameStatuses.Mobbing)
            {
                while (CloseMobsStillAlive)
                {
                    if (MobDirectionLeft && !CharacterDirectionLeft)
                    {

                    }
                    else if (!MobDirectionLeft && CharacterDirectionLeft)
                    {

                    }
                    CurrentCharacterData.TryToUseSkill();
                }
                nextMobCluster = NextMobClusterMinimapLocation;
                
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
