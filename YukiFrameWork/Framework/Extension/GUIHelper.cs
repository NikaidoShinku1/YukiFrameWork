using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    public interface IGUIDepend
    {
        IGUIDepend Space(float pixels = 10);
        #region Label
        IGUIDepend Label(string label, params GUILayoutOption[] options);
        IGUIDepend Label(GUIContent content, params GUILayoutOption[] options);
        IGUIDepend Label(Texture image, params GUILayoutOption[] options);
        IGUIDepend Label(string label,GUIStyle style, params GUILayoutOption[] options);
        IGUIDepend Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options);
        IGUIDepend Label(Texture image, GUIStyle style, params GUILayoutOption[] options);

        IGUIDepend Toggle(bool value,string label,Action<bool> callBack, params GUILayoutOption[] options);
        IGUIDepend Toggle(bool value,GUIContent content,Action<bool> callBack, params GUILayoutOption[] options);
        IGUIDepend Toggle(bool value,Texture image,Action<bool> callBack, params GUILayoutOption[] options);
        IGUIDepend Toggle(bool value,string label, GUIStyle style,Action<bool> callBack, params GUILayoutOption[] options);
        IGUIDepend Toggle(bool value,GUIContent content, GUIStyle style,Action<bool> callBack, params GUILayoutOption[] options);
        IGUIDepend Toggle(bool value,Texture image, GUIStyle style,Action<bool> callBack, params GUILayoutOption[] options);
        #endregion

        IGUIDepend PasswordField(string value, params GUILayoutOption[] options);
        IGUIDepend PasswordField(string value,GUIStyle style, params GUILayoutOption[] options);
        #region TextField
        IGUIDepend TextField(string text,Action<string> callBack, params GUILayoutOption[] options);
        IGUIDepend TextField(string text,int maxLength, Action<string> callBack, params GUILayoutOption[] options);
        IGUIDepend TextField(string text,int maxLength,GUIStyle style, Action<string> callBack, params GUILayoutOption[] options);
        IGUIDepend TextField(string text,GUIStyle style, Action<string> callBack, params GUILayoutOption[] options);

        IGUIDepend TextArea(string text, Action<string> callBack, params GUILayoutOption[] options);
        IGUIDepend TextArea(string text, int maxLength, Action<string> callBack, params GUILayoutOption[] options);
        IGUIDepend TextArea(string text, int maxLength, GUIStyle style, Action<string> callBack, params GUILayoutOption[] options);
        IGUIDepend TextArea(string text, GUIStyle style, Action<string> callBack, params GUILayoutOption[] options);
        #endregion
        IGUIDepend Button(string name, Action call, params GUILayoutOption[] options);

        IGUIDepend GUIEnabled();
        IGUIDepend GUIDisabled();

        IGUIDepend GUIColor(Color color);
        IGUIDepend GUIColorEnd();
#if UNITY_EDITOR
        IGUIDepend PropertyField(SerializedProperty property,params GUILayoutOption[] options);
        IGUIDepend PropertyField(SerializedProperty property,GUIContent content, params GUILayoutOption[] options);
        IGUIDepend PropertyField(SerializedProperty property, GUIContent content,bool includeChildren, params GUILayoutOption[] options);
        IGUIDepend PropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] options);
