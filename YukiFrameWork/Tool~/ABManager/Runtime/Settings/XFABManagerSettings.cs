using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace YukiFrameWork.XFABManager
{
    [Serializable]
    // 项目显示模式
    public enum ProjectsShowMode{
        Grid,  // 以格子的形式显示
        List   // 以列表的形式显示
    }

    public class XFABManagerSettings : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        private int _selectIndex;
        /// <summary>
        /// 当前选择的配置组的下标（只读）
        /// </summary>
        public int SelectIndex {
            get {
                return _selectIndex;
            }

            private set 
            {
                _selectIndex = value;
                Save();
            }

        }
        
        /// <summary>
        /// 所有的配置列表
        /// </summary>
        [HideInInspector]
        public List<Profile> Profiles = new List<Profile>();

        /// <summary>
        /// 所有的配置分组
        /// </summary>
        //[HideInInspector]
        public List<string> Groups = new List<string>() { "Default" };


        private static XFABManagerSettings _instance;

        public static XFABManagerSettings Instances {
            get {
                if (_instance == null) 
                {
                    // 通过Resource 加载
                    _instance = Resources.Load<XFABManagerSettings>(typeof(XFABManagerSettings).Name);

                    if (_instance == null) {

                        // 判断是不是编辑器模式
#if UNITY_EDITOR
                        _instance = ScriptableObject.CreateInstance<XFABManagerSettings>();
                        _instance.SelectIndex = 0;

                        Profile defualtProfile = new Profile(); // 默认配置 
                        _instance.Profiles.Add(defualtProfile);


                        string path = string.Format("Assets/Resources/{0}.asset",  typeof(XFABManagerSettings).Name); 
                        string dir = System.IO.Path.GetDirectoryName(path); 
                        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

                        AssetDatabase.CreateAsset(_instance, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
#else
                        throw new Exception("配置缺失:XFABManagerSettings!");
#endif 
                    } 
                }
                return _instance;
            }
        }

        /// <summary>
        /// 格子的显示模式(仅在编辑器模式下有效)
        /// </summary>
        public ProjectsShowMode ShowMode;

        /// <summary>
        /// 当前正在使用的配置组的所有配置选项
        /// </summary>
        public Profile[] CurrentProfiles {
            get {
                return Profiles.Where(x => x.GroupName.Equals(CurrentGroup)).ToArray();
            }
        }

        /// <summary>
        /// 当前选择的配置组的名称
        /// </summary>
        public string CurrentGroup {
            get {
                if (SelectIndex >= Groups.Count)
                    SelectIndex = 0;
                return Groups[SelectIndex];
            }
        }
  
        /// <summary>
        /// 判断某一个配置组是否含有某一个配置选项
        /// </summary>
        /// <returns></returns>
        public bool IsContainsProfileName(string name,string groupName = "Default") {

            foreach (var item in Profiles)
            {
                if (item.name.Equals(name) && item.GroupName.Equals(groupName)) {
                    return true;
                }
            }

            return false;
        }



        /// <summary>
        /// 设置当前选择的配置组(仅在编辑器模式下有效)
        /// </summary>
        /// <param name="group_name">组的名称</param>
        public void SetTargetGroup(string group_name)
        {
#if UNITY_EDITOR
            if (!Groups.Contains(group_name)) return;
            SetTargetGroup(Groups.IndexOf(group_name));
#endif
        }

        /// <summary>
        /// 设置当前选择的配置组(仅在编辑器模式下有效)
        /// </summary>
        /// <param name="group_index">组的下标</param>
        public void SetTargetGroup(int group_index)
        {
#if UNITY_EDITOR
            if (group_index < 0) 
                group_index = 0;

            if (group_index >= Groups.Count)
                group_index = Groups.Count - 1;
            
            SelectIndex = group_index;
#endif
        }

        /// <summary>
        /// 保存修改(仅在编辑器模式下有效)
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }



    }

}