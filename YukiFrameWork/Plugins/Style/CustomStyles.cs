using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork
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
    public class CustomStyles
    {
        private Dictionary<int, GUIStyle> styleDict;
        private Dictionary<int, GUIStyle> subStyleDict;
        public CustomStyles()
        {
            styleDict = new Dictionary<int, GUIStyle>();
            subStyleDict = new Dictionary<int, GUIStyle>();
            GUIStyle s = null;           
            GUIStyle on = null;         
            for (int i = 0; i <= 6; i++)
            {
                s = new GUIStyle(string.Format($"flow node {i}"));             
                on = new GUIStyle(string.Format($"flow node {i} on"));              
                styleDict.Add(i, s);
                styleDict.Add(i + 7, on);
            }

            for (int i = 0; i <= 6; i++)
            {
                s = new GUIStyle(string.Format($"flow node hex {i}"));            
                on = new GUIStyle(string.Format($"flow node hex {i} on"));                           
                subStyleDict.Add(i, s);
                subStyleDict.Add(i + 7, on);
            }
        }

        public GUIStyle GetStyle(Style style,bool subing = false)
            => subing ? subStyleDict[(int)style] : styleDict[(int)style];

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
