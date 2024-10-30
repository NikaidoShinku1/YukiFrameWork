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
    [DisableViewWarning]
	public class UIDiaLog : MonoBehaviour
	{
        #region TipField
        private const string dialogDes = "对话三件套";
        private const string settingDes = "调用Auto_Reset_MoveNext方法的自动对话器设置";
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

        [SerializeField,LabelText("打字机的间隔时间"),ShowIf(nameof(showTime)),FoldoutGroup(settingDes)]
        internal float intervalTime = 0.2f;
                 
        [SerializeField,LabelText("自动播放间隔时间"),FoldoutGroup(settingDes)]
        internal float timer = 1.5f;

        [LabelText("自动播放开始前触发事件:"), FoldoutGroup(settingDes)]
        public UnityEvent onAutomaticSessionStartEvent;

        [LabelText("自动播放结束后触发事件:"), FoldoutGroup(settingDes)]
        public UnityEvent onAutomaticSessionEndEvent;
       
        private DiaLog mDiaLog;
    
        private Coroutine mCurrentCoroutine;
        private Action mCurrentCallBack;       
     
        /// <summary>
        /// 从初始开始进行推进(必须是自动模式且已经调用InitDiaLog方法初始化)
        /// </summary>
        public async void Auto_Reset_MoveNext()
        {

            CoroutineTokenSource source = CoroutineTokenSource.Create(this);
            await CoroutineTool.WaitUntil(() => mDiaLog != null).Token(source.Token);
            if (mDiaLog.treeState == NodeTreeState.Waiting)
                mDiaLog.Start();
            onAutomaticSessionStartEvent?.Invoke();
            while (true)
            {
                if (mDiaLog == null)
                    break;               

                await CoroutineTool
                    .WaitUntil(() => 
                    {
                        Node node = mDiaLog.GetCurrentRuntimeNode();
                        return mDiaLog.treeState == NodeTreeState.Waiting || (node.IsCompleted && !node.IsComposite);
                    })
                    .Token(source.Token);

                if (mDiaLog.treeState == NodeTreeState.Waiting)
                    break;

                await CoroutineTool.WaitForSeconds(timer)
                    .Token(source.Token);

                MoveNodeState nodeState = mDiaLog.MoveNext();
                if (nodeState == MoveNodeState.Failed)
                    break;
            }
            onAutomaticSessionEndEvent?.Invoke();

        }

        private void Update_UI(Node node)
        {           
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
      

        public void InitDiaLog(DiaLog diaLog)
        {
            if (mDiaLog != null)
            {
                if (mDiaLog == diaLog)
                {
                    if (mDiaLog.treeState == NodeTreeState.Waiting)
                        mDiaLog.Start();
                    return;
                }               
            }
            mDiaLog = diaLog;           
            mDiaLog.RegisterWithNodeEnterEvent(Update_UI);       
            mDiaLog.Start();
        }      
    }
}
