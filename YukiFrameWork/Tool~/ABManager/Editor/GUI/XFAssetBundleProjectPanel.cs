using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


namespace XFABManager
{
    public class XFAssetBundleProjectPanel  : ScriptableObject
    {

        public new string name = string .Empty;
        private string lastName;
        public string displayName;
 

        [SerializeField]
        public List<XFABProject> dependenceProjects;        // 依赖的项目

        public string suffix = ".unity3d";
        private string lastSuffix;
        public string version = "1.0.0";
        private string lastVersion;

        private string secret_key = string.Empty;

        //序列化对象
        private SerializedObject _serializedObject;
        //序列化属性
        private SerializedProperty _assetLstProperty;

        private VerifyResult verifyNameResult;
        private VerifyResult verifySuffixResult;
        private VerifyResult verifyVersionResult;
        private VerifyResult verifyDependenceResult;

        private GUIStyle tipStyle;
        private GUIStyle tipDownStyle;
        private GUIStyle buttonStyle;
        private GUIStyle searchButtonStyle;

        private GUIContent nameLabel;
        private GUIContent displayNameLabel;
        private GUIContent suffixLabel;
        private GUIContent versionLabel;
        private GUIContent outputLabel;
        private GUIContent dependenceProjectLabel;
        private GUIContent secretKeyLabel;


        string nameMatch = "^[A-Za-z0-9-_]+$";
        string suffixMatch = "^[.][A-Za-z0-9]+$";
        string versionMatch = "^[1-9][.0-9]*$"; // 以数字开头 中间只能有数字 和 . 

        private XFABProject project;
        private EditorWindow window;


        public static XFAssetBundleProjectPanel CreatePanel(EditorWindow window)  
        {
            XFAssetBundleProjectPanel panel = CreateInstance<XFAssetBundleProjectPanel>();

            panel.InitProjects();
            panel.window = window;

            // 默认验证依赖项目
            panel.VerifyDependenceProject();

            return panel;
        }
        public static XFAssetBundleProjectPanel CreatePanel(EditorWindow window,XFABProject project)  
        {
            XFAssetBundleProjectPanel panel = CreateInstance<XFAssetBundleProjectPanel>();

            panel.InitProjects();
            panel.InitProjectToThis(project);
            panel.window = window;

            // 默认验证依赖项目
            panel.VerifyDependenceProject();

            return panel;
        }

        public void OnGUI()
        {

            GUILayout.Space(30);
            //画出 name
            DrawName();
            GUILayout.Space(20);
            //显示名
            DrawDisplayName();

            GUILayout.Space(20);
            // 画出依赖项目
            DrawDependenceProjects();
            // 画出后缀
            GUILayout.Space(20);
            DrawSuffix();
            GUILayout.Space(20);
            DrawVersion();

            // 绘制秘钥
            GUILayout.Space(20);
            DrawSecretKey();

        }


        #region OnGUI

        // 画出 Name
        private void DrawName()
        {

            if (!name.Equals(lastName))
            {
                VerifyProjectName();
                lastName = name;
            }

            DrawTextFieldLine(nameLabel, ref name, string.IsNullOrEmpty(name) ? string.Empty : verifyNameResult.GetMessageWithColor(), "*必填");

        }

