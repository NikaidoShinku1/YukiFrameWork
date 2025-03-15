///=====================================================
/// - FileName:      StateSelectWindow.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 22:51:39
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
namespace YukiFrameWork.Machine
{
    public class SelectStateWindow : PopupWindowContent
    {

        // Fix编码
        #region 字段

        private RuntimeStateMachineCore controller;

        // 搜索框
        private SearchField searchField;
        private Rect searchRect;
        const float searchHeight = 25f;

        // 标签 
        private Rect labelRect;
        const float labelHeight = 30f;

        // 参数列表
        private FSMStateListTree stateTree;
        private TreeViewState stateState;
        private Rect stateRect;

        private Rect rect;

        private StateNodeData nodeData;

        private bool showCreateScriptGUI = false;
        private string scriptName = string.Empty;
        private string scriptName2;
        #endregion


        public SelectStateWindow(Rect rect, RuntimeStateMachineCore controller, StateNodeData nodeData)
        {
            //this.width = width; 
            this.controller = controller;
            this.rect = rect;
            this.nodeData = nodeData;
            this.showCreateScriptGUI = false;
        }


        public override Vector2 GetWindowSize()
        {
            return new Vector2(this.rect.width, this.rect.height);
        }

        public override void OnGUI(Rect rect)
        {
            if (!showCreateScriptGUI)
            {
                OnGUISearchScripts(rect);
            }
            else
            {
                OnGUICreateScripts(rect);
            }

        }

        private void OnGUISearchScripts(Rect rect)
        {
            if (stateTree == null)
            {
                if (stateState == null)
                {
                    stateState = new TreeViewState();
                }

                stateTree = new FSMStateListTree(stateState, controller, this.nodeData, this);
                stateTree.Reload();
            }

            // 搜索框
            if (searchField == null)
            {
                searchField = new SearchField();
            }
            searchRect.Set(rect.x + 5, rect.y + 5, rect.width - 5, searchHeight);
            stateTree.searchString = searchField.OnGUI(searchRect, stateTree.searchString);

            // 标签 
            labelRect.Set(rect.x, rect.y + searchHeight, rect.width, labelHeight);
            EditorGUI.LabelField(labelRect, "StateBehaviours", GUI.skin.GetStyle("AC BoldHeader"));

            // 参数列表 

            stateRect.Set(rect.x, rect.y + searchHeight + labelHeight - 5, rect.width, rect.height - searchHeight - labelHeight - 20);
            stateTree.OnGUI(stateRect);

            stateRect.Set(rect.x, stateRect.y + stateRect.height, rect.width, 23);
            if (GUI.Button(stateRect, "创建 状态脚本", "AppToolbarButtonMid"))
            {
                showCreateScriptGUI = true;
                scriptName = string.Empty;
            }
        }

        private void OnGUICreateScripts(Rect rect)
        {

            EditorGUI.BeginDisabledGroup(true);

            // 搜索框
            if (searchField == null)
            {
                searchField = new SearchField();
            }
            searchRect.Set(rect.x + 5, rect.y + 5, rect.width - 5, searchHeight);
            stateTree.searchString = searchField.OnGUI(searchRect, stateTree.searchString);

            EditorGUI.EndDisabledGroup();

            // 标签
            labelRect.Set(rect.x, rect.y + searchHeight, rect.width, labelHeight);

            if (GUI.Button(labelRect, "New Script", "AC BoldHeader"))
            {
                showCreateScriptGUI = false;
            }
            labelRect.y -= 3;
            GUI.Label(labelRect, EditorGUIUtility.IconContent("ArrowNavigationLeft"));

            // 参数列表 

            stateRect.Set(rect.x, rect.y + searchHeight + labelHeight - 5, rect.width, rect.height - searchHeight - labelHeight - 20);

            GUILayout.BeginArea(stateRect);
            GUILayout.Space(10);
            GUILayout.Label("Name");
            EditorGUI.BeginChangeCheck();

            scriptName2 = EditorGUILayout.DelayedTextField(scriptName);

            if (EditorGUI.EndChangeCheck())
            {
                scriptName = scriptName2;
                EditorApplication.delayCall += CreateAndAddScript;
            }
            //GUILayout.Label(string.Format("scriptName:{0} scriptName2:{1}", scriptName, scriptName2));
            GUILayout.EndArea();

            stateRect.Set(rect.x, rect.height - 23, rect.width, 23);
            if (GUI.Button(stateRect, "创建并添加!", "AppToolbarButtonMid"))
            {
                OnButtonClick();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                EditorApplication.delayCall += CreateAndAddScript;
            }

        }

