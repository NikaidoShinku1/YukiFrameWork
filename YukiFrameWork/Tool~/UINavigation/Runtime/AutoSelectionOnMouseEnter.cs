
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace YukiFrameWork.UI
{
    /// <summary>
    /// 当鼠标进入时自动选中该组件,退出时取消选中
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    [DisallowMultipleComponent]
    public class AutoSelectionOnMouseEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        private Selectable selectable;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (selectable != null)
            {
                selectable.Select();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (selectable != null)
            {
                ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.deselectHandler);
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(null);
            }
        }





    }

}

