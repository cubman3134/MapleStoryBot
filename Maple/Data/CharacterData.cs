using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    public enum JumpTypes
    {
        UpJumpJump,
        JumpHoldAltAndArrow,
        JumpHoldArrowAltAlt,
        FlashJump
    }

    public class JumpTimestamp
    {
        Vector2 DistanceFromOrigin;
        DateTime TimeSinceBeginning;
    }

    public class JumpData
    {
        Dictionary<JumpTypes, List<JumpTimestamp>> JumpTypeToJumpTimestamps;
    }

    public class SkillData
    {
        public string SkillName;
        public int DiscrepencyTimeMillis;
        public char Key;
        public int RefreshMillis;
        public bool UseOnLogin;
        public DateTime NextUseTime
        {
            get { return LastUseTime + new TimeSpan(0, 0, 0, 0, RefreshMillis); }
        }

        public DateTime LastUseTime;

        public SkillData(char key, int refreshMillis)
        {
            LastUseTime = DateTime.Now - new TimeSpan(1, 0, 0, 0);

        }
    }

    public enum Jobs
    {
        FirePoisonMage,
        AngelicBuster,
        Bowmaster,
        WildHunter,
        Paladin,
        Hero,
        Shadower,
        Zero,
        Evan,
        Buccaneer,
        Bishop,
        BeastTamer,
        Kinesis,
        Luminous,
        NightWalker,
        DemonSlayer,
        IceLightningMage,
        Blaster,
        DemonAvenger,
        BattleMage,
        Shade,
        Illium,
        Ark,
        Pathfinder,
        Kaiser,
        Hayato,
        Aran,
        Phantom,
        BlazeWizard,
        DarkKnight,
        Mechanic,
        NightLord,
        Corsair,
        Cannoneer,
        Marksman,
        Mercedes,
        DualBlade,
        WindArcher,
        DawnWarrior,
        Hoyoung,
        Mihile,
        Cadena,
        ThunderBreaker,
        Jett,
        Xenon,
        Adele,
        Kain,
        Kanna
    }

    class CharacterData
    {
        string CharacterName;
        Jobs CharacterJob;
        int Level;
        JumpData JumpDataData;
        List<SkillData> SkillDataList;
        int CharacterSelectionLocation;
    }
}
