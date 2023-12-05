using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukiFrameWork.States
{
    public class LessCompare : IParamaterCompare
    {
        public bool IsMeetCondition(StateParameterData data, float v)
        {
            return data.Value.CompareTo(v) == -1;
        }
    }
}
