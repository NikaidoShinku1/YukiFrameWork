using System;

namespace YukiFrameWork.Events
{
    public enum EventLifecycleBindType
    {
        None,
        GameObjectDestroy,
        GameObjectDisable,
        SceneUnload
    }

    public enum EventLifecycleSafetyStatus
    {
        ActiveBoundDestroy,
        ActiveBoundDisable,
        ActiveBoundScene,
        ActiveStatic,
        ActiveOrphanRisk,
        LeakSuspect,
        UnregisteredManual,
        UnregisteredAll,
        UnregisteredByLifecycle
    }
}
