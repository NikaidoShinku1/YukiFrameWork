///=====================================================
/// - FileName:      BuffKit.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/5 16:15:51
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections.Generic;
using System.Collections;
using YukiFrameWork.Pools;
using UnityEngine;
using YukiFrameWork.Extension;
using System.Linq;

namespace YukiFrameWork.Buffer
{

    /// <summary>
    /// Buff的统一管理套件
    /// </summary>
    public static class BuffKit
    {
        private static IBuffLoader buffLoader = null;
        /// <summary>
        /// 缓存的所有Buff
        /// </summary>
        private static Dictionary<string, IBuff> runtime_buffs = new Dictionary<string, IBuff>();

        private static Dictionary<IBuffExecutor, List<BuffController>> allExecutor_BuffControllers = new Dictionary<IBuffExecutor, List<BuffController>>();

        public static void Init(string projectName)
        {
            Init(new ABManagerBuffLoader(projectName));
        }

        public static void Init(IBuffLoader buffLoader)
        {
            //初始化清空所有缓存
            runtime_buffs.Clear();

            allExecutor_BuffControllers.Clear();

            //赋值加载器
            BuffKit.buffLoader = buffLoader;
        }
        [RuntimeInitializeOnLoadMethod]
        public static void UpdateInit()
        {
            MonoHelper.Update_RemoveListener(Update);
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            MonoHelper.LateUpdate_RemoveListener(LateUpdate);

            MonoHelper.Update_AddListener(Update);
            MonoHelper.FixedUpdate_AddListener(FixedUpdate);
            MonoHelper.LateUpdate_AddListener(LateUpdate);
        }

        /// <summary>
        /// 添加单一个Buff数据
        /// </summary>
        /// <param name="buff"></param>
        public static void AddBuffData(IBuff buff)
        {
            runtime_buffs.Add(buff.Key, buff);
        }

        /// <summary>
        /// 移除单一个Buff数据,该APi不会影响已经正在执行的Buff
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveBuffData(string key)
        {
            runtime_buffs.Remove(key);
        }

        /// <summary>
        /// 移除单一个Buff数据,该APi不会影响已经正在执行的Buff
        /// </summary>
        /// <param name="buff"></param>
        public static void RemoveBuffData(IBuff buff)
        {
            RemoveBuffData(buff.Key);
        }

        /// <summary>
        /// 加载BuffDataBase载入所有的Buff。使用该API加载的BuffDataBase在使用后自动释放
        /// </summary>
        /// <param name="name"></param>
        public static void LoadBuffDataBase(string name)
        {
            var item = buffLoader.Load<BuffDataBase>(name);
            LoadBuffDataBase(item);
            buffLoader?.UnLoad(item);
        }

        /// <summary>
        /// 加载BuffDataBase载入所有的Buff。
        /// </summary>
        /// <param name="buffDataBase"></param>
        public static void LoadBuffDataBase(BuffDataBase buffDataBase)
        {
            foreach (var item in buffDataBase.buffConfigs)
                runtime_buffs.Add(item.Key, item.Instantiate());
        }

        /// <summary>
        /// 异步加载BuffDataBase载入所有的Buff。使用该API加载的BuffDataBase在使用后自动释放
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerator LoadBuffDataBaseAsync(string name)
        {
            bool completed = false;

            buffLoader.LoadAsync<BuffDataBase>(name, data =>
            {
                completed = true;
                LoadBuffDataBase(data);
            });

            yield return CoroutineTool.WaitUntil(() => completed);
        }

        /// <summary>
        /// 获取某一个执行者所有正在执行的Buff的效果
        /// </summary>        
        /// <returns></returns>
        public static List<IEffect> GetEffects(this IBuffExecutor player)
        {
            List<IEffect> results = new List<IEffect>();
            GetEffectsNonAlloc(player, results);
            return results;
        }

        /// <summary>
        /// 获取某一个执行者正在执行的Buff指定对象类型的效果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void GetEffectsNonAlloc<T>(this IBuffExecutor player, List<T> results) where T : IEffect
        {
            if (results == null)
                throw new Exception("集合不能为空");

            results.Clear();

            if (player == null) return;

            CacheExecutor(player);

            var buffers = allExecutor_BuffControllers[player];
            foreach (var item in buffers)
            {
                foreach (var effect in item.Buff.EffectDatas)
                {
                    if (!(effect is T t)) continue;
                    results.Add(t);
                }
            }
        }