        // 画出 DisplayName
        private void DrawDisplayName()
        {
            DrawTextFieldLine(displayNameLabel, ref displayName, "*显示名称,非必须,可填任意字符!", "*非必填");
        }

        
        // 画出依赖项目
        private void DrawDependenceProjects()
        {

            GUILayout.BeginHorizontal();
            //GUILayout.Label("依赖项目:", GUILayout.Width(145));
            //更新
            _serializedObject.Update();

            //开始检查是否有修改
            EditorGUI.BeginChangeCheck();

            //显示属性
            //第二个参数必须为true，否则无法显示子节点即List内容
            EditorGUILayout.PropertyField(_assetLstProperty , dependenceProjectLabel, true,GUILayout.Width(400));

            //结束检查是否有修改
            if (EditorGUI.EndChangeCheck())
            {
                //提交修改
                _serializedObject.ApplyModifiedProperties();
                //Debug.Log(" 提交修改! ");
                // 验证依赖的项目是否合格
                VerifyDependenceProject();

            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Space(150);
            GUILayout.Label(string.Format(
                "*准备资源时(AssetBundleManager.ReadyRes),会同时准备其依赖项目的资源!\n{0}", 
                "*只能单项依赖 例如:A依赖B 则B就不能依赖A"), tipDownStyle);
            GUILayout.EndHorizontal();

            // 当 下载 更新 加载 卸载 项目时 会同时对其依赖的项目进行操作 
            // 只能单项依赖 例如:A依赖B 则B就不能依赖A 

        }

        // 画出后缀
        private void DrawSuffix()
        {
            if (!suffix.Equals(lastSuffix))
            {
                VerifySuffix();
                lastSuffix = suffix;
            }

            DrawTextFieldLine(suffixLabel, ref suffix, string.IsNullOrEmpty(suffix) ? string.Empty : verifySuffixResult.GetMessageWithColor(), "*必填");
        }

        // 画出版本
        private void DrawVersion()
        {
            if (!version.Equals(lastVersion))
            {
                VerifyVersion();
                lastVersion = version;
            }

            DrawTextFieldLine(versionLabel, ref version, string.IsNullOrEmpty(version) ? string.Empty : verifyVersionResult.GetMessageWithColor(), "*必填");
        }

        // 绘制秘钥
        private void DrawSecretKey() {

            string tip = string.Empty;

            if (!string.IsNullOrEmpty(secret_key)) {
                tip = "<color=red>加密的AssetBundle在加载之前请调用 <color=green> AssetBundleManager.SetSecret(string projectName, string secret);</color>  设置秘钥,否则无法正常加载资源!</color> \n <color=yellow> 加载加密的AssetBundle,会造成额外的内存消耗!请根据项目确认是否需要加密!</color>";
            }
             

            DrawTextFieldLine(secretKeyLabel, ref secret_key, "默认不加密!", tip);
            GUILayout.BeginHorizontal();
            GUILayout.Space(150);
            if (GUILayout.Button("随机生成", GUILayout.Width(100))) {

                string tipMessage = "是否生成秘钥?若生成,已上线的旧版本的资源将无法加载!";

                if (EditorUtility.DisplayDialog("提示", tipMessage, "确认", "取消")) {
                    GUI.FocusControl(null);
                    secret_key = System.Guid.NewGuid().ToString().Substring(0, 10); // 55c9a3db-3b7a-40c0-b8a5-9a8294a168
                }
            }
            GUILayout.EndHorizontal();
             

        }

        private void DrawTextFieldLine(GUIContent label,ref string filedValue,string tip,string downTip = "") {

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            filedValue = EditorGUILayout.TextField(label, filedValue,GUILayout.Width(400));

            GUILayout.Label(tip, tipStyle, GUILayout.Width(190), GUILayout.Height(20));

            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(downTip))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(155);
                GUILayout.Label(downTip, tipDownStyle);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        #endregion

        #region 方法

        // 配置样式
        private void ConfigStyle()
        {
            tipStyle = new GUIStyle();
            tipStyle.fontSize = 10;
            tipStyle.alignment = TextAnchor.MiddleLeft;
            tipStyle.normal.textColor = Color.grey;
            tipStyle.richText = true;
            tipDownStyle = new GUIStyle();
            tipDownStyle.fontSize = 10;
            tipDownStyle.alignment = TextAnchor.MiddleLeft;
            tipDownStyle.normal.textColor = Color.grey;
            tipDownStyle.richText = true;
            tipDownStyle.margin.left = 110;

            buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));


            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.margin.left = 50;
            buttonStyle.margin.right = 50;

            searchButtonStyle = new GUIStyle(GUI.skin.GetStyle("button"));
            searchButtonStyle.alignment = TextAnchor.MiddleCenter;
            searchButtonStyle.margin.left = 0;
            searchButtonStyle.margin.right = 0;
        }

        // 配置 Label Content
        private void ConfigLabelContent() {

            nameLabel = new GUIContent("项目名:","*项目名要唯一 且 只能为英文 和 数字!");
            displayNameLabel = new GUIContent("显示名:","*显示名称,选填");
            suffixLabel = new GUIContent("AssetBundle后缀:", "*后缀名,默认为.unity3d!");
            versionLabel = new GUIContent("版本:", "*版本,默认为1.0.0!");
            outputLabel = new GUIContent("输出目录:","AB包存放目录!");
            dependenceProjectLabel = new GUIContent("依赖项目");
            secretKeyLabel = new GUIContent("秘钥","为空则不加密!");
        }

        // 配置验证结果
        private void ConfigVerifyResult()
        {
            verifyNameResult = new VerifyResult("项目名");
            verifySuffixResult = new VerifyResult("后缀");
            verifyVersionResult = new VerifyResult("版本");
            verifyDependenceResult = new VerifyResult("依赖项目");
        }

        // 初始化项目名称
        private void InitProjects()
        {
            ConfigStyle();
            ConfigVerifyResult();
            ConfigLabelContent();

            //outputPath = Application.dataPath.Replace("Assets", "AssetBundles");

            dependenceProjects = new List<XFABProject>();

            _serializedObject = new SerializedObject(this);
            ////获取当前类中可序列话的属性
            _assetLstProperty = _serializedObject.FindProperty("dependenceProjects");
        }

        // 初始化 Project 
        private void InitProjectToThis(XFABProject project) {

            name = project.name;
            displayName = project.displayName;
            //outputPath = project.out_path;
            //panel.dependenceProjects = new List<XFABProject>();

            for (int i = 0; i < project.dependenceProject.Count; i++)
            {
                XFABProject xFABProject = XFABProjectManager.Instance.GetProject(project.dependenceProject[i]);
                if (xFABProject != null)
                {
                    dependenceProjects.Add(xFABProject);
                }
            }

            suffix = project.suffix;
            version = project.version;
            secret_key = project.secret_key;
            this.project = project;
        }


