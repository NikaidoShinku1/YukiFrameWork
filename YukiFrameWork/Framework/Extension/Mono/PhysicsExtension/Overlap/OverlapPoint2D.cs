using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace YukiFrameWork.Overlap 
{
    /// <summary>
    /// 重叠检测某一个点
    /// </summary>
    public class OverlapPoint2D : OverlapBase2D
    {
        // Fix编码
        public override void CheckOverlap()
        {
            ClearResults();
            int count = Physics2D.OverlapPointNonAlloc(transform.position, results, layerMask.value);
            for (int i = 0; i < count; i++)
            {
                if (results[i].isTrigger) continue;
                AddResults(results[i],transform.position); 
            }

            OverlapEnd(); 
        }
    }
    
}

