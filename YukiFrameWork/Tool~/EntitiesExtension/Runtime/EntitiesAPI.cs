///=====================================================
/// - FileName:      BRGRenderKit.cs
/// - NameSpace:     YukiFrameWork.Entities
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/28 15:49:10
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Unity.Entities;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Entities.Graphics;
namespace YukiFrameWork.Entities
{
    public class EntitiesAPI 
    {    
        /// <summary>
        /// 封锁Transform，默认为True，在GameObject成功转换为Entity且仍使用GameObject作为主导体的情况下，主线程仅能使用LocalTransform属性。
        /// <para>默认情况下，无法进行任何对于Transform的获取与赋值操作，将此属性设置为False以解禁Transform,但同时会导致LocalTransform无法使用</para>
        /// <para>根据项目需求选择。这取决于逻辑重心在Mono 或 ECS</para>       
        /// </summary>
        public static bool BlockTransformForceProcessing
        {
            get
            {
                return World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SyncTransformSystemGroup>().Enabled;
            }
            set
            {
                World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SyncTransformSystemGroup>().Enabled = value;    
            }
        }

        /// <summary>
        /// 批量克隆EntityMonoBehaviour实体
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="entity"></param>
        /// <param name="instantiateCount"></param>
        /// <param name="row"></param>
        /// <param name="positions"></param>
        public static void Instantiate(EntityMonoBehaviour behaviour,int instantiateCount, int row, InstantiatePositionCallBack positions)
        {
            Instantiate(behaviour.EntityManager,behaviour.Entity,instantiateCount,row,positions);
            behaviour.gameObject.Destroy();

        }

        /// <summary>
        /// 批量克隆EntityMonoBehaviour实体
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="entity"></param>
        /// <param name="instantiateCount"></param>
        /// <param name="row"></param>
        /// <param name="positions"></param>
        public static void Instantiate(EntityManager entityManager,Entity entity, int instantiateCount, int row, InstantiatePositionCallBack positions)
        {
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            NativeArray<LocalTransform> transforms = new NativeArray<LocalTransform>(instantiateCount, Allocator.TempJob);

            for (int i = 0; i < instantiateCount; i++)
            {
                transforms[i] = positions(i, row);
            }

            InstantiateJob instantiateJob = new InstantiateJob()
            {
                root = entity,
                EntityCount = instantiateCount,
                Ecb = commandBuffer.AsParallelWriter(),
                localTransforms = transforms
            };
            var handle = instantiateJob.Schedule(instantiateCount, 4096);
            handle.Complete();
            commandBuffer.Playback(entityManager);
            commandBuffer.Dispose();
            transforms.Dispose();
        }

        /// <summary>
        /// 使用ECSGraphics进行渲染绘制
        /// <para>EntityMonoBehaviour选择GameObject转换而非批量时，可调用动态修改个体的渲染材质信息</para>
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="renderMeshDescription"></param>
        /// <param name="renderMeshArray"></param>
        /// <param name="materialMeshInfo"></param>
        public static void EntitiesGraphicsRenderingGameObject(EntityMonoBehaviour monoBehaviour,RenderMeshDescription renderMeshDescription, RenderMeshArray renderMeshArray, MaterialMeshInfo materialMeshInfo)
        {
            RenderMeshUtility.AddComponents(monoBehaviour.Entity, monoBehaviour.EntityManager, renderMeshDescription, renderMeshArray, materialMeshInfo);      
        }

