///=====================================================
/// - FileName:      EventListener.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/20 21:03:41
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using YukiFrameWork.Extension;
using System.Reflection;
using System.Text;




#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Events
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class EventListener
	{
        internal const string scriptAssemblies = "./Library/ScriptAssemblies/";
        private static AssemblyDefinition assemblyDefinition;
        private static FileStream definitionStream;       
        internal const string DefindDirectory = "./Temp/" + nameof(YukiFrameWork);
      
        public static void DoBackUpDirCreateOneTime(string targetDir)
        {
            var backeupDir = DefindDirectory;
            if (Directory.Exists(backeupDir))
            {
                Directory.Delete(backeupDir, true);
            }
            Directory.CreateDirectory(backeupDir);
            foreach (var file in Directory.GetFiles(targetDir))
            {
                var destFileName = Path.GetFileName(file);
                File.Copy(file, backeupDir + $"/{destFileName}");
            }
        }

        static StringBuilder sb = new StringBuilder();
        public static string Print()
        {
            sb.Clear();
            sb.AppendLine("YukiFrameWork Events:");
            sb.AppendLine("[AddListener]".Color(Color.cyan));
            foreach (var inject in InjectExecutor.Executors)
            {
                string info = inject.Value.GetMethodInfos(out int count);
                if (count == 0) continue;             
                sb.AppendLine($"{inject.Key.FullName}".Color(Color.yellow));
                sb.AppendLine(info);
            }
            sb.AppendLine();
            sb.AppendLine("[RemoveAllListeners]".Color(Color.cyan));
            foreach (var inject in InjectExecutor.Executors)
            {
                string info = inject.Value.GetRemoveMethodInfos(out int count);
                if (count == 0) continue;
                sb.AppendLine($"{inject.Value.definition.FullName}".Color(Color.yellow));
                sb.AppendLine(info);
            }
            return sb.ToString();
        }
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void OnBeforeAssemblyReload() 
        {
            Inject(scriptAssemblies);
        }

        static void Inject(string dir)
        {

            Dictionary<string, IInject_IO> definds = new Dictionary<string, IInject_IO>();
            try
            {
                DoBackUpDirCreateOneTime(dir);
                foreach (var dllFileName in assemblies)
                {
                    if (definds.ContainsKey(dllFileName)) continue;

                    var dllPath = $"{dir}/{dllFileName}";
                    dllPath = Path.ChangeExtension(dllPath, ".dll");
                    if (File.Exists(dllPath) == false) continue;
                    var injecter = new Inject_IO(dllPath);
                    definds.Add(dllFileName, injecter);
                }
                foreach (var item in definds.Values)
                {
                    item.Init();
                }

                bool IsAutoInjected = true;
                foreach (var inject in definds.Values)
                {
                    if (!inject.Injected)
                    {
                        IsAutoInjected = false;
                        break;
                    }
                }
                bool jump = false;
                if (IsAutoInjected)
                {
                    jump = true;
                    goto Finish;
                }
                string path = scriptAssemblies + nameof(YukiFrameWork) + ".dll";

                definitionStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                assemblyDefinition = AssemblyDefinition.ReadAssembly(definitionStream, new ReaderParameters()
                {
                    ReadSymbols = true,
                    AssemblyResolver = CreateAssemblyResolver(DefindDirectory + "/" + nameof(YukiFrameWork) + ".dll")
                });
                foreach (var item in definds.Values)
                {
                    item.InjectEvent();
                }             
                InjectExecutor.ForEach(executor =>
                {
                    executor.Init();
                    if (executor.definition.Module != executor.assemblyDefinition.MainModule)
                        return;
                    InjectMonoMethod(executor);
                    InjectStaticMethod(executor);
                    InjectDefaultMethod(executor);
                    InjectRemoveMethod(executor);
                });

                foreach (var item in definds.Values)
                {
                    item.Write();
                }
                
            Finish:
                if (!jump)
                {
                    LogKit.I("YukiFrameWork Events Inject Completed!".Color(UnityEngine.Color.green));
                    LogKit.I(Print());
                }
            }
            catch (Exception ex)
            {
#if YukiFrameWork_DEBUGFULL
                LogKit.Exception(new Exception("丢失数据 Error:" + ex.ToString()));
#endif
            }
            finally
            {
                foreach (var item in definds.Values)
                {
                    item.Dispose();
                }
                if (assemblyDefinition != null && assemblyDefinition.MainModule.SymbolReader != null)
                {
                    assemblyDefinition.MainModule.SymbolReader.Dispose();
                    assemblyDefinition.Dispose();
                }
                if (definitionStream != null)
                {
                    definitionStream.Dispose();
                    definitionStream.Close();
                }

            }
        }
#if UNITY_EDITOR
        [UnityEditor.Callbacks.PostProcessScene]
#endif
        private static void AutoInjectAssemblys()
        {
            var targetDir = "./Library/PlayerScriptAssemblies";
            if (!Directory.Exists(targetDir))
            {
                targetDir = "./Library/Bee/PlayerScriptAssemblies";
            }
            if (!Directory.Exists(targetDir))
            {
                targetDir = "./Library/ScriptAssemblies";
            }           
            Inject(targetDir);
        }
        public static DefaultAssemblyResolver CreateAssemblyResolver(string dllPath)
        {
            var fullPath = Path.GetFullPath(dllPath);
            var fullPathDir = Path.GetDirectoryName(fullPath);

            HashSet<string> searchDir = new HashSet<string>();
            foreach (var path in (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                  select Path.GetDirectoryName(asm.ManifestModule.FullyQualifiedName)).Distinct())
            {
                try
                {
                    string targetPath = path;

                    if (targetPath == fullPathDir)
                    {
                        targetPath = DefindDirectory;
                    }
                    if (searchDir.Contains(targetPath) == false)
                    {
                        // UnityEngine.Debug.Log(targetPath);
                        searchDir.Add(targetPath);
                    }
                }
                catch { }
            }

            DefaultAssemblyResolver resole = new DefaultAssemblyResolver();
            foreach (var referenceDir in searchDir)
            {
                resole.AddSearchDirectory(referenceDir);
            }

            return resole;
        }

        private static void InjectDefaultMethod(InjectExecutor executor)
        {
            var usageType = executor.definition;
            foreach (var m in usageType.Methods)
            {
                if (m.IsConstructor == false) continue;
                if (m.IsStatic) continue;
                if (m.Body == null) continue;

                InjectConstructor(m, executor);
            }
        }

        private static void InjectStaticMethod(InjectExecutor executor)
        {

            foreach (var item in executor.static_event_Dicts)
            {
                var key = item.Key;
                var value = item.Value;
                foreach (var m in value)
                {
                    var wapper = NewGenerate_StaticMethod_Register_Build(m, false, executor.assemblyDefinition);
                    NewAppendStaticEventToRegisterBuilder(executor,wapper);
                }
            }

            foreach (var item in executor.task_static_event_Dicts)
            {
                var key = item.Key;
                var value = item.Value;
                foreach (var m in value)
                {
                    var wapper = NewGenerate_StaticMethod_Register_Build(m, true, executor.assemblyDefinition);
                    NewAppendStaticTaskToRegisterBuilder(executor, wapper);
                }
            }

        }

        private static MethodDefinition NewGenerate_StaticMethod_Register_Build(MethodDefinition method, bool isTask,AssemblyDefinition assemblyDefinition)
        {
            TypeReference eventType = method.Parameters[0].ParameterType;

            var methodName = $"{method.Name}__Build";
            var methodAttri = Mono.Cecil.MethodAttributes.Public;
            methodAttri |= Mono.Cecil.MethodAttributes.HideBySig;
            methodAttri |= Mono.Cecil.MethodAttributes.Static;

            TypeReference methodRet = assemblyDefinition.MainModule.ImportReference(typeof(void));

            var methodWrapper = new MethodDefinition(methodName, methodAttri, methodRet);

            var ilProcesser = methodWrapper.Body.GetILProcessor();

            if (isTask)
            {
                ilProcesser.Append(ilProcesser.Create(OpCodes.Ldnull));
                ilProcesser.Append(ilProcesser.Create(OpCodes.Ldftn, method));

                var action_CTOR = GetTaskFuncConstructor(eventType, assemblyDefinition);
                ilProcesser.Append(ilProcesser.Create(OpCodes.Newobj, action_CTOR));

                var registerMethod = GetAddListener_TaskMethod(eventType, assemblyDefinition);
                ilProcesser.Append(ilProcesser.Create(OpCodes.Call, registerMethod));
               
            }
            else
            {
                ilProcesser.Append(ilProcesser.Create(OpCodes.Ldnull));
                ilProcesser.Append(ilProcesser.Create(OpCodes.Ldftn, method));

                var action_CTOR = GetEventActionConstructor(eventType, assemblyDefinition);
                ilProcesser.Append(ilProcesser.Create(OpCodes.Newobj, action_CTOR));

                var registerMethod = GetAddListenerMethod(eventType, assemblyDefinition);
                ilProcesser.Append(ilProcesser.Create(OpCodes.Call, registerMethod));
                
            }
            ilProcesser.Append(ilProcesser.Create(OpCodes.Pop));
            ilProcesser.Append(ilProcesser.Create(OpCodes.Ret));

            method.DeclaringType.Methods.Add(methodWrapper);

            return methodWrapper;
        }
       
        private static void NewAppendStaticEventToRegisterBuilder(InjectExecutor executor,MethodReference staticWrapper)
        {
            var ilProcesser = executor.newStaticRegisterMethod.Body.GetILProcessor();
            var count = executor.newStaticRegisterMethod.Body.Instructions.Count;
            var lastLine = executor.newStaticRegisterMethod.Body.Instructions[count - 1];

            ilProcesser.InsertBefore(lastLine, ilProcesser.Create(OpCodes.Call, staticWrapper));
        }

        private static void NewAppendStaticTaskToRegisterBuilder(InjectExecutor executor,MethodReference staticWrapper)
        {
            var ilProcesser = executor.newStaticRegisterMethod.Body.GetILProcessor();
            var count = executor.newStaticRegisterMethod.Body.Instructions.Count;
            var lastLine = executor.newStaticRegisterMethod.Body.Instructions[count - 1];

            ilProcesser.InsertBefore(lastLine, ilProcesser.Create(OpCodes.Call, staticWrapper));
        }


        private static void InjectMonoMethod(InjectExecutor executor)
        {
            if (!executor.isMono) return;        
            FieldDefinition fieldDefinition = InjectSceneChecker(executor);

            foreach (var item in executor.default_event_Dicts)
            {
                var key = item.Key;
                var value = item.Value;
                foreach (var m in value)
                {                  
                    InjectMonoMethod(m,false,executor.Customize_op_Implicit_,fieldDefinition,false,executor.assemblyDefinition);
                }
            }

            foreach (var item in executor.mono_event_Dicts)
            {
                var key = item.Key;
                var value = item.Value;
                foreach (var m in value)
                {
                    InjectMonoMethod(m, true, executor.Customize_op_Implicit_, fieldDefinition, false, executor.assemblyDefinition);
                }
            }

            foreach (var item in executor.task_mono_event_Dicts)
            {
                var key = item.Key;
                var value = item.Value;
                foreach (var m in value)
                {
                    InjectMonoMethod(m, true, executor.Customize_op_Implicit_, fieldDefinition, true, executor.assemblyDefinition);
                }
            }

            foreach (var item in executor.task_default_event_Dicts)
            {
                var key = item.Key;
                var value = item.Value;
                foreach (var m in value)
                {
                    InjectMonoMethod(m, false, executor.Customize_op_Implicit_, fieldDefinition, true, executor.assemblyDefinition);
                }
            }        
        }

        private static void InjectMonoMethod(MethodDefinition method, bool needEnable, MethodDefinition Customize_op_Implicit_, FieldDefinition sceneCheckerField, bool isTask,AssemblyDefinition assemblyDefinition,bool isYield = false)
        {
            var eventType = method.Parameters[0].ParameterType;

            var ilProcesser = method.Body.GetILProcessor();
            var firstLine = method.Body.Instructions[0];

            var gameObjectCheckFirstLine = ilProcesser.Create(OpCodes.Ldarg_0);
            var sceneConditionFirstLine = ilProcesser.Create(OpCodes.Ldarg_0);

            var enableBlockFirstLine = ilProcesser.Create(OpCodes.Ldarg_0);

            // if(!mono) unregister return;
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
            var monoIsExist = typeof(UnityEngine.Object).GetMethod("op_Implicit");
            var monoIsExist_Ref = assemblyDefinition.MainModule.ImportReference(monoIsExist);
            if (Customize_op_Implicit_ != null)
            {
                monoIsExist_Ref = assemblyDefinition.MainModule.ImportReference(Customize_op_Implicit_);
            }
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, monoIsExist_Ref));
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Brtrue_S, gameObjectCheckFirstLine));

            {
                // unregister
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldftn, method));
                var action_CTOR = GetEventActionConstructor(eventType,assemblyDefinition);
                if (isTask)
                {
                    action_CTOR = GetTaskFuncConstructor(eventType, assemblyDefinition);
                }
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Newobj, action_CTOR));
                var unregisterMethod = GetRemoveListenerMethod(eventType, assemblyDefinition);
                if (isTask)
                {
                    unregisterMethod = GetRemoveListener_TaskMethod(eventType, assemblyDefinition);
                }
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, unregisterMethod));
                if (isTask)
                {
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldnull));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ret));
                }
                else
                {
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ret));
                }
            }
           
            ilProcesser.InsertBefore(firstLine, gameObjectCheckFirstLine);
            var get_GameObject = typeof(UnityEngine.Component).GetProperty("gameObject").GetMethod;
            var get_GameObject_Ref = assemblyDefinition.MainModule.ImportReference(get_GameObject);
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, get_GameObject_Ref));
            var goIsExist = typeof(UnityEngine.Object).GetMethod("op_Implicit");
            var goIsExist_Ref = assemblyDefinition.MainModule.ImportReference(goIsExist);
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, goIsExist_Ref));
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Brtrue_S, sceneConditionFirstLine));
            if (isTask)
            {
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldnull));
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ret));
            }
            else
            {
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ret));
            }

            // __SceneChecker__ == false    
            ilProcesser.InsertBefore(firstLine, sceneConditionFirstLine);
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldfld, sceneCheckerField));
            if (needEnable)
            {
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Brtrue_S, enableBlockFirstLine));
            }
            else
            {
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Brtrue_S, firstLine));
            }

            // SceneChecker = true
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldc_I4_1));
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Stfld, sceneCheckerField));           
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
            var isSceneObj = typeof(GameObjectExtension).GetMethod(nameof(GameObjectExtension.IsSceneObj));
            var isSceneObj_Ref = assemblyDefinition.MainModule.ImportReference(isSceneObj);
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, isSceneObj_Ref));

            if (needEnable)
            {
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Brfalse_S, enableBlockFirstLine));
            }
            else
            {
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Brfalse_S, firstLine));
            }
            {
                // SceneChecker = false 资源的情况，让他继续是false；
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldc_I4_0));
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Stfld, sceneCheckerField));

                // unregister
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldftn, method));
                var action_CTOR = GetEventActionConstructor(eventType, assemblyDefinition);
                if (isTask)
                {
                    action_CTOR = GetTaskFuncConstructor(eventType,assemblyDefinition);
                }
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Newobj, action_CTOR));
                var unregisterMethod = GetRemoveListenerMethod(eventType, assemblyDefinition);
                if (isTask)
                {
                    unregisterMethod = GetRemoveListener_TaskMethod(eventType, assemblyDefinition);
                }
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, unregisterMethod));
                if (isTask)
                {
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldnull));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ret));
                }
                else
                {
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ret));
                }
            } 

            if (!needEnable) return; 

            ilProcesser.InsertBefore(firstLine, enableBlockFirstLine);
            var isEnableMethod = typeof(UnityEngine.Behaviour).GetProperty("isActiveAndEnabled").GetMethod;
            var isEnableMethodRef = assemblyDefinition.MainModule.ImportReference(isEnableMethod);
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, isEnableMethodRef));
            ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Brtrue_S, firstLine));
            if (isTask)
            {
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldnull));
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ret));
            }
            else
            {
                ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ret));
            }


        }
              
        private static List<string> assemblies = new List<string>();
        static EventListener()
        {
            try
            {
               
                var config = UnityEngine.Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
               
                assemblies = config.assemblies.ToList();
                assemblies.Add(config.assembly);
#if UNITY_EDITOR
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
#endif

            }
            catch(Exception ex)
            {
                //如果丢失配置就默认程序集
                assemblies = new List<string>() { "Assembly-CSharp" }; 
#if YukiFrameWork_DEBUGFULL
                LogKit.W("框架配置初始化异常，请尝试重新编译 ps:如是刚新建项目可忽略该警告 Error:" + ex.ToString());
#endif
            }
        }
        static FieldDefinition InjectSceneChecker(InjectExecutor executor)
        {
            var type = executor.definition;
            var fieldAttri = Mono.Cecil.FieldAttributes.Private;
            var fieldType = assemblyDefinition.MainModule.ImportReference(typeof(bool));
            var fieldRef = new FieldDefinition($"{type.Name}Loaded", fieldAttri, fieldType);
            type.Fields.Add(fieldRef);
            return fieldRef;
        }
        public static bool Exist(ref string path,string euffix = ".dll")
        {
            path = Path.ChangeExtension(path, euffix);           
            return File.Exists(path);          
        }

        public static MethodDefinition GetMethodDefinition(Func<MethodDefinition,bool> condition)
        {
            return assemblyDefinition.MainModule.GetType(typeof(EventManager).FullName).Methods.FirstOrDefault(condition);
        }

        public static bool CheckDefind_Assemblies()
        {
            try
            {
                if (!Directory.Exists(DefindDirectory))
                {
                    Directory.CreateDirectory(DefindDirectory);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void InjectRemoveMethod(InjectExecutor executor)
        {
            foreach (var item in executor.remove_event_Dicts)
            {
                MethodDefinition method = item.Key;
                List<TypeDefinition> definitions = item.Value;
                var ilProcesser = method.Body.GetILProcessor();
                var firstLine = method.Body.Instructions[0];              

                foreach (var type in definitions)
                {                   
                    //ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldftn, method));
                    var unRegisterMethod = GetRemoveAllListenersMethod(type,executor.assemblyDefinition);
                    var unRegisterTaskMethod = GetRemoveAllListeners_TaskMethod(type, executor.assemblyDefinition);
                    //ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Nop));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, unRegisterMethod));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Nop));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, unRegisterTaskMethod));
                   
                }               
            }
        }

        private static void InjectConstructor(MethodDefinition constructor, InjectExecutor executor)
        {
            var ilProcesser = constructor.Body.GetILProcessor();
            var firstLine = constructor.Body.Instructions[0];
            foreach (var kv in executor.default_event_Dicts)
            {
                foreach (var usage in kv.Value)
                {
                    var eventType = usage.Parameters[0].ParameterType;
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldftn, usage));                 
                    var action_CTOR = GetEventActionConstructor(eventType,executor.assemblyDefinition);
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Newobj, action_CTOR));

                    var registerMethod = GetAddListenerMethod(eventType, executor.assemblyDefinition);                 
                   
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, registerMethod));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Pop));

                }
            } 

            foreach (var kv in executor.mono_event_Dicts)
            {
                foreach (var usage in kv.Value)
                {
                    var eventType = usage.Parameters[0].ParameterType;
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldftn, usage));

                    var action_CTOR = GetEventActionConstructor(eventType, executor.assemblyDefinition);
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Newobj, action_CTOR));
                  
                    var registerMethod = GetAddListenerMethod(eventType, executor.assemblyDefinition);
                   
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, registerMethod));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Pop));
                }
            }

            foreach (var kv in executor.task_default_event_Dicts)
            {
                foreach (var usage in kv.Value)
                {
                    var eventType = usage.Parameters[0].ParameterType;
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldftn, usage));

                    var func_CTOR = GetTaskFuncConstructor(eventType, executor.assemblyDefinition);
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Newobj, func_CTOR));

                    var registerMethod = GetAddListener_TaskMethod(eventType, executor.assemblyDefinition);
                   
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, registerMethod));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Pop));
                }
            }

            foreach (var kv in executor.task_mono_event_Dicts)
            {
                foreach (var usage in kv.Value)
                {
                    var eventType = usage.Parameters[0].ParameterType;
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldarg_0));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Ldftn, usage));

                    var func_CTOR = GetTaskFuncConstructor(eventType, executor.assemblyDefinition);
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Newobj, func_CTOR));

                    var registerMethod = GetAddListener_TaskMethod(eventType, executor.assemblyDefinition);
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Call, registerMethod));
                    ilProcesser.InsertBefore(firstLine, ilProcesser.Create(OpCodes.Pop));
                }
            }
        }

        private static MethodReference GetAddListenerMethod(TypeReference eventType,AssemblyDefinition assemblyDefinition)
        {
            var registerMethod = typeof(EventManager).GetMethods().FirstOrDefault(x => x.Name == nameof(EventManager.AddListener) && x.GetParameters().Length == 1);
            var methodReference = assemblyDefinition.MainModule.ImportReference(registerMethod);

            var genMethod = new GenericInstanceMethod(methodReference);
            genMethod.GenericArguments.Add(eventType);
            return genMethod;
        }

        private static MethodReference GetAddListener_TaskMethod(TypeReference eventType, AssemblyDefinition assemblyDefinition)
        {
            var registerMethod = typeof(EventManager).GetMethods().FirstOrDefault(x => x.Name == nameof(EventManager.AddListener_Task) && x.GetParameters().Length == 1);
            var methodReference = assemblyDefinition.MainModule.ImportReference(registerMethod);

            var genMethod = new GenericInstanceMethod(methodReference);
            genMethod.GenericArguments.Add(eventType);
            return genMethod;
        }
    

        private static MethodReference GetRemoveListenerMethod(TypeReference eventType, AssemblyDefinition assemblyDefinition)
        {
            var unregisterMethod = typeof(EventManager).GetMethods().FirstOrDefault(x => x.Name == nameof(EventManager.RemoveListener) && x.GetParameters().Length == 1);
            var methodReference = assemblyDefinition.MainModule.ImportReference(unregisterMethod);

            var genMethod = new GenericInstanceMethod(methodReference);
            genMethod.GenericArguments.Add(eventType);
            return genMethod;
        }

        private static MethodReference GetRemoveAllListenersMethod(TypeReference eventType,AssemblyDefinition assemblyDefinition)
        {
            var unregisterMethod = typeof(EventManager).GetMethods().FirstOrDefault(x => x.Name == nameof(EventManager.RemoveAllListeners) && x.GetParameters().Length == 0);
            var methodReference = assemblyDefinition.MainModule.ImportReference(unregisterMethod);
            var genMethod = new GenericInstanceMethod(methodReference);
            genMethod.GenericArguments.Add(eventType);
            return genMethod;
        }

        private static MethodReference GetRemoveAllListeners_TaskMethod(TypeDefinition eventType,AssemblyDefinition assemblyDefinition)
        {
            var unregisterMethod = typeof(EventManager).GetMethods().FirstOrDefault(x => x.Name == nameof(EventManager.RemoveAllListeners_Task) && x.GetParameters().Length == 0);
            var methodReference = assemblyDefinition.MainModule.ImportReference(unregisterMethod);
            var genMethod = new GenericInstanceMethod(methodReference);
            genMethod.GenericArguments.Add(eventType);
            return genMethod;
        }

        private static MethodReference GetRemoveListener_TaskMethod(TypeReference eventType, AssemblyDefinition assemblyDefinition)
        {
            var unregisterMethod = typeof(EventManager).GetMethods().FirstOrDefault(x => x.Name == nameof(EventManager.RemoveListener_Task) && x.GetParameters().Length == 1);
            var methodReference = assemblyDefinition.MainModule.ImportReference(unregisterMethod);

            var genMethod = new GenericInstanceMethod(methodReference);
            genMethod.GenericArguments.Add(eventType);
            return genMethod;
        }      
        private static MethodReference GetEventActionConstructor(TypeReference eventType, AssemblyDefinition assemblyDefinition)
        {
            var actionType = assemblyDefinition.MainModule.ImportReference(typeof(Action<>));
            var fieldType = new GenericInstanceType(actionType);
            fieldType.GenericArguments.Add(eventType);
            // import  Action<GameEvent>.ctor;
            var original_CTOR = fieldType.Resolve().Methods.First(m => { return m.Name == ".ctor"; });
            var generic_CTOR = new MethodReference(original_CTOR.Name, original_CTOR.ReturnType, fieldType)
            {
                HasThis = original_CTOR.HasThis,
                ExplicitThis = original_CTOR.ExplicitThis,
                CallingConvention = original_CTOR.CallingConvention,
            };
            foreach (var p in original_CTOR.Parameters)
            {
                generic_CTOR.Parameters.Add(new ParameterDefinition(p.ParameterType));
            }
            foreach (var gp in original_CTOR.GenericParameters)
            {
                generic_CTOR.GenericParameters.Add(new GenericParameter(gp.Name, generic_CTOR));
            }
            var action_CTOR = eventType.Module.ImportReference(generic_CTOR);

            return action_CTOR;
        }

        private static MethodReference GetTaskFuncConstructor(TypeReference eventType, AssemblyDefinition assemblyDefinition)
        {
            var actionType = assemblyDefinition.MainModule.ImportReference(typeof(Func<,>));
            var fieldType = new GenericInstanceType(actionType);
            fieldType.GenericArguments.Add(eventType);
            fieldType.GenericArguments.Add(assemblyDefinition.MainModule.ImportReference(typeof(Task)));
            // import  Func<GameEvent,Task>.ctor;
            var original_CTOR = fieldType.Resolve().Methods.First(m => { return m.Name == ".ctor"; });
            var generic_CTOR = new MethodReference(original_CTOR.Name, original_CTOR.ReturnType, fieldType)
            {
                HasThis = original_CTOR.HasThis,
                ExplicitThis = original_CTOR.ExplicitThis,
                CallingConvention = original_CTOR.CallingConvention,
            };
            foreach (var p in original_CTOR.Parameters)
            {
                generic_CTOR.Parameters.Add(new ParameterDefinition(p.ParameterType));
            }
            foreach (var gp in original_CTOR.GenericParameters)
            {
                generic_CTOR.GenericParameters.Add(new GenericParameter(gp.Name, generic_CTOR));
            }
            var func_CTOR = eventType.Module.ImportReference(generic_CTOR);

            return func_CTOR;
        }
    }
}