        /// <summary>
        /// 获取某一个执行者正在执行Buff的指定标识的效果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<IEffect> GetEffects(this IBuffExecutor player, string key)
        {
            List<IEffect> results = new List<IEffect>();
            GetEffectsNonAlloc(player, results, key);
            return results;
        }

        /// <summary>
        /// 获取某一个执行者正在执行Buff的指定标识的具体的某一个效果
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="effect_key"></param>
        /// <returns></returns>
        public static IEffect GetEffect(this IBuffExecutor player, string key, string effect_key)
        {
            var effects = GetEffects(player, key);

            foreach (var item in effects)
            {
                if (item.Key == effect_key)
                    return item;
            }

            return null;
        }

        /// <summary>
        ///  获取某一个执行者正在执行Buff的指定标识的效果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void GetEffectsNonAlloc(this IBuffExecutor player, List<IEffect> results, string key)
        {
            if (results == null)
                throw new Exception("集合不能为空");

            results.Clear();

            if (player == null) return;

            CacheExecutor(player);

            var buffers = allExecutor_BuffControllers[player];
            foreach (var item in buffers)
            {
                foreach (var effect in item.Buff.EffectDatas)
                {
                    if (effect.Key != key) continue;

                    results.Add(effect);
                }
            }
        }

        /// <summary>
        /// 获取某一个执行者正在执行Buff的指定类型的效果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<IEffect> GetEffectsByType(this IBuffExecutor player, string type)
        {
            List<IEffect> results = new List<IEffect>();
            GetEffectsNonAllocByType(player, results, type);
            return results;
        }

        /// <summary>
        ///  获取某一个执行者正在执行Buff的指定类型的效果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void GetEffectsNonAllocByType(this IBuffExecutor player, List<IEffect> results, string type)
        {
            if (results == null)
                throw new Exception("集合不能为空");

            results.Clear();

            if (player == null) return;

            CacheExecutor(player);

            var buffers = allExecutor_BuffControllers[player];
            foreach (var item in buffers)
            {
                foreach (var effect in item.Buff.EffectDatas)
                {
                    if (effect.Type != type) continue;

                    results.Add(effect);
                }
            }
        }

        /// <summary>
        /// 获取Buff数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IBuff GetBuffData(string key)
        {
            runtime_buffs.TryGetValue(key, out var buff);
            if (buff == null)
                throw new Exception($"Buff丢失!请检查是否已经载入Buff Key:{key}");
            return buff;
        }

        /// <summary>
        /// 获取指定玩家下所有的BuffController       
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static List<BuffController> GetBuffControllers(this IBuffExecutor player)
        {
            CacheExecutor(player);
            return allExecutor_BuffControllers[player];
        }

        /// <summary>
        /// 获取指定玩家下的指定BuffController
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buffKey"></param>
        /// <returns></returns>
        public static BuffController GetBuffController(this IBuffExecutor player,string buffKey)
        {
            CacheExecutor(player);

            var controllers = GetBuffControllers(player);

            foreach (var item in controllers)
            {
                if(item.IsMarkIdle) continue;

                if (item.Buff.Key == buffKey)
                    return item;

            }
            return null;
        }

        /// <summary>
        /// 根据Buff标识添加Buff
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buffKey"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static BuffController AddBuff(this IBuffExecutor player,string buffKey,params object[] param)
        {
            return AddBuff(player, GetBuffData(buffKey), param);
        }

        /// <summary>
        /// 直接传递Buff配置以添加Buff
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buff"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static BuffController AddBuff(this IBuffExecutor player, IBuff buff, params object[] param)
        {          
            return AddBuff(player, buff, buff.Duration, param);
        }

