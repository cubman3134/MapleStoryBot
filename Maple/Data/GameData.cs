using System;
using System.Collections.Generic;
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
        CharacterSelection = 3,
        InGame = 4
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

        public static void GetGameStatus()
        {

        }
    }
}
