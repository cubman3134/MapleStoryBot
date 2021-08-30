using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maple.Data
{
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
        public JumpData JumpDataData;
        List<SkillData> SkillDataList;
        int CharacterSelectionLocation;

        public bool TryToUseSkill()
        {
            var curSkill = SkillDataList.OrderBy(x => x.NextUseTime).First();
            if (curSkill.NextUseTime < DateTime.Now)
            {
                curSkill.UseSkill();
                return true;
            }
            return false;
        }
    }
}
