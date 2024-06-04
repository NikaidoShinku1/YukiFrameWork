#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.Overlap 
{
    [CustomEditor(typeof(OverlapBase2D),true)]
    public class OverlapBase2DInpector : OdinEditor
    {     
        private OverlapBase2D overlap;

        private bool foldout;

        // Fix编码 

        protected override void OnEnable() {
            base.OnEnable();        

            overlap = serializedObject.targetObject as OverlapBase2D;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawResult(ref foldout, overlap.Results);

        }

        public static void DrawResult(ref bool resultFoldout,List<OverlapHit<Collider2D>> Results) {
            resultFoldout = EditorGUILayout.Foldout(resultFoldout, "Results");
            if (resultFoldout)
            {
                GUI.color = Color.gray;

                if ( Results.Count == 0)
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