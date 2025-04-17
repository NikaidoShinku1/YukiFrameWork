
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YukiFrameWork;

namespace YukiFrameWork.UI
{



    /// <summary>
    /// 自动定位到Dropdown的当前值所在的位置
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class DropdownLocation : MonoBehaviour
    {

        private Dropdown dropdown;

        private ScrollRect scrollRect;

        private bool success = false;

        private void Awake()
        {

            scrollRect = GetComponent<ScrollRect>();
        }

        private void OnEnable()
        {
            success = false;
        }


        private void Location()
        {
            dropdown = GetComponentInParent<Dropdown>();
            if (dropdown == null)
                return;

            if (dropdown.options.Count <= 1)
                return;

            float normalize = 1 - (float)dropdown.value / (dropdown.options.Count - 1);

            scrollRect.verticalNormalizedPosition = normalize;

            if (Mathf.Abs(scrollRect.verticalNormalizedPosition - normalize) < 0.01f)
                success = true;
        }


        private void Update()
        {
            if (!success)
                Location();
        }

    }

}
