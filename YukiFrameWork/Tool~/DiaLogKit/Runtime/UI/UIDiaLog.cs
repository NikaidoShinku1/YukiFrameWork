﻿///=====================================================
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
    [DisableViewWarning]
	public class UIDiaLog : MonoBehaviour
	{
        #region TipField
        private const string dialogDes = "对话三件套";
        private const string settingDes = "默认对话器的设置";
        private const string nameDes = "名称组件";
        private const string contextDes = "文本组件";
        private const string iconDes = "对应的图片";

     
        [FoldoutGroup(dialogDes), LabelText(nameDes + "回调")]
        public UnityEvent<string> onNameValueChanged;
        [FoldoutGroup(dialogDes), LabelText(contextDes + "回调")]
        public UnityEvent<string> onContextValueChanged;
        [FoldoutGroup(dialogDes), LabelText(iconDes + "回调")]
        public UnityEvent<Sprite> onSpriteValueChanged;    
        #endregion      
        private bool showTime => playMode == DiaLogPlayMode.Writer;

        [SerializeField,LabelText("文字的播放模式:"), FoldoutGroup(settingDes)]
        internal DiaLogPlayMode playMode = DiaLogPlayMode.Normal;

        internal enum AutoMode
        {
            None,
            MouseDown,
            KeyDown,
            Auto
        }

        [SerializeField,LabelText("打字机模式下的间隔时间"),ShowIf(nameof(showTime)),FoldoutGroup(settingDes)]
        internal float intervalTime = 0.2f;

        [SerializeField,LabelText("默认映射(基础)"), FoldoutGroup(settingDes)]
        [InfoBox("选择模式，映射为None时该组件仅作为注册对话UI更新组件使用，需要接着手动使用DiaLog控制器进行条件判断")]
        internal AutoMode autoMode = AutoMode.MouseDown;
        private bool isMouseDown => autoMode == AutoMode.MouseDown;
        private bool isKeyDown => autoMode == AutoMode.KeyDown;
        [SerializeField,LabelText("键盘映射"), FoldoutGroup(settingDes), ShowIf(nameof(isKeyDown))]
        [InfoBox("什么都不选的情况下运行默认是KeyCode.A")]
        internal KeyCode keyCode;

        [SerializeField,LabelText("鼠标点击下标"),FoldoutGroup(settingDes),MaxValue(2),MinValue(0),ShowIf(nameof(isMouseDown))]
        internal int mouseIndex;

        [SerializeField,LabelText("自动播放间隔时间"),FoldoutGroup(settingDes),ShowIf(nameof(autoMode),AutoMode.Auto)]
        internal float timer = 1.5f;

        [LabelText("自动播放结束后触发事件:"),FoldoutGroup(settingDes), ShowIf(nameof(autoMode), AutoMode.Auto)]
        public UnityEvent onAutomaticSessionEndEvent;
        private DiaLog mDiaLog;

        /// <summary>
        /// 外部注册的对话推进检测，当该事件返回True时会自动推进一次对话，如果该组件开启了自动模式，则自动模式下的映射以及该事件都生效
        /// </summary>
        public event Func<bool> MoveNextCondition = null;

        private bool isMoveNext = true;

        private Coroutine mCurrentCoroutine;
        private Action mCurrentCallBack;       

        private async void Start()
        {
            if (isKeyDown && keyCode == KeyCode.None)
            {
                keyCode = KeyCode.A;
            }
            //安全判定，等待一帧
            await CoroutineTool.WaitForFrame();
            Auto_Reset_MoveNext();
        }

        /// <summary>
        /// 从初始开始进行推进(必须是自动模式且已经调用InitDiaLog方法初始化)
        /// </summary>
        public async void Auto_Reset_MoveNext()
        {
            if (autoMode == AutoMode.Auto)
            {
                CoroutineTokenSource source = CoroutineTokenSource.Create(this);
                await CoroutineTool.WaitUntil(() => mDiaLog != null).Token(source.Token);
                if (mDiaLog.treeState == NodeTreeState.Waiting)
                    mDiaLog.Start();
                while (true)
                {
                    if (mDiaLog == null) 
                        break;

                    Node node = mDiaLog.GetCurrentRuntimeNode();
                    if (!node)
                        break;

                    await CoroutineTool.WaitUntil(() => node.IsCompleted && !node.IsComposite).Token(source.Token);
                    await CoroutineTool.WaitForSeconds(timer).Token(source.Token);

                    MoveNodeState nodeState = mDiaLog.MoveNext();
                    if (nodeState == MoveNodeState.Failed)
                        break;
                }

                onAutomaticSessionEndEvent?.Invoke();
            }
        }

        private void Update_UI(Node node)
        {
            isMoveNext = false;
            onNameValueChanged?.Invoke(node.GetName());
            onSpriteValueChanged?.Invoke(node.GetIcon());

            if (playMode == DiaLogPlayMode.Writer)
            {
                node.IsCompleted = false;
                mCurrentCallBack = OnFinish;
                if (mCurrentCoroutine != null)
                    StopCoroutine(mCurrentCoroutine);
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
            onContextValueChanged?.Invoke(node.GetContext());
        }
        
        private IEnumerator Update_Context(string content)
        {
            string current = string.Empty;
            int index = 0;
            bool enter = autoMode == AutoMode.MouseDown ? Input.GetMouseButtonDown(mouseIndex) : Input.GetKeyDown(keyCode);

            while (index < content.Length)
            {
                current = content.Substring(0, index);
                onContextValueChanged?.Invoke(current);
                yield return CoroutineTool.WaitForSeconds(intervalTime);
                index++;
            }
            
            yield return null;
            mCurrentCallBack?.Invoke();            
        }

        private void Update()
        {
            bool enter = false;
            if (autoMode == AutoMode.Auto)
                return;           
            else if (autoMode == AutoMode.MouseDown)
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

        public void InitDiaLog(DiaLog diaLog)
        {
            if (mDiaLog != null)
            {
                if (mDiaLog == diaLog)
                    return;
                mDiaLog.GlobalRelease();
            }
            mDiaLog = diaLog;           
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
