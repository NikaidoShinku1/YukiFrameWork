///=====================================================
/// - FileName:      MissionTreeGraphWindow.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/12/2026 7:14:37 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using YukiFrameWork.Extension;
using System.Collections.Generic;
using System.Linq;




#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEngine;
using System;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
namespace YukiFrameWork.Missions
{
    public class MissionTreeGraphWindow : EditorWindow
    {
        private VisualElement root => rootVisualElement;
        public void CreateGUI()
        {
            var viewTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ImportSettingWindow.GetData().path + "/MissionTree/Editor/GUI/MissionTreeGraphWindow.uxml");
            viewTree.CloneTree(root);
            inspectorView = root.Q<InspectorView>("InspectorView");
            backGroundView = root.Q<BackGroundView>(nameof(BackGroundView));
            backGroundView.onNodeSelected += v =>
            {              
                inspectorView.Update_InspectorView(v);
            };
            onValidate += () =>
            {
                backGroundView.Init(this);
                backGroundView.Refresh(missionTree);
                AssetDatabase.SaveAssets();
            };
            objectField = root.Q<ObjectField>("MissionTreeField");
            objectField.RegisterValueChangedCallback(OnBehaviourChanged);
            objectField.value = missionTree;
            objectField.objectType = typeof(MissionTreeSO);
            dropdownField = root.Q<DropdownField>("RuntimeNode");
            dropdownField.RegisterValueChangedCallback(OnDropDownChanged);
            Button repaintBtn = root.Q<Button>("SaveButton");
            repaintBtn.clicked += () =>
            {
                onValidate?.Invoke();
                Repaint();
            };
            //backGroundView.Init(this);

        }

        private void OnDropDownChanged(ChangeEvent<string> evt)
        {
            if (!Application.isPlaying) return;

            
            string key = evt.newValue;
            if (key.IsNullOrEmpty()) return;
            MissionTree missionTree = MissionTree.GetMissionTree(key);

            objectField.value = missionTree.MissionTreeSO;
        }

        private void OnDisable()
        {
            if (missionTree)
                missionTree.onValidate = null;
        }
        private void OnEnable()
        {
            Undo.undoRedoPerformed += () =>
            {
                backGroundView.Refresh(missionTree);
            };
        }
        private void OnInspectorUpdate()
        {
            Repaint();
        }
        private void OnBehaviourChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            missionTree = evt.newValue as MissionTreeSO;
            if (missionTree != null)
            {
                onValidate?.Invoke();
            }
            else
            {
                backGroundView.Init(this);
                backGroundView.Refresh(missionTree);
            }
        }

        public static event System.Action onUpdate;
        public static event System.Action onValidate;
        private void Update()
        {
            onUpdate?.Invoke();

            if (!objectField.value)
            {
                objectField.value = null;

            }

            if (Application.isPlaying)
            {
                if (dropdownField.childCount != MissionTree.runtime_MissionTreeSO.Count())
                {
                    UpdateRuntimeMissionTree(MissionTree.runtime_MissionTreeSO);
                }
            }
            else if (dropdownField.childCount != 0)
                dropdownField.choices.Clear();
        }

        public void UpdateRuntimeMissionTree(IEnumerable<MissionTree> missionTrees)
        {
            dropdownField.choices = missionTrees.Select(x => x.MissionTreeSO.missionTreeKey).ToList();
        }
        private BackGroundView backGroundView;
        private InspectorView inspectorView;
        internal ObjectField objectField;
        public MissionTreeSO missionTree;
        private DropdownField dropdownField;
        public static void ShowExample(MissionTreeSO missionTree)
        {
            MissionTreeGraphWindow window = GetWindow<MissionTreeGraphWindow>();
            window.titleContent = new GUIContent("任务图编辑器窗口");
            window.missionTree = missionTree;
            window.objectField.value = missionTree;
            window.backGroundView.Init(window);
            window.backGroundView.Refresh(missionTree);
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAssets(int id, int line)
        {
            if (EditorUtility.InstanceIDToObject(id) is MissionTreeSO tree)
            {
                ShowExample(tree);

                return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            AssetDatabase.SaveAssets();
        }


    }
}
#endif