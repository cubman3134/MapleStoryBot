using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    public enum JumpTypes
    {
        FlashJump,
        DoubleJump,
        Glide
    }

    public class JumpData
    {
        public JumpTypes JumpType;
        public int FullHoldingJumpDistance;
        public int HalfHoldingJumpDistance;
        public int NoHoldingJumpDistance;
    }

    public class SkillData
    {
        public static int DiscrepencyTimeMillis = 150;
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

    class CharacterData
    {
        public static char HealthPotionKey = 'a';
        public static char ManaPotionKey = 's';
        JumpData JumpDataData;
        List<SkillData> SkillDataList;
        public static int InterCharacterDelay = 100;
        public static int InterCharacterRandomDelayMax = 25;
        public bool IsTurnedLeft;
        public void CharacterBrain()
        {
            
        }
    }
}
