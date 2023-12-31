using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.States
{
    public enum Style
    {
        Normal = 0,
        Blue,
        Mint,
        Green,
        Yellow,
        Orange,
        Red,
        NormalOn,
        BlueOn,
        MintOn,
        GreenOn,
        YellowOn,
        OrangeOn,
        RedOn
    }
    public class StateStyles
    {
        private Dictionary<int, GUIStyle> styleDict;

        public StateStyles()
        {
            styleDict = new Dictionary<int, GUIStyle>();
            for (int i = 0; i <= 6; i++)
            {
                styleDict.Add(i, new GUIStyle(string.Format($"flow node {i}")));
                styleDict.Add(i + 7, new GUIStyle(string.Format($"flow node {i} on")));

            }
        }

        public GUIStyle GetStyle(Style style)
            => styleDict[(int)style];

        public void ApplyZoomFactor(float zoomFactor)
        {
            foreach (var item in styleDict.Values)
            {
                item.fontSize = (int)Mathf.Lerp(5, 30, zoomFactor);
                item.contentOffset = new Vector2(0,Mathf.Lerp(-30,-20,zoomFactor));
            }
        }
        
        
    }
}
