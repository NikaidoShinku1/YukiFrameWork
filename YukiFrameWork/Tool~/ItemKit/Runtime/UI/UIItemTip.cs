///=====================================================
/// - FileName:      UITip.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/26 20:34:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
namespace YukiFrameWork.Item
{
    [DisableViewWarning]
	public class UIItemTip : MonoBehaviour
	{		
		[SerializeField,LabelText("物品的Icon组件")]
		private Image icon;
		[SerializeField,LabelText("物品的名称组件")]
		private Text itemName;
		[SerializeField,LabelText("物品的介绍组件")]
		private Text description;

		private const int offect = 40;

        [SerializeField,LabelText("是否默认关闭")]
        private bool IsDefaultHide = true;

        public void Start()
        {
            if (!(transform as RectTransform))
            {
                Debug.LogWarning("UIItemTip应该位于UGUI，且有Canvas管理的对象下添加!");
            }
            if(IsDefaultHide)
            this.Hide();
        }
        public void Show(UISlot slot)
		{
			if (slot.Slot?.Item != null)
			{
				this.icon.sprite = slot.Slot.Item.GetIcon;
				this.itemName.text = slot.Slot.Item.GetName;
				this.description.text = slot.Slot.Item.GetDescription;
				this.Show();
				this.SetPosition2D(slot.transform.position);						               
                var panelSize = this.GetComponent<RectTransform>().sizeDelta;
                var screen = this.transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
                Vector2 tipPosition = this.transform.localPosition;

                var bottomDistance = (tipPosition.y - panelSize.y * 0.5f) - (-screen.y * 0.5f);
                var topDistance = screen.y * 0.5f - (tipPosition.y + panelSize.y * 0.5f);
                var leftDistance = (tipPosition.x - panelSize.x * 0.5f) - (-screen.x * 0.5f);
                var rightDistance = screen.x * 0.5f - (tipPosition.x + panelSize.x * 0.5f);

                var minDistance = Mathf.Min(topDistance, bottomDistance, leftDistance, rightDistance);
                this.SetPosition2D(tipPosition);
                if (Mathf.Abs(minDistance - topDistance) < 0.01f)
                {
                    this.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                    this.SetLocalPosition2D(tipPosition + Vector2.down * offect);
                }
                else if (Mathf.Abs(minDistance - bottomDistance) < 0.01f)
                {
                    this.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                    this.SetLocalPosition2D(tipPosition + Vector2.up * offect);
                }
                else if (Mathf.Abs(minDistance - leftDistance) < 0.01f)
                {
                    Debug.Log("Right");
                    this.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    this.SetLocalPosition2D(tipPosition + Vector2.right * offect);
                }
                else if (Mathf.Abs(minDistance - rightDistance) < 0.01f)
                {
                    this.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
                    this.SetLocalPosition2D(tipPosition + Vector2.left * offect);
                }

            }
		}
	}
}