        /// <summary>
        /// 使用ECSGraphics进行渲染绘制
        /// <para>EntityMonoBehaviour选择GameObject转换而非批量时，可调用动态修改个体的渲染材质信息</para>
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="renderMeshDescription"></param>
        /// <param name="materialMeshInfo"></param>
        public static void EntitiesGraphicsRenderingGameObject(EntityMonoBehaviour monoBehaviour, RenderMeshDescription renderMeshDescription, MaterialMeshInfo materialMeshInfo)
        {
            RenderMeshUtility.AddComponents(monoBehaviour.Entity, monoBehaviour.EntityManager, renderMeshDescription,  materialMeshInfo);
        }
        /// <summary>
        /// 使用ECSGraphics进行渲染绘制
        /// <para>EntityMonoBehaviour选择GameObject转换而非批量时，可调用动态修改个体的渲染材质信息</para>
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="renderMeshDescription"></param>
        /// <param name="materials"></param>
        /// <param name="meshs"></param>
        /// <param name="indices"></param>
        public static void EntitiesGraphicsRenderingGameObject(EntityMonoBehaviour monoBehaviour,RenderMeshDescription renderMeshDescription, Material[] materials, Mesh[] meshs,int indices)
        {
            RenderMeshArray renderMeshArray = new RenderMeshArray(materials, meshs);
            MaterialMeshInfo materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(indices,indices);
            EntitiesGraphicsRenderingGameObject(monoBehaviour, renderMeshDescription, renderMeshArray, materialMeshInfo);
        }

        /// <summary>
        /// 使用ECSGraphics进行渲染绘制 
        /// <para>EntityMonoBehaviour选择GameObject转换而非批量时，可调用动态修改个体的渲染材质信息</para>
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="renderMeshDescription"></param>
        /// <param name="material"></param>
        /// <param name="mesh"></param>
        public static void EntitiesGraphicsRenderingGameObject(EntityMonoBehaviour monoBehaviour, RenderMeshDescription renderMeshDescription, Material material, Mesh mesh)
        {
            EntitiesGraphicsRenderingGameObject(monoBehaviour,renderMeshDescription,new Material[] {material},new Mesh[] {mesh},0);
        }

        /// <summary>
        /// 使用ECSGraphics进行批量渲染绘制 该方法仅能设置渲染实体所保存的RenderMeshArray信息内传递索引同步。
        /// </summary>
        /// <param name="entities">应该传入生命周期为TempJob的实体集合</param>
        /// <param name="renderMeshDescription"></param> 
        /// <param name="indices"></param>
        public static void EntitiesGraphicsRenderingBatch(NativeArray<Entity> entities,RenderMeshDescription renderMeshDescription, int indices)
        {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            RenderMeshJob renderMeshJob = new RenderMeshJob() {entities = entities,renderIndex = indices,IsFilter = true,filterSettings =renderMeshDescription.FilterSettings,Ecb = entityCommandBuffer.AsParallelWriter() };            EntitiesGraphicsRenderingBatch(ref entities,ref entityCommandBuffer,ref renderMeshJob);
            
        }

        /// <summary>
        ///  使用ECSGraphics进行批量渲染绘制 该方法仅能设置渲染实体所保存的RenderMeshArray信息内传递索引同步。
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="indices"></param>
        public static void EntitiesGraphicsRenderingBatch(NativeArray<Entity> entities, int indices)
        {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            RenderMeshJob renderMeshJob = new RenderMeshJob() { entities = entities, renderIndex = indices, IsFilter = false,Ecb = entityCommandBuffer.AsParallelWriter() };
            EntitiesGraphicsRenderingBatch(ref entities, ref entityCommandBuffer, ref renderMeshJob);
        }

        internal static void EntitiesGraphicsRenderingBatch(ref NativeArray<Entity> entities, ref EntityCommandBuffer entityCommandBuffer, ref RenderMeshJob renderMeshJob)
        {
            int length = entities.Length;
            var handle = renderMeshJob.Schedule(length, length > 3 ? length / 3 : 1);
            handle.Complete();
            entityCommandBuffer.Playback(World.DefaultGameObjectInjectionWorld.EntityManager);
            entityCommandBuffer.Dispose();
            entities.Dispose();

        }

    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class SyncTransformSystemGroup : ComponentSystemGroup
    {
        
    }
}
