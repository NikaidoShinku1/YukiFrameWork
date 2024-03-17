using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XFABManager;

namespace XFABManager
{
     
    internal class TimerManager
    {

        internal static void DelayInvoke(Action action, float delay)
        {
            CoroutineStarter.Start(DelayExecute(action, delay));
        }

        internal static IEnumerator DelayExecute(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

    }
}