        public void SaveProject() {

            if (!IsAllVerifyPass()) {

                this.window.ShowNotification(new GUIContent("请完善项目信息!"));
                return;
            }

            if (project != null)
            {
                project.name = name;
                project.displayName = displayName;
                //project.out_path = outputPath;
                project.dependenceProject.Clear();
                for (int i = 0; i < dependenceProjects.Count; i++)
                {
                    project.dependenceProject.Add(dependenceProjects[i].name);
                }

                project.suffix = suffix;
                project.version = version;
                project.secret_key = secret_key;
                project.Save();
            }
            else {
                Debug.Log("project is null!");
            }

        }

        #endregion

        #region 验证方法

        // 验证项目名称是否合法
        private void VerifyProjectName()
        {

            VerifyItem(name, nameMatch, "只能为英文 数字 - 和 _ !",  verifyNameResult);
            if (verifyNameResult.isPass == false)
            {
                return;
            }


            if (project == null)
            {
                // 新建项目 
                // 判断是不是已经有这个项目 只有在 project 为空( 新建项目的时候需要判断 )的情况下 ，
                if (XFABProjectManager.Instance.IsContainProject(name))
                {
                    verifyNameResult.message = "名称已存在!";
                    verifyNameResult.isPass = false;
                    return;
                }
            }
            else if (!project.name.Equals(name))
            {
                // 修改项目
                // 判断是不是已经有这个项目 只有在 project 为空( 新建项目的时候需要判断 )的情况下 ，
                if (XFABProjectManager.Instance.IsContainProject(name))
                {
                    verifyNameResult.message = "名称已存在!";
                    verifyNameResult.isPass = false;
                    return;
                }

            }



            verifyNameResult.message = "✔名称可用";
        }

        // 验证 后缀
        private void VerifySuffix()
        {
            VerifyItem(suffix, suffixMatch, "必须以 . 开头 且 只能为英文 和 数字!",  verifySuffixResult);
            if (verifySuffixResult.isPass == false)
            {
                return;
            }

            verifySuffixResult.message = "后缀名,默认为.unity3d!";
        }

        // 验证版本
        private void VerifyVersion()
        {
            VerifyItem(version, versionMatch, "*必须以数字开头 且 只能为 . 和 数字!",  verifyVersionResult);
            if (verifyVersionResult.isPass == false)
            {
                return;
            }

            verifyVersionResult.message = "*版本,默认为1.0.0!";

        }


        private void VerifyItem(string value, string regex, string failedTip, VerifyResult result)
        {

            if (string.IsNullOrEmpty(value))
            {
                result.message = "不能为空!";
                result.isPass = false;
                return;
            }

            if (!Regex.IsMatch(value, regex))
            {

                result.message = failedTip;
                result.isPass = false;
                return;
            }

            result.isPass = true;

        }

        // 验证依赖项目
        private void VerifyDependenceProject()
        {

            // 验证有没有重复的 空的
            for (int i = 0; i < dependenceProjects.Count; i++)
            {
                for (int j = i + 1; j < dependenceProjects.Count; j++)
                {
                    if (dependenceProjects[i] != null && dependenceProjects[j] != null && dependenceProjects[i].name == dependenceProjects[j].name)
                    {
                        verifyDependenceResult.message = "请删除重复的依赖项目!";
                        verifyDependenceResult.isPass = false;
                        return;
                    }
                }
            }

            if (project != null)
            {



                // 验证是否是单向依赖
                for (int i = 0; i < dependenceProjects.Count; i++)
                {
                    if (dependenceProjects[i] != null)
                    {
                        if (dependenceProjects[i].IsDependenceProject(project.name) || dependenceProjects[i].name.Equals(project.name))
                        {
                            verifyDependenceResult.message = string.Format("项目依赖 {0} 出错,只能单向依赖!", dependenceProjects[i].name);
                            verifyDependenceResult.isPass = false;
                            return;
                        }
                    }
                }
            }

            verifyDependenceResult.isPass = true;

        }

        // 是不是验证通过 
        public bool IsAllVerifyPass() {

            return verifyNameResult.isPass && verifySuffixResult.isPass && verifyVersionResult.isPass && verifyDependenceResult.isPass;
        }

        // 获取验证失败的原因
        public string GetErrorMesssage() {
            if (!verifyNameResult.isPass) {
                return verifyNameResult.GetErrorMessage();
            }
            if (!verifySuffixResult.isPass)
            {
                return verifySuffixResult.GetErrorMessage();
            }
            if (!verifyVersionResult.isPass)
            {
                return verifyVersionResult.GetErrorMessage();
            }
            if (!verifyDependenceResult.isPass)
            {
                return verifyDependenceResult.GetErrorMessage();
            }
            return string.Empty;
        }

        #endregion




    }

    public class VerifyResult {
        private string name;
        public bool isPass;
        public string message;

        public VerifyResult(string name) {
            this.name = name;
        }

        public string GetMessageWithColor() {
            return string.Format("<color={0}>{1}</color>", isPass ? "#00FF25" : "#FF2700" , message);
        }

        public string GetErrorMessage() {

            if (!isPass) {
                return string.Format("{0} {1}",name,message);
            }

            return string.Empty;
        }

    }

}