        private void Update()
        {

            if (!showCreateScriptGUI && editorWindow != null)
                editorWindow.Repaint();

            if (EditorWindow.focusedWindow != editorWindow)
            {
                EditorApplication.update -= Update;
            }
        }


        public override void OnOpen()
        {
            base.OnOpen();
            EditorApplication.update += Update;
        }

        public override void OnClose()
        {
            base.OnClose();
            EditorApplication.update -= Update;
        }

        public void Close()
        {
            EditorApplication.update -= Update;
            if (editorWindow == null)
                return;

            editorWindow.Close();
        }


        private void CreateAndAddScript()
        {

            if (EditorWindow.focusedWindow != editorWindow)
                return;

            if (string.IsNullOrEmpty(scriptName2))
                return;
            StateGraphView.SavePrefsSelection(this.nodeData.name);
            string dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(controller));
            string path = string.Format("{0}/{1}.cs", dir, scriptName2);
            bool isSuccess = StateCreator.CreateFSMState(path, true);

            if (!isSuccess)
            {
                GUIContent content = new GUIContent(string.Format("名称:{0}不可用,请修改后重试!", scriptName));
                this.editorWindow.ShowNotification(content);
                return;
            }

            EditorApplication.delayCall += () =>
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                EditorGUIUtility.PingObject(script);
                this.nodeData.AddStateScript(script);
                controller.Save();
                AssetDatabase.SaveAssets();
            };
            //Close();
        }


        private void OnButtonClick()
        {
            GUI.FocusControl(null);
        }

    }

    public class StateCreator : EndNameEditAction
    {

        static string regex = "^[a-zA-Z][a-zA-Z0-9_]*$";

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            CreateFSMState(pathName);
        }


        internal static bool CreateFSMState(string pathName, bool errorTip = true)
        {

            string fileName = Path.GetFileNameWithoutExtension(pathName);
            // 判断文件是否已经存在 如果已经存在则不能创建

            string[] guids = AssetDatabase.FindAssets(fileName);

            foreach (var item in guids)
            {
                string asset_path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(asset_path);
                if (script == null || script.GetClass() == null) continue;
                if (script.GetClass().Name.Equals(fileName))
                {
                    if (errorTip)
                    {
                        string message = string.Format("脚本名称:{0}已经存在!", fileName);
                       // StateMachineEditorWindow.ShowNotification(string.Format("脚本名称:{0}已经存在!", fileName));
                        EditorUtility.DisplayDialog("提示", message, "确定");
                    }
                    return false;
                }
            }

            if (!Regex.Match(fileName, regex).Success)
            {
                EditorUtility.DisplayDialog("提示", string.Format("文件名:{0}不可用!", fileName), "确定");
                return false;
            }

            var content = codeGenerator.BuildFile();
            content = content.Replace("用于替换提示类", fileName);           
            using (FileStream stream = new FileStream(pathName,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite))
            {
                 
                StreamWriter streamWriter = new StreamWriter(stream,Encoding.UTF8);
                streamWriter.Write(content);
                streamWriter.Close();
                stream.Close();
            }

                //stream.Write()
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            return true;
        }

        private static ICodeGenerator codeGenerator = new StateBehaviourGenerator();
    }

   

}
#endif