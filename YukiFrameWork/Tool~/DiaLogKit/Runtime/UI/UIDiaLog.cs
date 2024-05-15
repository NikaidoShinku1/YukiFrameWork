///=====================================================
/// - FileName:      UIDiaLog.cs
/// - NameSpace:     YukiFrameWork.DiaLogue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/2 21:39:58
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using YukiFrameWork.Pools;
using UnityEngine.Events;
namespace YukiFrameWork.DiaLogue
{
	public class UIDiaLog : MonoBehaviour
	{
        #region TipField
        private const string dialogDes = "对话三件套";
        private const string settingDes = "默认对话器的设置";
        private const string nameDes = "名称组件";
        private const string contextDes = "文本组件";
        private const string iconDes = "对应的图片";

        [SerializeField, LabelText("是否自定义文本/名称组件进行文本的接收"), FoldoutGroup(dialogDes)]
        private bool IsCustomContextAndName;
         
        [SerializeField,FoldoutGroup(dialogDes),LabelText(nameDes),HideIf(nameof(IsCustomContextAndName))]
		internal Text Name;
        [SerializeField,FoldoutGroup(dialogDes),LabelText(contextDes),HideIf(nameof(IsCustomContextAndName))]
        internal Text Context;

        [SerializeField, FoldoutGroup(dialogDes), LabelText(nameDes + "回调"), ShowIf(nameof(IsCustomContextAndName))]
        internal UnityEvent<string> onNameCallBack;
        [SerializeField, FoldoutGroup(dialogDes), LabelText(contextDes + "回调"), ShowIf(nameof(IsCustomContextAndName))]
        internal UnityEvent<string> onContextCallBack;

        [SerializeField,FoldoutGroup(dialogDes),LabelText(iconDes)]
        internal Image Icon;     

        #endregion
        [LabelText("配置的加载方式"), SerializeField,FoldoutGroup(settingDes)]
        internal DiaLogLoadType loadType = DiaLogLoadType.Inspector;

        [SerializeField,LabelText("注册对话控制器的标识"), FoldoutGroup(settingDes), InfoBox("该标识用于控制器标记以及同步"),ShowIf(nameof(loadType), DiaLogLoadType.Inspector)]
        internal string DiaLogKey;
        [LabelText("对话配置"),ShowIf(nameof(loadType), DiaLogLoadType.Inspector), FoldoutGroup(settingDes), SerializeField]
        internal NodeTree NodeTree;            

        private bool showTime => playMode == DiaLogPlayMode.Writer;

        [SerializeField,LabelText("文字的播放模式:"), FoldoutGroup(settingDes)]
        internal DiaLogPlayMode playMode = DiaLogPlayMode.Normal;

        internal enum AutoMode
        {
            None,
            MouseDown,
            KeyDown
        }

        [SerializeField,LabelText("打字机模式下的间隔时间"),ShowIf(nameof(showTime)),FoldoutGroup(settingDes)]
        internal float intervalTime = 0.2f;

        [SerializeField,LabelText("默认的按键映射(基础)"), FoldoutGroup(settingDes)]
        internal AutoMode autoMode = AutoMode.MouseDown;
        private bool isMouseDown => autoMode == AutoMode.MouseDown;
        private bool isKeyDown => autoMode == AutoMode.KeyDown;
        [SerializeField,LabelText("键盘映射"), FoldoutGroup(settingDes), ShowIf(nameof(isKeyDown))]
        [InfoBox("什么都不选的情况下运行默认是KeyCode.A")]
        internal KeyCode keyCode;

        [SerializeField,LabelText("鼠标点击下标"),FoldoutGroup(settingDes),MaxValue(2),MinValue(0),ShowIf(nameof(isMouseDown))]
        internal int mouseIndex;

        private DiaLog mDiaLog;

        /// <summary>
        /// 外部注册的对话推进检测，当该事件返回True时会自动推进一次对话，如果该组件开启了自动模式，则自动模式下的映射以及该事件都生效
        /// </summary>
        public event Func<bool> MoveNextCondition = null;

        private bool isMoveNext = true;

        private Coroutine mCurrentCoroutine;
        private Action mCurrentCallBack;    

