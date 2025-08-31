///=====================================================
/// - FileName:      DiaLogKit.cs
/// - NameSpace:     YukiFrameWork.DiaLogueue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/12 20:50:57
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Pools;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections;
namespace YukiFrameWork.DiaLogue
{  
    public static class DiaLogKit 
	{
        private static IDiaLogLoader loader;
        private static Dictionary<string, DiaLogController> runtime_Controllers = new Dictionary<string, DiaLogController>();

        internal static IReadOnlyDictionary<string, DiaLogController> RuntimeControllers => runtime_Controllers;
        /// <summary>
        /// 初始化加载器
        /// </summary>
        /// <param name="projectName"></param>
        public static void Init(string projectName)
        {
            Init(new ABManagerDiaLogLoader(projectName));
        }
        /// <summary>
        /// 初始化加载器
        /// </summary>
        /// <param name="projectName"></param>
        public static void Init(IDiaLogLoader loader)
        {
            DiaLogKit.loader = loader;
        }

        [RuntimeInitializeOnLoadMethod]
        static void Init_Update()
        {
            Debug.Log("Init");
            MonoHelper.Update_RemoveListener(Update);
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            MonoHelper.LateUpdate_RemoveListener(LateUpdate);

            MonoHelper.Update_AddListener(Update);
            MonoHelper.FixedUpdate_AddListener(FixedUpdate);
            MonoHelper.LateUpdate_AddListener(LateUpdate);
        }

        internal static void UnLoad(NodeTreeBase nodeTree)
        {
            runtime_Controllers.Remove(nodeTree.Key);
            loader.UnLoad(nodeTree);
        }

        /// <summary>
        /// 释放已经加载的指定标识的对话控制器与对话配置
        /// </summary>
        /// <param name="key"></param>
        public static void Release(string key)
        {
            if (runtime_Controllers.TryGetValue(key, out var controller))            
                controller.GlobalRelease();           
        }
        /// <summary>
        /// 获取或者创建对话控制器(通过内置Loader加载，需要Init)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DiaLogController GetOrCreateController(string path)
        {
            return GetOrCreateController(loader.Load<NodeTreeBase>(path));
        }

        /// <summary>
        /// 根据标识获取控制器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static DiaLogController GetDiaLogController(string key)
        {
            runtime_Controllers.TryGetValue(key, out var controller);
            return controller;
        }

        /// <summary>
        /// 如自定义节点数据结构，可直接绕开配置构建或获取控制器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static T GetOrCreateController<T>(string key,IEnumerable<INode> nodes) where T : DiaLogController
        {
            return GetOrCreateController(key,typeof(T), nodes) as T;
        }

        /// <summary>
        /// 如自定义节点数据结构，可直接绕开配置构建或获取控制器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static DiaLogController GetOrCreateController(string key,Type type,IEnumerable<INode> nodes)
        {
            if (runtime_Controllers.TryGetValue(key, out var controller))
            {
                return controller;
            }

            controller = DiaLogController.CreateInstance(type, nodes);
            runtime_Controllers[key] = controller;
            return controller;
        }

        /// <summary>
        /// 通过自己加载的对话配置获取或创建控制器
        /// </summary>
        /// <param name="nodeTree"></param>
        /// <returns></returns>
        public static DiaLogController GetOrCreateController(NodeTreeBase nodeTree)
        {
            if (runtime_Controllers.TryGetValue(nodeTree.Key, out var controller))
            {
                return controller;
            }
            controller = DiaLogController.CreateInstance(nodeTree);
            runtime_Controllers[nodeTree.Key] = controller;
            return controller;           
        }
        /// <summary>
        /// 异步获取或者创建对话控制器(通过内置Loader加载，需要Init)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void GetOrCreateControllerAsync(string path,Action<DiaLogController> completed)
        {           
            loader.LoadAsync<NodeTreeBase>(path, nodeTree => completed?.Invoke(GetOrCreateController(nodeTree)));
        }

        /// <summary>
        /// 启动对话控制器 指定对话节点进行直接性的跳转对话
        /// </summary>
        /// <param name="controller"></param>
        public static void Start(this DiaLogController controller,int id,params object[] param)
        {
            controller.Start(id,param);
        }
        /// <summary>
        /// 启动对话控制器 指定对话节点进行直接性的跳转对话
        /// </summary>
        /// <param name="controller"></param>
        public static void Start(this DiaLogController controller, INode node, params object[] param)
        {
            Start(controller,node.Id,param);
        }

        /// <summary>
        /// 启动对话控制器 从根节点开始进行对话
        /// </summary>
        /// <param name="controller"></param>
        public static void Start(this DiaLogController controller, params object[] param)
        {
            controller.Start(-1, param);
        }     
        

