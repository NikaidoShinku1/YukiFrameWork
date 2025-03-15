///=====================================================
/// - FileName:      StyleTool.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 23:34:46
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
namespace YukiFrameWork.Machine
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
        RedOn,

        NormalHEX,
        BlueHEX,
        MintHEX,
        GreenHEX,
        YellowHEX,
        OrangeHEX,
        RedHEX,
        NormalOnHEX,
        BlueOnHEX,
        MintOnHEX,
        GreenOnHEX,
        YellowOnHEX,
        OrangeOnHEX,
        RedOnHEX,
    }

    public class StyleTool
    {
        private static Dictionary<Style, GUIStyle> styleDictionary = null;
        static StyleTool()
        {

            styleDictionary = new Dictionary<Style, GUIStyle>();

            styleDictionary.Add(Style.Normal, new GUIStyle(string.Format("flow node {0}", (int)Style.Normal)));
            styleDictionary.Add(Style.Blue, new GUIStyle(string.Format("flow node {0}", (int)Style.Blue)));
            styleDictionary.Add(Style.Mint, new GUIStyle(string.Format("flow node {0}", (int)Style.Mint)));
            styleDictionary.Add(Style.Green, new GUIStyle(string.Format("flow node {0}", (int)Style.Green)));
            styleDictionary.Add(Style.Yellow, new GUIStyle(string.Format("flow node {0}", (int)Style.Yellow)));
            styleDictionary.Add(Style.Orange, new GUIStyle(string.Format("flow node {0}", (int)Style.Orange)));
            styleDictionary.Add(Style.Red, new GUIStyle(string.Format("flow node {0}", (int)Style.Red)));
            styleDictionary.Add(Style.NormalOn, new GUIStyle(string.Format("flow node {0} on", (int)Style.Normal)));
            styleDictionary.Add(Style.BlueOn, new GUIStyle(string.Format("flow node {0} on", (int)Style.Blue)));
            styleDictionary.Add(Style.MintOn, new GUIStyle(string.Format("flow node {0} on", (int)Style.Mint)));
            styleDictionary.Add(Style.GreenOn, new GUIStyle(string.Format("flow node {0} on", (int)Style.Green)));
            styleDictionary.Add(Style.YellowOn, new GUIStyle(string.Format("flow node {0} on", (int)Style.Yellow)));
            styleDictionary.Add(Style.OrangeOn, new GUIStyle(string.Format("flow node {0} on", (int)Style.Orange)));
            styleDictionary.Add(Style.RedOn, new GUIStyle(string.Format("flow node {0} on", (int)Style.Red)));


            styleDictionary.Add(Style.NormalHEX, new GUIStyle(string.Format("flow node hex {0}", (int)Style.Normal)));
            styleDictionary.Add(Style.BlueHEX, new GUIStyle(string.Format("flow node hex {0}", (int)Style.Blue)));
            styleDictionary.Add(Style.MintHEX, new GUIStyle(string.Format("flow node hex {0}", (int)Style.Mint)));
            styleDictionary.Add(Style.GreenHEX, new GUIStyle(string.Format("flow node hex {0}", (int)Style.Green)));
            styleDictionary.Add(Style.YellowHEX, new GUIStyle(string.Format("flow node hex {0}", (int)Style.Yellow)));
            styleDictionary.Add(Style.OrangeHEX, new GUIStyle(string.Format("flow node hex {0}", (int)Style.Orange)));
            styleDictionary.Add(Style.RedHEX, new GUIStyle(string.Format("flow node hex {0}", (int)Style.Red)));
            styleDictionary.Add(Style.NormalOnHEX, new GUIStyle(string.Format("flow node hex {0} on", (int)Style.Normal)));
            styleDictionary.Add(Style.BlueOnHEX, new GUIStyle(string.Format("flow node hex {0} on", (int)Style.Blue)));
            styleDictionary.Add(Style.MintOnHEX, new GUIStyle(string.Format("flow node hex {0} on", (int)Style.Mint)));
            styleDictionary.Add(Style.GreenOnHEX, new GUIStyle(string.Format("flow node hex {0} on", (int)Style.Green)));
            styleDictionary.Add(Style.YellowOnHEX, new GUIStyle(string.Format("flow node hex {0} on", (int)Style.Yellow)));
            styleDictionary.Add(Style.OrangeOnHEX, new GUIStyle(string.Format("flow node hex {0} on", (int)Style.Orange)));
            styleDictionary.Add(Style.RedOnHEX, new GUIStyle(string.Format("flow node hex {0} on", (int)Style.Red)));

        }

        public static GUIStyle Get(Style style)
        {
            if (styleDictionary.ContainsKey(style))
                return styleDictionary[style];

            return null;
        }

    }



}
#endif