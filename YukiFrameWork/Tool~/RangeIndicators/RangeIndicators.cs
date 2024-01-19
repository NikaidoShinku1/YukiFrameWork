using UnityEditor;
using UnityEngine;
using YukiFrameWork.Editorial;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    [ClassAPI("范围指示器")]
    [System.Obsolete("该类是废弃的")]
    public class RangeIndicators : MonoBehaviour
    {
        // [HideInInspector]
        [Range(1, 360)]
        public float angle = 1;

        [Range(1, 10)]
        public float radius = 1;

        public Vector3 offect = Vector3.up;
    }
}
