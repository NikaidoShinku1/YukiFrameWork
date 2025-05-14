///=====================================================
/// - FileName:      ReadAllTransformSystem.cs
/// - NameSpace:     YukiFrameWork.Entities
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/25 19:12:00
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine.Jobs;
using Unity.Collections;
namespace YukiFrameWork.Entities
{
    [BurstCompile]
    [UpdateInGroup(typeof(SyncTransformSystemGroup))]
    internal partial struct ReadSyncAllTransformSystem : Unity.Entities.ISystem
    {

        private EntityQuery transformConvertEntitiesQuery;
        private ComponentLookup<LocalTransform> localTransforms;
        public void OnCreate(ref SystemState state)
        {
            transformConvertEntitiesQuery = SystemAPI
                .QueryBuilder()
                .WithAll<Transform>()
                .WithAllRW<LocalTransform>()
                .WithNone<EntityGameObjectPhysicsTag>()
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
            state.Dependency = new ReadSyncJob()
            {
                entities = entities,
                localTransforms = localTransforms
            }.Schedule(array, state.Dependency);
        }

    }
    [BurstCompile]
    internal struct ReadSyncJob : IJobParallelForTransform
    {
        [ReadOnly]
        public ComponentLookup<LocalTransform> localTransforms;
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> entities;
        public void Execute(int index, TransformAccess transformAccess)
        {
            Entity entity = entities[index];

            LocalTransform localTransform = localTransforms[entity];
            transformAccess.position = localTransform.Position;
            transformAccess.rotation = localTransform.Rotation;
        }
    }

}