#endif
        IGUIDepend Horizontal(Action<IGUIDepend> action, params GUILayoutOption[] options);
        IGUIDepend Vertical(Action<IGUIDepend> action, params GUILayoutOption[] options);

        IGUIDepend BeginHorizontal(params GUILayoutOption[] options);
        IGUIDepend BeginVertical(params GUILayoutOption[] options);
        IGUIDepend BeginHorizontal(string text,GUIStyle style,params GUILayoutOption[] options);
        IGUIDepend BeginVertical(string text,GUIStyle style,params GUILayoutOption[] options);
        IGUIDepend BeginHorizontal(Texture image,GUIStyle style,params GUILayoutOption[] options);
        IGUIDepend BeginVertical(Texture image,GUIStyle style,params GUILayoutOption[] options);
        IGUIDepend BeginHorizontal(GUIContent content,GUIStyle style,params GUILayoutOption[] options);
        IGUIDepend BeginVertical(GUIContent content,GUIStyle style, params GUILayoutOption[] options);
        IGUIDepend BeginHorizontal(GUIStyle style, params GUILayoutOption[] options);
        IGUIDepend BeginVertical(GUIStyle style, params GUILayoutOption[] options);



        IGUIDepend EndHorizontal();
        IGUIDepend EndVertical();       
        #region Area
        IGUIDepend Area(Rect screenRect, Action<IGUIDepend> action);
        IGUIDepend Area(Rect screenRect,GUIContent content, Action<IGUIDepend> action);
        IGUIDepend Area(Rect screenRect, string text, Action<IGUIDepend> action);
        IGUIDepend Area(Rect screenRect, Texture image, Action<IGUIDepend> action);
        IGUIDepend Area(Rect screenRect, Texture image, GUIStyle style, Action<IGUIDepend> action);
        IGUIDepend Area(Rect screenRect, GUIContent content,GUIStyle style, Action<IGUIDepend> action);
        IGUIDepend Area(Rect screenRect, string text, GUIStyle style, Action<IGUIDepend> action);
        IGUIDepend Area(Rect screenRect, GUIStyle style, Action<IGUIDepend> action);
        #endregion

        #region Box
        IGUIDepend Box(GUIContent content,params GUILayoutOption[] options);
        IGUIDepend Box(GUIContent content, GUIStyle style, params GUILayoutOption[] options);
        IGUIDepend Box(string text,GUIStyle style, params GUILayoutOption[] options);
        IGUIDepend Box(string text, params GUILayoutOption[] options);
        IGUIDepend Box(Texture image, params GUILayoutOption[] options);
        IGUIDepend Box(Texture image, GUIStyle style, params GUILayoutOption[] options);
        #endregion          
    }
    
    public static class GUIHelper 
    {
        private static SimpleObjectPools<IGUIDepend> dependPools
            = new SimpleObjectPools<IGUIDepend>(() => new GUIDepend(), null, initSize: 200);

        public static IGUIDepend GetContent() => dependPools.Get();

        public class GUIDepend : IGUIDepend
        {
            #region Horizontak And Vertical
            public IGUIDepend Horizontal(Action<IGUIDepend> action, params GUILayoutOption[] options)
            {                             
                GUILayout.BeginHorizontal(options);
                action?.Invoke(this);
                GUILayout.EndHorizontal();
                return this;
            }

            public IGUIDepend Vertical(Action<IGUIDepend> action,params GUILayoutOption[] options)
            {                
                GUILayout.BeginVertical(options);
                action?.Invoke(this);
                GUILayout.EndHorizontal();
                return this;              
            }

            public IGUIDepend BeginHorizontal(params GUILayoutOption[] options)
            {
                GUILayout.BeginHorizontal(options);
                return this;
            }

            public IGUIDepend BeginVertical(params GUILayoutOption[] options)
            {
                GUILayout.BeginVertical(options);
                return this;
            }

            public IGUIDepend BeginHorizontal(string text, GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.BeginHorizontal(text,style,options);
                return this;
            }

            public IGUIDepend BeginVertical(string text, GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.BeginVertical(text,style,options);
                return this;
            }

            public IGUIDepend BeginHorizontal(Texture image, GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.BeginHorizontal(image, style, options);
                return this;
            }

            public IGUIDepend BeginVertical(Texture image, GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.BeginVertical(image, style, options);
                return this;
            }

            public IGUIDepend BeginHorizontal(GUIStyle style, params GUILayoutOption[] options)
            {
                return BeginHorizontal(GUIContent.none, style, options);
            }

            public IGUIDepend BeginVertical(GUIStyle style, params GUILayoutOption[] options)
            {
                return BeginVertical(GUIContent.none, style, options);
            }      

            public IGUIDepend BeginHorizontal(GUIContent content,GUIStyle style, params GUILayoutOption[] options)
            {
                return BeginHorizontal(content.text, style, options) ;
            }

            public IGUIDepend BeginVertical(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
            {
                return BeginVertical(content.text, style, options);
            }

            public IGUIDepend EndHorizontal()
            {
                GUILayout.EndHorizontal();
                return this;
            }

            public IGUIDepend EndVertical()
            {
                GUILayout.EndVertical();
                return this;
            }
            #endregion

            #region Label
            public IGUIDepend Label(string label,params GUILayoutOption[] options)
            {               
                return Label(label, GUI.skin.label, options);
            }

            public IGUIDepend Label(GUIContent content, params GUILayoutOption[] options)
            {               
                return Label(content, GUI.skin.label, options);
            }

            public IGUIDepend Label(Texture image, params GUILayoutOption[] options)
            {
                return Label(image, GUI.skin.label, options);
            }

            public IGUIDepend Label(string label, GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.Label(label, style, options);
                return this;
            }

            public IGUIDepend Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
            {
                return Label(content.text, style, options);
            }

            public IGUIDepend Label(Texture image, GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.Label(image, style, options);
                return this;
            }
            #endregion
#if UNITY_EDITOR
            public IGUIDepend PropertyField(SerializedProperty property, params GUILayoutOption[] options)
            {
                EditorGUILayout.PropertyField(property, options);
                return this;
            }

            public IGUIDepend PropertyField(SerializedProperty property, GUIContent content, params GUILayoutOption[] options)
            {
                EditorGUILayout.PropertyField(property,content, options);
                return this;
            }

            public IGUIDepend PropertyField(SerializedProperty property, GUIContent content, bool includeChildren, params GUILayoutOption[] options)
            {
                EditorGUILayout.PropertyField(property, content,includeChildren, options);
                return this;
            }

            public IGUIDepend PropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] options)
            {
                EditorGUILayout.PropertyField(property, includeChildren, options);
                return this;
            }
#endif
            public IGUIDepend Button(string name, Action call, params GUILayoutOption[] options)
            {
                if (GUILayout.Button(name, options))
                {
                    call?.Invoke();
                }
                return this;
            }

            public IGUIDepend Space(float pixels = 10)
            {               
                GUILayout.Space(pixels);
                return this;
            }
            #region Area
            public IGUIDepend Area(Rect screenRect, Action<IGUIDepend> action)
            {
                return Area(screenRect, GUIContent.none, action);
            }

            public IGUIDepend Area(Rect screenRect, GUIContent content, Action<IGUIDepend> action)
            {
                return Area(screenRect, content.text, action);
            }

            public IGUIDepend Area(Rect screenRect, string text, Action<IGUIDepend> action)
            {
                return Area(screenRect, text, GUIStyle.none, action);
            }

            public IGUIDepend Area(Rect screenRect, Texture image, Action<IGUIDepend> action)
            {
                return Area(screenRect, image, GUIStyle.none,action);
            }

            public IGUIDepend Area(Rect screenRect, Texture image, GUIStyle style, Action<IGUIDepend> action)
            {
                GUILayout.BeginArea(screenRect, image,style);
                action?.Invoke(this);
                GUILayout.EndArea();
                return this;
            }

            public IGUIDepend Area(Rect screenRect, GUIContent content, GUIStyle style, Action<IGUIDepend> action)
            {
                return Area(screenRect, content.text, style, action);
            }

            public IGUIDepend Area(Rect screenRect, string text, GUIStyle style, Action<IGUIDepend> action)
            {
                GUILayout.BeginArea(screenRect, text,style);
                action?.Invoke(this);
                GUILayout.EndArea();
                return this;
            }

            public IGUIDepend Area(Rect screenRect, GUIStyle style, Action<IGUIDepend> action)
            {
                return Area(screenRect, GUIContent.none, style, action);
            }
            #endregion Area

            #region Box
            public IGUIDepend Box(GUIContent content, params GUILayoutOption[] options)
            {
                return Box(content, GUI.skin.box,options);
            }

            public IGUIDepend Box(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
            {
                return Box(content.text, style, options);
            }

            public IGUIDepend Box(string text, GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.Box(text, style, options);
                return this;
            }

            public IGUIDepend Box(string text, params GUILayoutOption[] options)
            {
                return Box(text, GUI.skin.box, options);
            }

            public IGUIDepend Box(Texture image, params GUILayoutOption[] options)
            {
                return Box(image, GUI.skin.box, options);
            }

            public IGUIDepend Box(Texture image, GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.Box(image, style, options);
                return this;
            }
            #endregion

            public IGUIDepend TextField(string text, Action<string> callBack, params GUILayoutOption[] options)
            {
                return TextField(text, -1, GUI.skin.textField, callBack, options);
            }

            public IGUIDepend TextField(string text, int maxLength, Action<string> callBack, params GUILayoutOption[] options)
            {
                return TextField(text, maxLength,GUI.skin.textField, callBack, options);
            }

            public IGUIDepend TextField(string text, int maxLength, GUIStyle style, Action<string> callBack, params GUILayoutOption[] options)
            {
                string target = GUILayout.TextField(text,maxLength,style, options);
                callBack?.Invoke(target);
                return this;
            }

            public IGUIDepend TextField(string text, GUIStyle style, Action<string> callBack, params GUILayoutOption[] options)
            {
                return TextField(text, -1, style, callBack, options);
            }

            public IGUIDepend PasswordField(string value, params GUILayoutOption[] options)
            {
                return PasswordField(value, GUI.skin.textField, options);
            }

            public IGUIDepend PasswordField(string value,GUIStyle style, params GUILayoutOption[] options)
            {
                GUILayout.PasswordField(value, '*',style, options);
                return this;
            }

            public IGUIDepend GUIEnabled()
            {
                GUI.enabled = true;
                return this;
            }

            public IGUIDepend GUIDisabled()
            {
                GUI.enabled = false;
                return this;
            }

            public IGUIDepend GUIColor(Color color)
            {
                GUI.color = color;
                return this;
            }

            public IGUIDepend GUIColorEnd()
            {
                return GUIColor(Color.white);
            }
            #region TextArea
            public IGUIDepend TextArea(string text, Action<string> callBack, params GUILayoutOption[] options)
            {
                return TextArea(text, -1, GUI.skin.textArea, callBack, options);
            }

            public IGUIDepend TextArea(string text, int maxLength, Action<string> callBack, params GUILayoutOption[] options)
            {
                return TextArea(text, maxLength, GUI.skin.textArea, callBack, options);
            }

            public IGUIDepend TextArea(string text, int maxLength, GUIStyle style, Action<string> callBack, params GUILayoutOption[] options)
            {
                string target = GUILayout.TextArea(text, maxLength, style, options);
                callBack?.Invoke(target);
                return this;
            }

            public IGUIDepend TextArea(string text, GUIStyle style, Action<string> callBack, params GUILayoutOption[] options)
            {
                return TextArea(text, -1, style, callBack, options);
            }
            #endregion
            #region Toggle
            public IGUIDepend Toggle(bool value,string label,Action<bool> callBack, params GUILayoutOption[] options)
            {
                return Toggle(value,label, GUI.skin.toggle,callBack,options);
            }

            public IGUIDepend Toggle(bool value,GUIContent content,Action<bool> callBack, params GUILayoutOption[] options)
            {
                return Toggle(value, content.text, GUI.skin.toggle,callBack, options);
            }

            public IGUIDepend Toggle(bool value,Texture image,Action<bool> callBack, params GUILayoutOption[] options)
            {
                return Toggle(value, image, GUI.skin.toggle,callBack, options);
            }

            public IGUIDepend Toggle(bool value,string label, GUIStyle style,Action<bool> callBack, params GUILayoutOption[] options)
            {
                bool target = GUILayout.Toggle(value,label, style, options);
                callBack?.Invoke(target);
                return this;
            }

            public IGUIDepend Toggle(bool value,GUIContent content, GUIStyle style,Action<bool> callBack, params GUILayoutOption[] options)
            {
                return Toggle(value, content.text, style, callBack, options);
            }

            public IGUIDepend Toggle(bool value,Texture image, GUIStyle style,Action<bool> callBack, params GUILayoutOption[] options)
            {
                bool target = GUILayout.Toggle(value, image, style, options);
                callBack?.Invoke(target);
                return this;
            }
            #endregion
        }

        public static void SetDesignResolution(float width, float height)
        {
            var scaleX = Screen.width / width;
            var scaleY = Screen.height / height;

            var scale = Mathf.Max(scaleX, scaleY);

            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), new Vector2(0, 0));
        }
    }
}