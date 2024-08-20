///=====================================================
/// - FileName:      GuideHandler.cs
/// - NameSpace:     YukiFrameWork.Pilot
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/15 1:39:08
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Pilot
{
    [RequireComponent(typeof(Image))]
	public class GuideHandler : MonoBehaviour,ICanvasRaycastFilter
	{     
        private Dictionary<string, GuideInfo> runtime_Info_Dict = new Dictionary<string, GuideInfo>();

        private bool isInited = false;       

        public void Init(GuideDataBase dataBase,Canvas root)
        {
            if (isInited) return;
            this.root = root;
            runtime_Info_Dict = dataBase.infos.ToDictionary(x => x.guideKey, x => x);
            foreach (var item in runtime_Info_Dict.Values)
            {
                switch (item.guideType)
                {
                    case GuideType.Circle:
                        item.guideBase = new CircleGuide();
                        item.guideBase.material = Resources.Load<Material>("Materials_不可变动/CircleMat");                       

                        break;
                    case GuideType.Rect:
                        item.guideBase = new RectGuide();
                        item.guideBase.material = Resources.Load<Material>("Materials_不可变动/RectMat"); 
                        break;
                    case GuideType.Custom:
                        Type type = AssemblyHelper.GetType(item.guideBaseType);
                        if (type == null)
                            throw new Exception("选择了自定义引导，但类型不正确:GuideKey:" + item.guideKey);
                        item.guideBase = Activator.CreateInstance(type) as GuideBase;
                        item.guideBase.material = item.material;
                        break;                 
                }

                item.guideBase.OnInit(transform as RectTransform);
            }           
            mask = this.GetOrAddComponent<Image>();
            isInited = true;
        }

        internal GuideInfo CurrentInfo;

        /// <summary>
        /// 判断当前引导是否已经完成，如果引导不存在或者还在执行则返回False
        /// </summary>
        public bool IsCurrentCompleted => CurrentInfo?.guideBase.IsCompleted == true;

        /// <summary>
        /// 当前引导的目标
        /// </summary>
        public RectTransform CurrentTarget => CurrentInfo?.guideBase.target;

        /// <summary>
        /// 当前引导镂空的中心坐标
        /// </summary>
        public Vector3 CurrentCenter => CurrentInfo == null ? default : CurrentInfo.guideBase.Center;

        /// <summary>
        /// 执行引导
        /// </summary>
        /// <param name="eventKey">引导标识</param>
        /// <param name="target">引导目标</param>
        /// <param name="IsInterruption">引导是否能在前置打断前置还没有执行完成的引导</param>
        /// <returns></returns>
        public bool OnExecute(string eventKey, RectTransform target, bool IsInterruption = false)
        {
            if (runtime_Info_Dict.TryGetValue(eventKey, out var info))
            {
                if (CurrentInfo?.guideBase.IsCompleted == false && !IsInterruption) return false;
              
                mask.material = info.guideBase.material;
                info.guideBase.target = target;
                info.guideBase.root = root;
                info.guideBase.Guide(info);
                CurrentInfo = info;
                return true;
            }

            return false;
        }      
    
      
        private Image mask;

        private RectTransform current => CurrentTarget;

        public Canvas root { get;private set; }
        
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if(!current)    
                return true;

            return !RectTransformUtility.RectangleContainsScreenPoint(current, sp, root.worldCamera);
        }
    }
}
