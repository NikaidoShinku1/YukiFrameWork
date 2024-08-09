#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork.ActionStates
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TimeStateMachineView))]
    public class TimeStateMachineViewEditor : StateMachineViewEditor
    {
    }
}
#endif