        /// <summary>
        /// 根据Buff标识添加Buff，可动态设置持续时间
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buffKey"></param>
        /// <param name="duration"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static BuffController AddBuff(this IBuffExecutor player, string buffKey, float duration, params object[] param)
        {
            return AddBuff(player, GetBuffData(buffKey),duration, param);
        }
        /// <summary>
        /// 直接传递Buff配置以添加Buff,可动态设置持续时间
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buff"></param>
        /// <param name="duration"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public static BuffController AddBuff(this IBuffExecutor player, IBuff buff, float duration, params object[] param)
        {
            if (player == null)
                return null;

            if (buff == null)
                return null;           

            if (!player.OnAddBuffCondition()) return null;

            CacheExecutor(player);

            List<BuffController> controllers = allExecutor_BuffControllers[player];

            if (player is Component component)
            {
                if (!component || !component.ActiveInHierarchy())
                {
                    LogKit.W($"传递的执行者非活动状态!无法添加Buff GameObject:{component.name}");
                    return null;
                }
            }
            Type type = AssemblyHelper.GetType(buff.BuffControllerType);

            if (type == null || !type.IsSubclassOf(typeof(BuffController)))
                throw new InvalidCastException($"类型不正确，无法构建BuffController Type:{buff.BuffControllerType}");
            BuffController buffController = BuffController.CreateInstance(type,buff,player);

            if (!buffController.OnAddBuffCondition())
            {
                buffController.GlobalRelease();
                return null;
            }
            buffController.Duration = duration;
            buffController.Add(param);           

            if (buff.BuffMode == BuffMode.Single)
            {
                BuffController current = null;

                foreach (var item in controllers)
                {
                    if (item.Buff.Key == buff.Key)
                    {
                        current = item;
                        break;
                    }
                }

                if (current != null)
                {
                    RemoveBuff(player,current);
                }
            }

            if (!controllers.Contains(buffController))
                controllers.Add(buffController);
            allExecutor_BuffControllers[player] = controllers;
            player.OnAdd(buffController);

            return buffController;
        }

        /// <summary>
        /// 移除执行者所有的Buff
        /// </summary>
        /// <param name="player"></param>
        public static void RemoveBuff(this IBuffExecutor player)
        {
            if (player == null) return;
            if (!allExecutor_BuffControllers.TryGetValue(player, out var controllers))
                return;

            if (controllers == null || controllers.Count == 0)
                allExecutor_BuffControllers.Remove(player);

            for (int i = controllers.Count - 1; i >= 0; i--)
            {
                RemoveBuff(player, controllers[i]);
            }            
                
        }

        /// <summary>
        /// 移除指定的Buff
        /// </summary>
        /// <param name="player"></param>
        /// <param name="controller"></param>
        public static void RemoveBuff(this IBuffExecutor player, BuffController controller)
        {
            if (player == null) return;
            if (controller == null) return;
            if (!allExecutor_BuffControllers.ContainsKey(player))
                return;
            if (!allExecutor_BuffControllers[player].Contains(controller))
                return;

            allExecutor_BuffControllers[player].Remove(controller);            
            controller.Remove();
            player.OnRemove(controller);            
            controller.GlobalRelease();
        }

        /// <summary>
        /// 移除一组Buff
        /// </summary>
        /// <param name="player"></param>
        /// <param name="controllers"></param>
        public static void RemoveBuff(this IBuffExecutor player, IList<BuffController> controllers)
        {
            if (player == null) return;

            if (controllers == null || controllers.Count == 0) return;

            if (!allExecutor_BuffControllers.TryGetValue(player, out var item))
                return;

            if (item == controllers)
                RemoveBuff(player);
            else
            {
                foreach (var temp in controllers)
                {
                    RemoveBuff(player, temp);
                }
            }
                
        }

        /// <summary>
        /// 移除指定类型所有的Buff
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="player"></param>
        public static void RemoveBuff<T>(this IBuffExecutor player) where T : BuffController
        {
            if (player == null) return;

            if (!allExecutor_BuffControllers.TryGetValue(player, out var item))
                return;
            var target = item.FindAll(x => x.GetType() == typeof(T));
            RemoveBuff(player, target);
        }

        /// <summary>
        /// 根据Buff标识移除Buff
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        public static void RemoveBuff(this IBuffExecutor player, string key)
        {
            if (player == null) return;

            if (!allExecutor_BuffControllers.TryGetValue(player, out var item))
                return;
            RemoveBuff(player,item.Find(x => x.Buff.Key == key));
        }

