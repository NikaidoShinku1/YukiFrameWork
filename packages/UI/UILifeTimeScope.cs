using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.UI
{
    public class UILifeTimeScope : LifeTimeScope
    {
        protected override void InitBuilder(IContainerBuilder builder)
        {
            builder.RegisterInstance(typeof(PanelManager));
        }
    }
}
