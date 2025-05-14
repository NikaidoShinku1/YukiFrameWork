///=====================================================
/// - FileName:      SyncAllTransformSystem.cs
/// - NameSpace:     YukiFrameWork.Entities
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/25 14:49:28
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Unity.Entities;
using UnityEngine.Scripting;
using Unity.Transforms;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
namespace YukiFrameWork.Entities
{
    [BurstCompile]
    [UpdateInGroup(typeof(SyncTransformSystemGroup))]
    [UpdateAfter(typeof(ReadSyncAllTransformSystem))]    
    internal partial struct WriteSyncAllTransformSystem : Unity.Entities.ISystem
    {
        private EntityQuery transformConvertEntitiesQuery;
        private ComponentLookup<LocalTransform> localTransforms;
        public void OnCreate(ref SystemState state)
        {
            transformConvertEntitiesQuery = SystemAPI
                .QueryBuilder()
                .WithAll<Transform>()
                .WithAllRW<LocalTransform>()
                .Build();
            localTransforms = state.GetComponentLookup<LocalTransform>();          
        }      

        public void OnDestroy(ref SystemState state) 
        {

        }
      
        public void OnUpdate(ref SystemState state) 
        {                      
            var entities = transformConvertEntitiesQuery.ToEntityArray(Allocator.TempJob);
            var array = transformConvertEntitiesQuery.GetTransformAccessArray();
            localTransforms.Update(ref state);
            state.Dependency = new WriteSyncJob()
            {
                entities = entities,
                localTransforms = localTransforms
            }.Schedule(array,state.Dependency);

        }
    }
    [BurstCompile]
    internal struct WriteSyncJob : IJobParallelForTransform
    {
        [NativeDisableParallelForRestriction]
        public ComponentLookup<LocalTransform> localTransforms;
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> entities;
        public void Execute(int index, [ReadOnly]TransformAccess transformAccess)
        {
            Entity entity = entities[index];

            LocalTransform localTransform = localTransforms[entity];
            localTransform.Position = transformAccess.position;
            localTransform.Rotation = transformAccess.rotation;
            localTransforms[entity] = localTransform;
        }
    }
}
