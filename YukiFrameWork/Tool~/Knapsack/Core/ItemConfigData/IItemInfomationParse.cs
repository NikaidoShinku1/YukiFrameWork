using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace YukiFrameWork.Knapsack
{
    public interface IItemInfomationParse 
    {
        /// <summary>
        /// 解析方法
        /// </summary>
        /// <param name="jsonData">获得这个序列化的Json信息(依赖Litjson)</param>
        ItemData ParseJsonToItem(JsonData jsonData);
    }
}
