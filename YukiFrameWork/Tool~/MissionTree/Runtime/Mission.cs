///=====================================================
/// - FileName:      Mission.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/12/2026 7:25:23 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Newtonsoft.Json;
using YukiFrameWork.Extension;
using System.Collections;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
namespace YukiFrameWork.Missions
{

    public enum MissionStatus
    {
        Lock,
        InActive,
        Running,
        Failed,
        Success
    }
    public class Mission : ScriptableObject
    {
        #region Field
        [SerializeField, LabelText("任务标识")]
        [InfoBox("任务Id应该唯一")]
        [JsonProperty] private int missionId;

        [SerializeField, LabelText("任务名称")]
        [JsonProperty] private string missionName;

        [SerializeField, LabelText("任务介绍")]
        [TextArea]
        [JsonProperty] private string description;

        [SerializeField, LabelText("任务图标")]
#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawPreview))]
#endif
        private Sprite icon;

        [JsonProperty, SerializeField, LabelText("任务控制器类型")]
        [ValueDropdown(nameof(AllControllerTypes))]
        [Required("任务控制器类型是不可以为空的!")]
        private string missionControllerType;

        [SerializeField, LabelText("任务目标:"), JsonProperty]
        private List<MissionParam> missionTargetParams = new List<MissionParam>();

        [SerializeField, LabelText("任务完成奖励"), JsonProperty]
        private List<MissionParam> missionAwardParams = new List<MissionParam>();

        [SerializeField,ReadOnly, JsonIgnore, ExcelIgnore]
        private List<Mission> childs = new List<Mission>();
        [SerializeField, ReadOnly, JsonIgnore, ExcelIgnore]
        private List<Mission> parents = new List<Mission>();

        [SerializeField, HideInInspector, JsonProperty]
        internal List<int> missionsChildIds = new List<int>();
        [SerializeField, HideInInspector,JsonIgnore,ExcelIgnore]
        internal MissionTreeSO missionTreeSO;
        [JsonIgnore, ExcelIgnore]
        public System.Action onValidate;
#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        public Position position;
#endif
        #endregion
        #region Property
        [field:JsonIgnore, ExcelIgnore] internal MissionStatus MissionStatus { get; set; }
        [JsonIgnore, ExcelIgnore] public int MissionId { get => missionId; set => missionId = value; }
        [JsonIgnore, ExcelIgnore] public string MissionName { get => missionName; set => missionName = value; }
        [JsonIgnore, ExcelIgnore] public string Description { get => description; set => description = value; }
        [JsonIgnore, ExcelIgnore] public string MissionControllerType { get => missionControllerType; }
        [JsonIgnore, ExcelIgnore] internal List<Mission> Parents => parents;
        [JsonIgnore, ExcelIgnore] internal List<Mission> Childs => childs;
        [JsonIgnore, ExcelIgnore] public bool IsChild => Parents.Count != 0;
        [JsonIgnore, ExcelIgnore] public Sprite Icon { get => icon; set => icon = value; }
        [JsonIgnore, ExcelIgnore] public List<MissionParam> MissionTargetParams => missionTargetParams;
        [JsonIgnore, ExcelIgnore] public List<MissionParam> MissionAwardParams => missionAwardParams;

        [JsonIgnore, ExcelIgnore]
        IEnumerable AllControllerTypes => AssemblyHelper
            .GetTypes(x => x
            .IsSubclassOf(typeof(MissionController)) && !x.IsAbstract)
            .Select(x => new ValueDropdownItem() { Text = x.ToString(),Value = x.ToString()});
        #endregion

        #region Method

        private void OnValidate()
        {
            onValidate?.Invoke();
        }

        internal virtual void ForEachChildrens(Action<Mission> each)
        {
            foreach (var item in childs)
            {
                each?.Invoke(item);
            }
        }

#if UNITY_EDITOR

        private void DrawPreview()
        {
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            GUILayout.Label("任务图标");
            icon = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.icon, typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndHorizontal();
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                this.Save();
            }

        }

        internal virtual void ReLoadChild()
        {
            if (Application.isPlaying) return;
            if (childs != null && childs.Count > 0)
            {
                childs.Sort((a, b) =>
                {
                    if (Math.Abs(a.position.x - b.position.x) > float.Epsilon)
                    {
                        return a.position.x.CompareTo(b.position.x);
                    }

                    return a.MissionId.CompareTo(b.MissionId);
                });
            }
        }

        internal virtual void AddChild(Mission mission)
        {
            if (mission == null) return;

            if (!mission.Parents.Contains(this))
            {
                mission.Parents.Add(this);
                childs.Add(mission);
            }
        }

        internal virtual void RemoveChild(Mission mission)
        {
            if (mission == null) return;
            if (mission.Parents.Contains(this) || childs.Contains(mission))
            {
                mission.Parents.Remove(this);             
                childs.Remove(mission);
            }
        }  

        internal virtual void Clear()
        {
            foreach (var item in childs)
                item.Parents.Remove(this);

            childs.Clear();
        }
        internal void UpdateAllChilds()
        {
            if (missionsChildIds.Count == 0) return;

            foreach (var item in missionsChildIds)
            {
                var mission = missionTreeSO.FindMission(item);
                if (mission)
                    AddChild(mission);
            }
        }

        public virtual void OnInspectorGUI()
        {

        }
#endif
        #endregion



    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Mission), true)]
    [CanEditMultipleObjects]
    public class MissionEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            Mission mission = target as Mission;
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            if (mission)
                mission.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
                mission.Save();
        }
    }
#endif
}
