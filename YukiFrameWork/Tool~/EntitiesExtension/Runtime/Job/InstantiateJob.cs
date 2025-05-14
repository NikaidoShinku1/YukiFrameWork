///=====================================================
/// - FileName:      InstantiateJob.cs
/// - NameSpace:     YukiFrameWork.Entities
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/30 20:56:38
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
namespace YukiFrameWork.Entities
{
    [BurstCompile]
    public struct InstantiateJob : IJobParallelFor
    {
        public Entity root;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public int EntityCount;
        [Unity.Collections.ReadOnly]
        public NativeArray<LocalTransform> localTransforms;
        public void Execute(int index)
        {
            var entity = Ecb.Instantiate(index, root);
            Ecb.SetComponent(index, entity, localTransforms[index]);
        }
    }

    /// <summary>
    /// 实体实例化构建(无GameObject)坐标赋值
    /// </summary>
    /// <param name="index">索引</param>
    /// <param name="row">行数</param>
    /// <returns></returns>
    public delegate LocalTransform InstantiatePositionCallBack(int index, int row);

}
