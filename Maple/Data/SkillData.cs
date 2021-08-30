using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maple.Data
{
    public class SkillData
    {
        public string SkillName { get; set; }
        public int DiscrepencyTimeMillis { get; set; }
        private int _currentDiscrepencyTimeMillis;
        public int HoldMillis { get; set; }
        public char Key { get; set; }
        public int RefreshMillis { get; set; }
        public bool UseOnLogin { get; set; }
        public static Random rand = new Random();
        public DateTime NextUseTime
        {
            get { return LastUseTime + new TimeSpan(0, 0, 0, 0, RefreshMillis + _currentDiscrepencyTimeMillis); }
        }

        public DateTime LastUseTime;

        public void UseSkill()
        {
            Input.StartInput(Key);
            Thread.Sleep(HoldMillis);
            Input.StopInput(Key);
            _currentDiscrepencyTimeMillis = rand.Next(DiscrepencyTimeMillis);
            LastUseTime = DateTime.Now;
        }

        public SkillData(char key, int refreshMillis)
        {



            LastUseTime = DateTime.Now;

        }
    }
}
