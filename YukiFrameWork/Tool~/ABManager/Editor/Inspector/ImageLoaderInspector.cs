using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace XFABManager
{
    [CustomEditor(typeof(ImageLoader))]
    public class ImageLoaderInspector : Editor
    {

        private SerializedObject imageloaderSerialized;
        private SerializedProperty imageModel;
        private SerializedProperty imageModelType;
        private SerializedProperty imageModelPath;
        private SerializedProperty imageModelProjectName;
        private SerializedProperty imageModelAssetName;

        private SerializedProperty load_type;

        private SerializedProperty targetComponentType;

        private SerializedProperty target_component_full_name;
        private SerializedProperty target_component_fields_name;

        private SerializedProperty loading_color;
        private SerializedProperty load_complete_color;
        private SerializedProperty auto_set_native_size;


        private SerializedProperty OnStartLoad;
        private SerializedProperty OnLoading;
        private SerializedProperty OnLoadCompleted;
        private SerializedProperty OnLoadSuccess;
        private SerializedProperty OnLoadFailure;
         

        private void OnEnable()
        {
            imageloaderSerialized = new SerializedObject(target);
            imageModel = imageloaderSerialized.FindProperty("imageModel");
            imageModelType = imageModel.FindPropertyRelative("type");
            imageModelPath = imageModel.FindPropertyRelative("path");
            imageModelProjectName = imageModel.FindPropertyRelative("projectName");
            imageModelAssetName = imageModel.FindPropertyRelative("assetName");

            load_type = imageModel.FindPropertyRelative("load_type");

            targetComponentType = imageloaderSerialized.FindProperty("targetComponentType");

            target_component_full_name = imageloaderSerialized.FindProperty("target_component_full_name");
            target_component_fields_name = imageloaderSerialized.FindProperty("target_component_fields_name");


            loading_color = imageloaderSerialized.FindProperty("loading_color");
            load_complete_color = imageloaderSerialized.FindProperty("load_complete_color");
            auto_set_native_size = imageloaderSerialized.FindProperty("auto_set_native_size");

            OnStartLoad = imageloaderSerialized.FindProperty("OnStartLoad");
            OnLoading = imageloaderSerialized.FindProperty("OnLoading");
            OnLoadCompleted = imageloaderSerialized.FindProperty("OnLoadCompleted");
            OnLoadSuccess = imageloaderSerialized.FindProperty("OnLoadSuccess");
            OnLoadFailure = imageloaderSerialized.FindProperty("OnLoadFailure"); 
  
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI(); 
            //EditorGUILayout.LabelField("--------------------------------------------");
             
            imageloaderSerialized.Update();
             
            EditorGUILayout.PropertyField(imageModelType);

            switch (imageModelType.enumValueIndex)
            {
                case (int)ImageLoaderType.Network:
                case (int)ImageLoaderType.Local:
                    EditorGUILayout.PropertyField(imageModelPath);
                    break;
                case (int)ImageLoaderType.AssetBundle:
                    EditorGUILayout.PropertyField(load_type);
                    EditorGUILayout.PropertyField(imageModelProjectName);
                    EditorGUILayout.PropertyField(imageModelAssetName);
                    break;
            }

            EditorGUILayout.PropertyField(targetComponentType);

            if (targetComponentType.enumValueIndex == (int)TargetComponentType.Other) 
            {
                EditorGUILayout.PropertyField(target_component_full_name);
                EditorGUILayout.PropertyField(target_component_fields_name);
            }

            EditorGUILayout.PropertyField(loading_color);
            EditorGUILayout.PropertyField(load_complete_color);
            EditorGUILayout.PropertyField(auto_set_native_size);

            EditorGUILayout.PropertyField(OnStartLoad);
            EditorGUILayout.PropertyField(OnLoading);
            EditorGUILayout.PropertyField(OnLoadCompleted);
            EditorGUILayout.PropertyField(OnLoadSuccess);
            EditorGUILayout.PropertyField(OnLoadFailure);

            imageloaderSerialized.ApplyModifiedProperties(); 
        }
    }
}


