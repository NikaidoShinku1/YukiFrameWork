using System;
using UnityEngine;
using YukiFrameWork;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Events
{
    public class RuntimeEventInitializationFactory
    {        
        public static void Initialization<T>(T viewController) where T : ViewController
        {
            var eventCenter = viewController.GetComponent<RuntimeEventCenter>();        
            if (eventCenter == null) return;

            foreach (var index in eventCenter.GetEventCenterIndex())
            {
                var center = eventCenter.GetEventCenter(index);

                if (center != null) 
                {
                    if (string.IsNullOrEmpty(center.name)) continue;
                    switch (eventCenter.registerType)
                    {
                        case RegisterType.String:                           
                            viewController.RegisterEvent<EventArgs>(center.name, args => center.mEvent?.Invoke(args));
                            break;
                        case RegisterType.Enum:
                            var enumType = AssemblyHelper.GetType(center.name);
                            center.mEnum = (Enum)Enum.Parse(enumType, center.mEnumInfos[center.enumIndex]);
                            if (center.mEnum != null)
                                viewController.RegisterEvent<EventArgs>(center.mEnum, args => center.mEvent?.Invoke(args));
                            break;                    
                    }
                }
            }
        }     
        
    }
}