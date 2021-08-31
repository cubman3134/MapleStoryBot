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

    public class JumpData
    {
        // int is intercharacter delay
        public Dictionary<Tuple<JumpTypes, int>, List<double>> EquationCoefficients { get; set; }

        public JumpData()
        {
            EquationCoefficients = new Dictionary<Tuple<JumpTypes, int>, List<double>>();
        }
    }
}