        private void Awake()
        {
            if (loadType == DiaLogLoadType.Inspector)
            {
                if (NodeTree == null)
                {
                    throw new Exception("对话树配置为空，请检查是否正确添加，或者将加载模式改成外部初始化");
                }

                if (string.IsNullOrEmpty(DiaLogKey))
                {
                    throw new Exception("对话控制器绑定标识为空，请检查是否输入，或者将加载模式改成外部初始化");
                }

                DiaLog diaLog = DiaLogKit.CreateDiaLog(DiaLogKey, NodeTree);
                InitDiaLog(diaLog);
            }
        }

        private void Start()
        {
            if (isKeyDown && keyCode == KeyCode.None)
            {
                keyCode = KeyCode.A;
            }
        }

        private void Update_UI(Node node)
        {
            isMoveNext = false;
            if (IsCustomContextAndName)           
                onNameCallBack?.Invoke(node.GetName());           
            else 
                Name.text = node.GetName();
            Icon.sprite = node.GetIcon();
            if (playMode == DiaLogPlayMode.Writer)
            {
                node.IsCompleted = false;
                mCurrentCallBack = OnFinish;
                mCurrentCoroutine = StartCoroutine(Update_Context(node.GetContext()));
            }
            else
            {
                OnFinish();
            }

            void OnFinish()
            {
                InitNodeData(node);
                node.IsCompleted = true;
                isMoveNext = true;
                mCurrentCoroutine = null;
            }
        }

        private void InitNodeData(Node node)
        {
            if (IsCustomContextAndName)
            {                
                onContextCallBack?.Invoke(node.GetContext());
            }
            else
            {                
                Context.text = node.GetContext();
            }
        }
        

        private IEnumerator Update_Context(string content)
        {
            string current = string.Empty;
            int index = 0;
            bool enter = autoMode == AutoMode.MouseDown ? Input.GetMouseButtonDown(mouseIndex) : Input.GetKeyDown(keyCode);

            while (index < content.Length)
            {
                current = content.Substring(0, index);
                if (IsCustomContextAndName)
                    onContextCallBack?.Invoke(current);
                else
                    Context.text = current;              
                yield return CoroutineTool.WaitForSeconds(intervalTime);
                index++;
            }
            
            yield return null;
            mCurrentCallBack?.Invoke();            
        }

        private void Update()
        {
            bool enter = false;
            if (autoMode == AutoMode.MouseDown)
            {
                enter = Input.GetMouseButtonDown(mouseIndex);
            }
            else if (autoMode == AutoMode.KeyDown)
            {
                enter = Input.GetKeyDown(keyCode);
            }

            if ((MoveNextCondition?.Invoke() == true || enter))
            {
                if (isMoveNext)
                    mDiaLog.MoveNext();
                else
                {
                    if (mCurrentCoroutine != null)
                    {
                        StopCoroutine(mCurrentCoroutine);
                        mCurrentCallBack?.Invoke();
                    }
                }
            }
        }

        public void UpdateDiaLogByIndex(int index)
        {
            if (mCurrentCoroutine != null)           
                StopCoroutine(mCurrentCoroutine);          
            mDiaLog.MoveByNodeIndex(index);
        }

#if UNITY_EDITOR
        [Button("更新该对话的进度/下标"),InfoBox("可在编辑器进行对该对话树的回档功能/或者强制跳转")]
        void UpdateDiaLog_Inspector(int loadIndex)
        {
            UpdateDiaLogByIndex(loadIndex);
        }
#endif

        public DiaLog GetCurrentDiaLog()
        {
            if (loadType == DiaLogLoadType.Inspector)
                return mDiaLog;

            if (mDiaLog == null)
            {
                LogKit.E("获取对话控制器失败!当前UIDiaLog不是通过编辑器内部添加的对话树以及标识，请检查是否在外部进行注册");
                return null;
            }
            return mDiaLog;
         
        }

        public void InitDiaLog(DiaLog diaLog)
        {
            if (mDiaLog != null)
            {
                mDiaLog.GlobalRelease();
            }
            mDiaLog = diaLog;
            DiaLogKey = diaLog.DiaLogKey;
            mDiaLog.RegisterWithNodeEnterEvent(Update_UI);
            mDiaLog.Start();
        }

        private void OnDestroy()
        {
            MoveNextCondition = null;
            mDiaLog.GlobalRelease();
        }
    }
}
