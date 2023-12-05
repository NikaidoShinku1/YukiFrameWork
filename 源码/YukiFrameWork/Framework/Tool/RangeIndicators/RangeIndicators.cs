using UnityEditor;
using UnityEngine;

/// <summary>
/// 范围指示器
/// </summary>
public class RangeIndicators : MonoBehaviour
{
   // [HideInInspector]
    [Range(1,360)]
    public float angle = 1;
   
    [Range(1,10)]
    public float radius = 1;  
   
    public Vector3 offect = Vector3.up;
}

