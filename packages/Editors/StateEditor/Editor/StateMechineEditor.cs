using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.FSM;
using UnityEditor;

namespace YukiFrameWork.FSM.Editors
{
    [CustomEditor(typeof(StateMechine))]
    public class StateMechineEditor : Editor
    {
        private static bool isRemoveBtn;
        private static int currentID;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            StateMechine stateMechine = (StateMechine)target;

            if (stateMechine.stateData != null)
                foreach (var info in stateMechine.stateData)
                {
                    EditorGUILayout.LabelField($"State£º{info.name}");
                    info.name = EditorGUILayout.TextField("×´Ì¬Ãû×Ö:", info.name);
                    info.id = EditorGUILayout.IntField("×´Ì¬id", info.id);
                    info.isTransition = EditorGUILayout.Toggle("ÊÇ·ñÖ´ÐÐ¹ý¶É", info.isTransition);

                    if (info.isTransition)
                    {
                        info.intervalTime = EditorGUILayout.FloatField("¼ä¸ôÊ±¼ä", info.intervalTime);
                    }
                    else info.intervalTime = -1;
                }

            if (GUILayout.Button("Ìí¼Ó×´Ì¬"))
            {
                if (stateMechine.DataCount > 0)
                    stateMechine.AddData(new StateData(stateMechine.DataCount));
                else stateMechine.AddData(new StateData());
            }
            if (!isRemoveBtn)
            {
                isRemoveBtn = GUILayout.Button("É¾³ý×´Ì¬");
            }
            else
            {
                currentID = EditorGUILayout.IntField("ÇëÊäÈëÒªÉ¾³ýµÄ×´Ì¬id", currentID);
                if (GUILayout.Button("É¾³ý"))
                {
                    stateMechine.RemoveData(currentID);
                    isRemoveBtn = false;
                }

                if (GUILayout.Button("È¡Ïû"))
                {
                    isRemoveBtn = false;
                }
            }



        }
    }
}
