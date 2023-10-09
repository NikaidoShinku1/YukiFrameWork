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
                    EditorGUILayout.LabelField($"State��{info.name}");
                    info.name = EditorGUILayout.TextField("״̬����:", info.name);
                    info.id = EditorGUILayout.IntField("״̬id", info.id);
                    info.isTransition = EditorGUILayout.Toggle("�Ƿ�ִ�й���", info.isTransition);

                    if (info.isTransition)
                    {
                        info.intervalTime = EditorGUILayout.FloatField("���ʱ��", info.intervalTime);
                    }
                    else info.intervalTime = -1;
                }

            if (GUILayout.Button("���״̬"))
            {
                if (stateMechine.DataCount > 0)
                    stateMechine.AddData(new StateData(stateMechine.DataCount));
                else stateMechine.AddData(new StateData());
            }
            if (!isRemoveBtn)
            {
                isRemoveBtn = GUILayout.Button("ɾ��״̬");
            }
            else
            {
                currentID = EditorGUILayout.IntField("������Ҫɾ����״̬id", currentID);
                if (GUILayout.Button("ɾ��"))
                {
                    stateMechine.RemoveData(currentID);
                    isRemoveBtn = false;
                }

                if (GUILayout.Button("ȡ��"))
                {
                    isRemoveBtn = false;
                }
            }



        }
    }
}