        /// <summary>
        /// 以当前节点进行随机推进,该方法推进随机一个连接的节点，当节点只有一个时，等同于不传linkId的MoveNext方法
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="failedTip"></param>
        /// <returns></returns>
        public static bool MoveRandom(this DiaLogController controller, out string failedTip)
        {
            if (controller.DiaLogState != DiaLogState.Running)
            {
                failedTip = "对话控制器不在运行状态，请先调用Start方法启动";
                return false;
            }

            INode node = controller.CurrentNode;

            int id = node.LinkNodes.Random();
            return MoveNextInternal(controller, id, false, out failedTip);
        }
        /// <summary>
        /// 以当前节点进行随机推进,该方法推进随机一个连接的节点，当节点只有一个时，等同于不传linkId的MoveNext方法
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool MoveRandom(this DiaLogController controller)
        {
            return MoveRandom(controller, out _);
        }
        /// <summary>
        /// 以当前节点进行推进,该方法推进第一个连接的节点。
        /// <para>如果没有连接节点，则该推进默认失败</para>
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="failedTip">如果推进返回False，则输出日志</param>
        /// <returns></returns>
        public static bool MoveNext(this DiaLogController controller, out string failedTip)
        {
            if (controller.DiaLogState != DiaLogState.Running)
            {
                failedTip = "对话控制器不在运行状态，请先调用Start方法启动";
                return false;
            }

            INode node = controller.CurrentNode;

            int id = node.LinkNodes.FirstOrDefault();
            return MoveNextInternal(controller,id,false,out failedTip);
        }
        /// <summary>
        /// 以当前节点进行推进,该方法推进第一个连接的节点。
        /// <para>如果没有连接节点，则该推进默认失败</para>
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool MoveNext(this DiaLogController controller)
        {
            return MoveNext(controller, out _);
        }
        /// <summary>
        /// 以当前节点进行推进,该方法推进指定连接的节点。
        /// <para>如果没有连接节点，则该推进默认失败</para>
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool MoveNext(this DiaLogController controller, int linkId)
        {
            return MoveNextInternal(controller, linkId, true, out _);
        }
        /// <summary>
        /// 以当前节点进行推进,该方法推进指定连接的节点。
        /// <para>如果没有连接节点，则该推进默认失败</para>
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="failedTip">如果推进返回False，则输出日志</param>
        /// <returns></returns>
        public static bool MoveNext(this DiaLogController controller, int linkId,out string failedTip)
        {
            return MoveNextInternal(controller, linkId, true,out failedTip);
        }     
        /// <summary>
        /// 在对话控制器运行状态下,可直接传入节点id进行跳转。该方法无视当前进度可强制切换
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Move(this DiaLogController controller,int id)
        {          
            return controller.MoveInternal(id);
        }
        /// <summary>
        /// 在对话控制器运行状态下,可直接传入节点id进行跳转。该方法无视当前进度可强制切换
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="id"></param>
        /// <param name="failedTip">如果推进返回False，则输出日志</param>
        /// <returns></returns>
        public static bool Move(this DiaLogController controller, int id,out string failedTip)
        {         
            return controller.MoveInternal(id,out failedTip);
        }

        internal static bool MoveNextInternal(this DiaLogController controller, int id, bool isCheck, out string failedTip)
        {
            if (controller.DiaLogState != DiaLogState.Running)
            {
                failedTip = "对话控制器不在运行状态，请先调用Start方法启动";
                return false;
            }

            var linkNodes = controller.CurrentNode.LinkNodes;
            if (linkNodes == null || linkNodes.Count == 0)
            {
                failedTip = "对话控制器不在运行状态，请先调用Start方法启动";
                return false;
            }

            if (isCheck)
            {
                for (int i = 0; i < linkNodes.Count; i++)
                {
                    if (linkNodes[i] == id)
                    {
                        return controller.MoveInternal(id, out failedTip);
                    }
                }
            }
            else
            {
                return controller.MoveInternal(id, out failedTip);
            }
            failedTip = $"默认推进失败!请检查id是否传递正确或是为当前节点的连接 linkId:{id}";
            return false;
        }

        private static void Update(MonoHelper monoHelper)
        {
            foreach (var item in runtime_Controllers.Values)
            {
                if (item.DiaLogState == DiaLogState.Running)
                    item.Update();
            }
        }

        private static void FixedUpdate(MonoHelper monoHelper)
        {
            foreach (var item in runtime_Controllers.Values)
            {
                if (item.DiaLogState == DiaLogState.Running)
                    item.FixedUpdate();
            }
        }

        private static void LateUpdate(MonoHelper monoHelper)
        {
            foreach (var item in runtime_Controllers.Values)
            {
                if (item.DiaLogState == DiaLogState.Running)
                    item.LateUpdate();
            }
        }
        
    }

  
    
}
