using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.UI
{
    [Serializable]
    public class UICustomData : GenericDataBase
    {
        [SerializeField]
        private string assetPath = @"Assets/";

        public string AssetPath
        {
            get => assetPath;
            set => assetPath = value;
        }

        private bool isPartialLoading = false;

        public bool IsPartialLoading
        {
            get => isPartialLoading;
            set => isPartialLoading = value;
        }
    }
}
