#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.AI;
using YukiFrameWork.DrawEditor;
namespace YukiFramework.AI.Editor
{
    public class AIServicesConfigWindow : DrawConfigEditorWindowBase<AIServicesConfig>
    {
        
        protected override Type ConfigItemBaseType => typeof(AIServicesInfo);
        protected override void OnCreateItem(Type type, GenericMenu menu)
        {
            menu.AddItem(new GUIContent($"添加新的AI服务"), false, () =>
            {
                tBase.CreateServicesData();
                tBase.onValidate?.Invoke();
                AssetDatabase.Refresh();
            });
        }

        protected override GUIContent DisableItem()
        {
            return new GUIContent("添加新的AI服务");
        }

        protected override void OnDelete(OdinMenuItem item)
        {
            AIServicesInfo info = item.Value as AIServicesInfo;
            if (info)
            {
                tBase.DeleteServicesData(info);
                tBase.onValidate?.Invoke();
                AssetDatabase.Refresh();
         
            }
        }
        
        protected override void OnImGUI()
        {
            base.OnImGUI();
            if (CheckMenuTreeNullOrEmpty()) return;
            
            foreach (var item in MenuTree.MenuItems)
            {
                var info = item.Value as AIServicesInfo; 
                if (!info) continue;

                item.Name = $"{info.id}_{info.GetInstanceID()}";

            }
        }   


        protected override void ConfigRefresh()
        {
            tBase.onValidate = () =>
            {
                if(Instance)
                    Instance.ForceMenuTreeRebuild();
            };
            base.ConfigRefresh();
        }

        protected override void Update_ConfigBase(OdinMenuTree odinMenuTree)
        {
            if (tBase.aiServicesInfos.Count > 0)
            {
                for (int i = tBase.aiServicesInfos.Count - 1; i >= 0; i--)
                {
                    if (tBase.aiServicesInfos[i]) continue;
                    tBase.DeleteServicesData(i);
                }
            }
            foreach (var skill in tBase.aiServicesInfos)
            {
                if (!skill) continue;
                odinMenuTree.Add($"{skill.id}_{skill.GetInstanceID()}",skill, Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }          
        }

        protected override string SELECT_GUID_KEY => "AISERVICESKIT_CONFIGWINDOW";

        public static void ShowWindow()
        {
            var window = GetWindow<AIServicesConfigWindow>();
            window.titleContent = new GUIContent("AI服务配置窗口");
            window.Show();
        }
    }
}
#endif