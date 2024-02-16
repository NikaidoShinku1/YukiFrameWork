using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class BezierExecution : MonoBehaviour
    {
        private FastList<Bezier> releaseList = new FastList<Bezier>();
        private FastList<Bezier> runtimeCores = new FastList<Bezier>();
        public void AddCore(Bezier core)
        {
            runtimeCores.Add(core);
        }
      
        private void Update()
        {
            OnUpdate_Execute(BezierRuntimeMode.OnUpdate);

            if (releaseList.Count > 0)
            {
                for (int i = 0; i < releaseList.Count; i++)
                {
                    Bezier.Release(releaseList[i]);
                    runtimeCores.Remove(releaseList[i]);
                }
                releaseList.Clear();
            }

        }

        private void FixedUpdate()
        {
            OnUpdate_Execute(BezierRuntimeMode.OnFixedUpdate);
        }

        private void LateUpdate()
        {
            OnUpdate_Execute(BezierRuntimeMode.OnLateUpdate);
        }

        private void OnUpdate_Execute(BezierRuntimeMode mode)
        {
            foreach (var core in runtimeCores)
            {
                if (core.Mode == mode)
                {
                    if (core?.Condition() == true)
                        releaseList.Add(core);
                    else
                        core.UpdateInvoke();
                }

            }
        }
    }
}