using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    public enum GameStatuses
    {
        GameClosed = 0,
        GameOpening = 1,
        ServerSelection = 2,
        ChoosingChannel = 3,
        CharacterSelection = 4,
        InGame = 5
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
    }
}
