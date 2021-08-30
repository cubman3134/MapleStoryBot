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
        public Vector2 DistanceFromOrigin;
        public DateTime TimeSinceBeginning;
    }

    public class JumpData
    {
        public Dictionary<JumpTypes, List<JumpTimestamp>> JumpTypeToJumpTimestamps;
    }
}
