///=====================================================
/// - FileName:      UIBuffHandlerGroup.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/10 22:12:02
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
namespace YukiFrameWork.Buffer
{
	public class UIBuffHandlerGroup : MonoBehaviour
	{

        [LabelText("UIBuffer需要挂载的根节点"),SerializeField]
        internal RectTransform UIBufferRoot;

        [LabelText("UIBuffer预制体"),SerializeField]
        [InfoBox("一般来说，一个对象只需要一种UIBuffer进行Buff的同步")]
        internal UIBuffer BufferPrefab;
        
        private Stack<UIBuffer> buffers = new Stack<UIBuffer>();

        private void Awake()
        {
            if (UIBufferRoot.GetComponentInParent<Canvas>() == null)
            {
                Debug.LogWarning("根节点应该处于Canvas下方！");
            }
            BufferPrefab.Hide();
        }      

        public UIBuffer CreateBuffer()
        {
            return buffers.Count > 0 ? buffers.Pop().Show() : BufferPrefab.Instantiate(UIBufferRoot).Show();
        }

        public void ReleaseBuffer(UIBuffer buffer)
        {
            buffer.Hide();
            buffer.OnDispose();
            buffers.Push(buffer);
        }
    }
}
