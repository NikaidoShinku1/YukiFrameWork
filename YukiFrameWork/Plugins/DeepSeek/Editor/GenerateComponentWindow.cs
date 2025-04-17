///=====================================================
/// - FileName:      GenerateComponentWindow.cs
/// - NameSpace:     YukiFrameWork.ItemDemo
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/15 20:02:40
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Linq;
namespace YukiFrameWork.ItemDemo
{
    public class GenerateComponentWindow : EditorWindow
    {
        private const string ApiKeyErrorText = "未设置API秘钥 请检查项目设置 " +
                                               "(YukiFrameWork/LocalConfiguration/DeepSeek AI代码生成设置/API Key).";       
        private string prompt = "";
        private string className = "";
        private bool forgetHistory;
        private bool deletePrevious;

        // Property to check if the API key is valid (not null or empty).
        private bool IsApiKeyOk => !string.IsNullOrEmpty(ScriptGeneratorSettings.instance.apiKey) && !string.IsNullOrEmpty(DeepSeekAPI.Encryption.Decrypt(ScriptGeneratorSettings.instance.apiKey, CloudProjectSettings.userName));

        // Wraps the user’s prompt with standard instructions for the DeepSeek API to generate a Unity script.
        private static string WrapPrompt(string input) =>
            "Write a Unity script.\n" + " - It is a C# MonoBehaviour.\n " + // Ensures the script is a Unity MonoBehaviour.
            " - Include all the necessary imports.\n" + // Requests required using statements.
            " - Add a RequireComponent attribute to any components used.\n" + // Ensures dependencies are enforced.
            " - Generate tooltips for all properties.\n" + // Adds tooltips for better usability in Unity Inspector.
            " - All properties should have default values.\n" + // Ensures properties are initialized.
            " - I only need the script body. Don’t add any explanation.\n" + // Requests only the code, no comments.
            " - Do not start and end the result with ```csharp and ```.\n" + // Avoids markdown formatting.
            "The task is described as follows:\n" + input; // Appends the user’s specific prompt.

        // Initiates the script generation process by submitting the prompt to the DeepSeek API.
        void RunGenerator(bool forgetHistory = false)
        {
            // Builds the full prompt, optionally instructing the API to forget prior context.
            var prompt = forgetHistory ? "Forget everything I said before.\n" : string.Empty;
            prompt += WrapPrompt(this.prompt); // Combines standard instructions with user input.
            var code = DeepSeekAPI.Submit(prompt); // Sends the prompt to the API and gets the generated code.
            CreateScriptAsset(code); // Creates a script asset from the returned code.
        }

        // Creates a new script asset in the Unity project using the generated code.
        void CreateScriptAsset(string code)
        {
           
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
           
            var everythingBeforeColon = code.Split(':')[0];
            var words = everythingBeforeColon.Split(' ');
            words = words.Where(w => !string.IsNullOrEmpty(w)).ToArray();
         
            className = words[words.Length - 1];

            Debug.Log($"{DeepSeekAPI.DeepSeekCode}Created class {className}");

          
            var i = 1;
            while (Resources.FindObjectsOfTypeAll<MonoScript>().Any(s => s.name == className))
            {
                className = words[words.Length - 1] + i++; // e.g., "MyScript" becomes "MyScript1", "MyScript2", etc.
            }

          
            EditorPrefs.SetString("AiEngineerClassName", className);

            var settings = ScriptGeneratorSettings.instance;
            if (!AssetDatabase.IsValidFolder(settings.path))
            {
                var folder = settings.path.Substring(7);

                if (folder.EndsWith("/"))
                {
                    folder = folder.Substring(0, folder.Length - 1);
                }

                AssetDatabase.CreateFolder("Assets", folder);
            }

          
            var path = $"{settings.path}/{className}.cs";

           
            method!.Invoke(null, new object[] { path, code });
        }

      
        void OnGUI()
        {          
            if (IsApiKeyOk)
            {             
                if (string.IsNullOrEmpty(prompt))
                {
                    prompt = EditorPrefs.GetString("AiEngineerPrompt", "");
                }

             
                prompt = EditorGUILayout.TextArea(prompt, GUILayout.ExpandHeight(true));

                // Toggle to forget previous context, useful for starting fresh.
                forgetHistory = EditorGUILayout.Toggle("忘记之前的命令", forgetHistory);
             
                var className = EditorPrefs.GetString("AiEngineerClassName", "");
                deletePrevious =
                    !(string.IsNullOrEmpty(className) || !Selection.activeGameObject ||
                      !Selection.activeGameObject.GetComponent(className)) &&
                    EditorGUILayout.Toggle($"Overwrite {className}", deletePrevious);

                // Button to trigger script generation and optionally add it to the selected GameObject.
                if (GUILayout.Button("生成并添加组件"))
                {
                    // Saves the current prompt.
                    EditorPrefs.SetString("AiEngineerPrompt", prompt);

                    // Deletes the previous script and component if the overwrite option is selected.
                    if (deletePrevious)
                    {
                        var gameObject = Selection.activeGameObject;
                        if (gameObject != null)
                        {
                            var component = gameObject.GetComponent(className);
                            if (component != null)
                            {
                                // Removes the component from the GameObject.
                                DestroyImmediate(component);
                            }
                        }

                        var settings = ScriptGeneratorSettings.instance;
                        var file = AssetDatabase.LoadAssetAtPath<MonoScript>($"{settings.path}{className}.cs");
                        if (file != null)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(file));
                        }
                    }

                    // Generates the new script.
                    RunGenerator(forgetHistory);
                }
            }
            else
            {
                
                EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
            }
        }

     
        void OnEnable() => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;


        void OnDisable() => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

    
        void OnAfterAssemblyReload()
        {
            // Finds the generated script by its class name
            var script = Resources.FindObjectsOfTypeAll<MonoScript>().FirstOrDefault(s => s.name == className);
            if (script != null)
            {
                var gameObject = GenerateComponentButtonEditor.SelectGameObject;
                if (gameObject == null)
                {
                    gameObject = Selection.activeGameObject;
                }

                if (gameObject != null)
                {
                    gameObject.AddComponent(script.GetClass()); // Attaches the script as a component.
                }
                else
                {
                    Debug.LogError($"{DeepSeekAPI.DeepSeekCode}Target GameObject not found.");
                }
            }
            else
            {
                Debug.LogError($"{DeepSeekAPI.DeepSeekCode}Script not found.");
            }

            Close();
        }


    }
}
#endif