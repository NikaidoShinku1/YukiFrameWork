using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class BezierExecution : MonoBehaviour
    {
        private List<BezierCore> releaseList = new List<BezierCore>();
        private List<BezierCore> runtimeCores = new List<BezierCore>();
        public void AddCore(BezierCore core)
        {
            runtimeCores.Add(core);
        }

        public void RemoveCore(BezierCore core)
        {
            runtimeCores.Remove(core);
        }

        private void Update()
        {
            OnUpdate_Execute(BezierRuntimeMode.OnUpdate);

            if (releaseList.Count > 0)
            {
                for (int i = 0; i < releaseList.Count; i++)               
                    runtimeCores.Remove(releaseList[i]);               
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
                    if (core.OnUpdate())
                        releaseList.Add(core);
            }
        }
    }
}