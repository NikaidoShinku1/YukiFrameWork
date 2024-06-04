
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.Overlap 
{
    [CustomEditor(typeof(OverlapBase3D),true)]
    public class OverlapBase3DInpector : OdinEditor
    {
             
        private OverlapBase3D overlap;

        private bool foldout;

        // Fix编码 

        protected override void OnEnable()
        {
            base.OnEnable();
            
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
           
            DrawResult(ref foldout, overlap.Results);

        }

        public static void DrawResult(ref bool resultFoldout, List<OverlapHit<Collider>> Results)
        {
            resultFoldout = EditorGUILayout.Foldout(resultFoldout, "Results");
            if (resultFoldout)
            {
                GUI.color = Color.gray;

                if (Results.Count == 0)
                    EditorGUILayout.LabelField("Empty!");

                foreach (var item in Results)
                {
                    EditorGUILayout.LabelField("name", item.collider.name);
                    //EditorGUIUtility.PingObject(item.collider); 
                    EditorGUILayout.Vector3Field("point", item.point);

                    if (GUILayout.Button("select"))
                        EditorGUIUtility.PingObject(item.collider);

                }
            }
        }
    }
}
#endif
