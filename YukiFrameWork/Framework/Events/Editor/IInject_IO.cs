///=====================================================
/// - FileName:      IInject_IO.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/22 11:16:07
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.IO;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using YukiFrameWork.Extension;
using System.Reflection;
namespace YukiFrameWork.Events
{
	public interface IInject_IO : IDisposable
	{
        void Init();
        void InjectEvent();
        bool Injected { get; }
        void Write();
	}

    public class Inject_IO : IInject_IO 
    {       
        private AssemblyDefinition assemblyDefinition;
        private FileStream dllStream;
        private string dllPath;
        private string dllNameNoExten;
        private string pdbPath;
        public Inject_IO(string dllPath)
        {
            this.dllPath = dllPath;
            this.dllNameNoExten = Path.GetFileNameWithoutExtension(this.dllPath);
            this.pdbPath = Path.ChangeExtension(this.dllPath, ".pdb");
        }
        private string backeupDir;
        private string bakeDllPath;
        private string bakPdbPath;

        public void Init()
        {
            //this.logger.AppendLine($"[GameEvent] 开始注入");

            this.Initialize_IO();
            this.BackUpDll();
            this.ReadDll();
        }
        private void BackUpDll()
        {
            File.Copy(this.dllPath, this.bakeDllPath);
            File.Copy(this.pdbPath, this.bakPdbPath);
        }
        private void ReadDll()
        {
            this.dllStream = new FileStream(this.bakeDllPath, FileMode.Open, FileAccess.ReadWrite);          
            var assemblyResolver = EventListener.CreateAssemblyResolver(dllPath);

            var assemblyReadParams = new ReaderParameters
            {
                ReadSymbols = true,
                AssemblyResolver = assemblyResolver,
            };

            this.assemblyDefinition = AssemblyDefinition.ReadAssembly(dllStream, assemblyReadParams);
        }
        public string backUpTempDirName => EventListener.DefindDirectory;
        private void Initialize_IO()
        {
            this.backeupDir = backUpTempDirName;

            this.bakeDllPath = $"{this.backeupDir}/bak_{this.dllNameNoExten}.dll";
            this.bakPdbPath = $"{this.backeupDir}/bak_{this.dllNameNoExten}.pdb";
        }
        public void InjectEvent()
        {           
            if (assemblyDefinition == null) return;
            TypeDefinition[] types = assemblyDefinition.MainModule.GetTypes().ToArray();

            for (int i = 0; i < types.Length; i++)
            {
                TypeDefinition t = types[i];
                if (t == null) continue;
                ForEachByMethod(t);
            }         
        }
        public bool Injected => HasInjected();
        private bool HasInjected()
        {
            var InjectedNameSpace = "YukiFrameWork";
            var InjectedClazz = "Event_Builder";

            var injectedFullName = $"{InjectedNameSpace}.{InjectedClazz}";
          
            var injected = assemblyDefinition.MainModule.Types.Any((t)
                => {                   
                    return t.FullName == injectedFullName;
                });



            return injected;
        }


        public void ForEachByMethod(TypeDefinition type)
        {
            InjectExecutor executor = null;
            foreach (var evt in type.Methods)
            {
                executor = InjectExecutor.GetOrCreate(evt.DeclaringType, assemblyDefinition);
                if (evt.Parameters.Count != 1) continue; 
                ParameterDefinition parameter = evt.Parameters[0];              
             
                var register = evt.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(AddListenerAttribute).FullName);              
                if (register == null) continue;
                
                if (!IsCheckParameterType(parameter.ParameterType.Resolve()))
                    continue;               
                bool onlyMonoEnable = false;
                if (register != null && evt.ReturnType.Name == typeof(void).Name)
                {
                    if(executor.IsMono(type))
                        onlyMonoEnable = (bool)register.ConstructorArguments.FirstOrDefault().Value;
                    executor.AddListener(evt,onlyMonoEnable);
                }
                else if (register != null && evt.ReturnType.Name == typeof(Task).Name)
                {
                    if (executor.IsMono(type))
                        onlyMonoEnable = (bool)register.ConstructorArguments.FirstOrDefault().Value;
                    executor.AddListener_Async(evt, onlyMonoEnable);
                }                     

            }

            foreach (var evt in type.Methods)
            {
                var remove = evt.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(RemoveAllListenersAttribute).FullName);

                if (remove == null) continue; 
                 executor = InjectExecutor.GetOrCreate(evt.DeclaringType, assemblyDefinition);
                executor.RemoveListener(evt,remove);
            }         
        }

        bool Exist(ref string path, string euffix = ".dll")
        {
            path = Path.ChangeExtension(path, euffix);
            return File.Exists(path);
        }

        private bool IsCheckParameterType(TypeDefinition type)
        {
            if (type.FullName == typeof(IEventArgs).FullName)
                return true;
            foreach (var item in type.NestedTypes)
            {
                if(IsCheckParameterType(item))
                    return true;
            }

            foreach (var interfaces in type.Interfaces)
            {
                if (interfaces.InterfaceType.FullName == typeof(IEventArgs).FullName)
                    return true;
            }
           
            return false;
        }

        public void Dispose()
        {
            if (assemblyDefinition != null && assemblyDefinition.MainModule.SymbolReader != null)
            {
                assemblyDefinition.MainModule.SymbolReader.Dispose();
                assemblyDefinition.Dispose();
            }

            if (dllStream != null)
            {
                dllStream.Close();
                dllStream = null;
            }
        }

        public void Write()
        {
            var writeParam = new WriterParameters
            {
                WriteSymbols = true,
            };        
            assemblyDefinition.Write(this.dllPath, writeParam);
        }
    }
}