        /// <summary>
        /// 判断执行者正在执行的Buff有没有指定的效果
        /// </summary>
        /// <param name="player"></param>
        /// <param name="effect_key"></param>
        /// <returns></returns>
        public static bool IsContainEffect(this IBuffExecutor player, string effect_key)
        {
            if (!allExecutor_BuffControllers.TryGetValue(player, out var controllers))
                return false;

            foreach (var item in controllers)
            {
                foreach (var effect in item.Buff.EffectDatas)
                {
                    if (effect.Key == effect_key)
                        return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 查询执行者是否存在指定的Buff
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buffKey"></param>
        /// <returns></returns>
        public static bool IsContainBuff(this IBuffExecutor player, string buffKey)
        {
            return IsContainBuff(player, buffKey, out _);
        }

        public static bool IsContainBuff(this IBuffExecutor player, string buffKey,out BuffController controller)
        {
            controller = null;
            if (!allExecutor_BuffControllers.TryGetValue(player, out var items))
                return false;

            foreach (var item in items)
            {
                if (item.Buff.Key == buffKey)
                {
                    controller = item;
                    return true;
                }
            }

            return false;
        }



        private static void CacheExecutor(IBuffExecutor player)
        {
            if (!allExecutor_BuffControllers.ContainsKey(player))
                allExecutor_BuffControllers[player] = new List<BuffController>();           
        }

        private static List<IBuffExecutor> results = new List<IBuffExecutor>();
        private static void Update(MonoHelper monoHelper)
        {
            foreach (var executor in allExecutor_BuffControllers)
            {
                if (IsCheckDestroy(executor.Key))
                {
                    results.Add(executor.Key);
                    continue;
                }

                if (IsDisableBuff(executor.Key)) continue;


                for (int i = 0; i < executor.Value.Count; i++)
                {

                    BuffController controller = executor.Value[i];

                    controller.Update();
                    if (controller.Progress >= 1)
                    {                       
                        RemoveBuff(executor.Key, controller);
                        i--;
                    }
                }

                if (executor.Value.Count == 0)
                     results.Add(executor.Key);

            }
        }

        private static void FixedUpdate(MonoHelper monoHelper)
        {
            foreach (var executor in allExecutor_BuffControllers)
            {
                if (IsCheckDestroy(executor.Key))
                {
                    results.Add(executor.Key);
                    continue;
                }

                if (IsDisableBuff(executor.Key)) continue;


                for (int i = 0; i < executor.Value.Count; i++)
                {

                    BuffController controller = executor.Value[i];

                    controller.FixedUpdate();
                   
                    
                }

            }
        }

        private static void LateUpdate(MonoHelper monoHelper)
        {
            foreach (var executor in allExecutor_BuffControllers)
            {
                if (IsCheckDestroy(executor.Key))
                {
                    results.Add(executor.Key);
                    continue;
                }

                if (IsDisableBuff(executor.Key)) continue;


                for (int i = 0; i < executor.Value.Count; i++)
                {

                    BuffController controller = executor.Value[i];

                    controller.LateUpdate();
                    
                }
            }

            if (results.Count == 0) return;

            foreach (var item in results)
            {
                if (allExecutor_BuffControllers.ContainsKey(item))
                {
                    item.RemoveBuff();
                    allExecutor_BuffControllers.Remove(item);
                }
                
            }
            results.Clear();
        }

        
        private static bool IsDisableBuff(IBuffExecutor player)
        {
            Component component = player as Component;
            return !component.ActiveInHierarchy();
        }

        private static bool IsCheckDestroy(IBuffExecutor player)
        {
            bool destroy = IsDestroyBuff(player);

            if (destroy)
            {
                RemoveBuff(player);
            }

            return destroy;
        }


        /// <summary>
        /// 判断buff是否被销毁
        /// </summary>
        private static bool IsDestroyBuff(IBuffExecutor player)
        {
            if (!(player is UnityEngine.Object)) return false;
            UnityEngine.Object obj = player as UnityEngine.Object;
            return !obj;
        }

    }
}
