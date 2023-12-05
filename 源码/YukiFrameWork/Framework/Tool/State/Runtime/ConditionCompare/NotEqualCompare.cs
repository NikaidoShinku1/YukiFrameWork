using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukiFrameWork.States
{
    public class NotEqualCompare : IParamaterCompare
    {
        public bool IsMeetCondition(StateParameterData data, float v)
        {
            return !data.Value.Equals(v);
        }
    }
}
