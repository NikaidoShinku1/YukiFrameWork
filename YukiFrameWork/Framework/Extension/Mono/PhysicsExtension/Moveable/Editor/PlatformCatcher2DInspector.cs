using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using YukiFrameWork.Platform2D;


namespace YukiFrameWork.Platform2D
{

    [CustomEditor(typeof(PlatformCatcher2D))]
    public class PlatformCatcher2DInspector : OdinEditor
    {
        private PlatformCatcher2D catcher;
         
        private Vector3 snap = new Vector3 (0.1f, 0.1f, 0.01f);

        protected override void OnEnable()
        {
            base.OnEnable();
            catcher = target as PlatformCatcher2D;
        }

        private void OnSceneGUI()
        {

            if (catcher.areas == null || catcher.areas.Length == 0)
                return;

            serializedObject.Update();

            for (int i = 0; i < catcher.areas.Length; i++)
            {
                Area item = catcher.areas[i];

                Vector3 from = catcher.transform.localToWorldMatrix.MultiplyPoint(item.from);
                Vector3 to = catcher.transform.localToWorldMatrix.MultiplyPoint(item.to);

                float size = HandleUtility.GetHandleSize(to) * 0.1f;

                Handles.color = Color.green;

                EditorGUI.BeginChangeCheck(); 
                var fmh_44_53_638531341894066023 = Quaternion.identity; from = Handles.FreeMoveHandle(from, size, snap, Handles.CubeHandleCap);

                if (EditorGUI.EndChangeCheck()) 
                { 
                    item.from = catcher.transform.worldToLocalMatrix.MultiplyPoint(from);
                    catcher.areas[i] = item;
                    EditorUtility.SetDirty(target);
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.BeginChangeCheck();

                
                
                var fmh_58_49_638531341894079935 = Quaternion.identity; to = Handles.FreeMoveHandle(to, size, Vector3.one * 0.1f, Handles.CubeHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    item.to = catcher.transform.worldToLocalMatrix.MultiplyPoint(to);
                    catcher.areas[i] = item;
                    EditorUtility.SetDirty(target);
                    serializedObject.ApplyModifiedProperties();
                }

            }
 

        }

    }
}

#endif