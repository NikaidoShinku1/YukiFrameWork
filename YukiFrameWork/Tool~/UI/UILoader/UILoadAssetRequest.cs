///=====================================================
/// - FileName:      UILoadAssetRequest.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/23 20:22:09
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================

using UnityEngine;
using System.Collections;
using YukiFrameWork;
namespace YukiFrameWork.UI
{
    public class UILoadAssetRequest : CustomYieldInstruction
    {
        public BasePanel Panel { get; protected set; }
        public override bool keepWaiting => Panel == null;

        public bool isDone => !keepWaiting;

        internal UILoadAssetRequest LoadPanelAsync(IPanel panel)
        {           
            this.Panel = panel as BasePanel;
            return this;
        }          
    }

    public static class UILoadAssetRequestExtension
    {
        public static YieldTask<BasePanel> GetAwaiter(this UILoadAssetRequest request)
        {
            var awaiter = new YieldTask<BasePanel>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return request;
                awaiter.Complete(null, request.Panel);
            }
            return awaiter;
        }    
    }
    
}
