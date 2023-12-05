using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateInspectorHelper))]
    public class StateInspector : Editor
    {
        private string stateName;
        private readonly List<string> animClipsName = new List<string>();       

        private string filePath = @"Assets/Scripts/";
        private string fileName = "NewStateBehaviour";

        private bool isRecomposeScript = false;
        public override void OnInspectorGUI()
        {
            StateInspectorHelper helper = (StateInspectorHelper)target;
            if (helper == null) return;

            bool disabled = EditorApplication.isPlaying || helper.node.name.Equals(StateConst.entryState);

            EditorGUI.BeginDisabledGroup(disabled);


            EditorGUILayout.BeginHorizontal();           
            EditorGUILayout.EndHorizontal();           
            EditorGUILayout.BeginHorizontal();         
            EditorGUILayout.LabelField("状态下标：", GUILayout.Width(80));
            helper.node.index = EditorGUILayout.IntField(helper.node.index);
            EditorGUILayout.EndHorizontal();

            if (!helper.node.name.Equals(stateName))
            {
                stateName = helper.node.name;
                EditorUtility.SetDirty(helper.StateMechine);
            }
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("动画模式:");
            OnSwitchAnimState(helper.node, helper.node.animData.type);
            helper.node.animData.type = (StateAnimType)EditorGUILayout.EnumPopup(helper.node.animData.type,GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            if (helper.node.animData.type != StateAnimType.None)
            {
                helper.node.animData.isActiveDefaultAnim = EditorGUILayout.ToggleLeft("是否需要当前状态拥有默认动画", helper.node.animData.isActiveDefaultAnim);

                if (helper.node.animData.isActiveDefaultAnim)
                {
                    ModifyAnimData(helper.node, helper.node.animData.type);
                }
            }           
            LoadScriptData(helper.node);

            EditorGUI.EndDisabledGroup();          
        }

        private void LoadScriptData(StateBase state)
        {
            EditorGUILayout.Space(10);          
            for (int i = 0; i < state.dataBases.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                state.dataBases[i].isActive = EditorGUILayout.ToggleLeft(state.dataBases[i].typeName, state.dataBases[i].isActive);

                if (GUILayout.Button("删除脚本"))
                {
                    state.dataBases.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            RecomposeScript(state);
        }

        private void RecomposeScript(StateBase state)
        {
            if (!isRecomposeScript)
            {
                if (GUILayout.Button("添加状态脚本"))
                {
                    isRecomposeScript = true;
                }
            }
            else
            {
                //Type[] types = Assembly.GetAssembly(typeof(StateBehaviour)).GetTypes();
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.FullName.StartsWith("UnityEditor") || assembly.FullName.StartsWith("UnityEngine")
                        || assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Microsoft"))
                        continue;
                    Type[] types = assembly.GetTypes();

                    foreach (var infoType in types)
                    {
                        if (infoType.BaseType == typeof(StateBehaviour))
                        {
                            string typeName = infoType.Namespace + "." + infoType.Name;
                            if (GUILayout.Button(infoType.Name))
                            {
                                if (state.dataBases.Find(x => x.typeName.Equals(typeName)) == null)
                                {
                                    StateDataBase stateDataBase = new StateDataBase()
                                    {
                                        typeName = typeName,
                                        index = state.index,
                                        isActive = true
                                    };
                                    state.dataBases.Add(stateDataBase);
                                }
                                isRecomposeScript = false;
                            }
                        }
                    }
                }
                
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("创建状态脚本路径:");
                filePath = EditorGUILayout.TextField(filePath,GUILayout.Width(350));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("状态脚本名称:");
                fileName = EditorGUILayout.TextField(fileName,GUILayout.Width(350));
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("创建状态脚本"))
                {
                    CreateBehaviourScript();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("取消"))
                {
                    isRecomposeScript = false;
                }
            }
        }

        /// <summary>
        /// 创建脚本文件
        /// </summary>
        private void CreateBehaviourScript()
        {
            string targetPath = filePath + fileName + ".cs";
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(filePath);
                AssetDatabase.Refresh();
            }

            using (FileStream fileStream = new FileStream(filePath + fileName + ".cs", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                TextAsset textAsset = Resources.Load<TextAsset>("StateBehaviourScripts");

                if (textAsset == null)
                {
                    Debug.LogError("配置文件丢失请重新下载框架或自行配置！配置文件名称：" + "StateBehaviourScripts.txt");
                    return;
                }

                string content = textAsset.text;
                content = content.Replace("#SCRIPTNAME#", fileName);              
                StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
                sw.Write(content);

                sw.Close();
                fileStream.Close();
                AssetDatabase.Refresh();
                
            }
        }

        /// <summary>
        /// 修改状态所关联的动画数据
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="type">状态的动画类型</param>
        private void ModifyAnimData(StateBase state, StateAnimType type)
        {
            animClipsName.Clear();
            switch (type)
            {
                case StateAnimType.None:
                    {
                        state.animData.layer = 0;
                        state.animData.animSpeed = 1;
                        state.animData.animLength = 100f;
                    }
                    break;
                case StateAnimType.Animator:
                    {
                        if (state.animData.animator != null && state.animData.animator.runtimeAnimatorController != null)
                        {
                            var animState = state.animData.animator.runtimeAnimatorController;
                            var animClips = animState.animationClips;

                            for (int i = 0; i < animClips.Length; i++)
                            {
                                animClipsName.Add(animClips[i].name);
                            }                 
                        }
                        if (animClipsName.Count == 0)
                        {
                            if (state.animData.animator == null)
                                GUILayout.Label("当前动画没有设置该组件!");
                            else if (state.animData.animator.runtimeAnimatorController == null)
                                GUILayout.Label("当前动画没有设置runtimeAnimatorController!");
                            else GUILayout.Label("当前动画没有设置正确的动画剪辑");
                        }
                    }
                    break;
                case StateAnimType.Animation:
                    {
                        if (state.animData.animation != null)
                        {
                            foreach (AnimationState clip in state.animData.animation)
                            {
                                Debug.Log(clip);
                                animClipsName.Add(clip.clip.name);
                            }
                        }
                        if(animClipsName.Count == 0)
                        {
                            if(state.animData.animation == null)
                                 GUILayout.Label("当前状态没有设置该组件");
                            else GUILayout.Label("当前状态没有设置正确的动画剪辑");
                        }
                    }
                    break;
            }
            EditorGUILayout.Space();
            if (type != StateAnimType.None)
            {
                SetAnim(state);
                state.animData.layer = EditorGUILayout.IntField("动画默认图层", state.animData.layer);
                state.animData.animSpeed = EditorGUILayout.FloatField("动画默认速度", state.animData.animSpeed);
                state.animData.animLength = EditorGUILayout.FloatField("动画默认长度", state.animData.animLength);
            }
        }

        private void SetAnim(StateBase state)
        {
            if (animClipsName.Count > 0)
            {
                state.animData.clipIndex = EditorGUILayout.Popup("默认动画选择", state.animData.clipIndex, animClipsName.ToArray());
                if (animClipsName.Count > 0 && state.animData.clipIndex != -1 && !string.IsNullOrEmpty(animClipsName[state.animData.clipIndex]))
                    state.animData.clipName = animClipsName[state.animData.clipIndex];
            }
        }

        private void OnSwitchAnimState(StateBase state,StateAnimType type)
        {          
            switch (type)
            {
                case StateAnimType.None:
                    {
                        GUILayout.Label("当前模式不会使用Unity动画系统");
                    }
                   
                    break;
                case StateAnimType.Animator:
                    {
                        GUILayout.Label("新版动画:");
                        state.animData.animator = (Animator)EditorGUILayout.ObjectField(state.animData.animator, typeof(Animator), true);                     
                    }
                    break;
                case StateAnimType.Animation:
                    {
                        GUILayout.Label("旧版动画:");
                        state.animData.animation = (Animation)EditorGUILayout.ObjectField(state.animData.animation, typeof(Animation), true);                      
                    }
                    break;              
            }
            EditorGUILayout.Space();
           
        }

        protected override void OnHeaderGUI()
        {
            StateInspectorHelper helper = (StateInspectorHelper)target;
            if (helper == null) return;

            string name = null;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(EditorGUIUtility.IconContent("icons/processed/unityeditor/animations/animatorstate icon.asset"), GUILayout.Width(30), GUILayout.Height(30));
                EditorGUILayout.LabelField("状态名称：", GUILayout.Width(60));
                bool disabled = EditorApplication.isPlaying || helper.node.name.Equals(StateConst.entryState);

                EditorGUI.BeginDisabledGroup(disabled);
                name = EditorGUILayout.DelayedTextField(helper.node.name);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                StateNodeFactory.Rename(helper.StateMechine, helper.node, name);
            }
            EditorGUILayout.Space();
            var rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
          
    }
}
