///=====================================================
/// - FileName:      RenderMeshJob.cs
/// - NameSpace:     YukiFrameWork.Entities
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/5/1 16:08:02
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Entities.Graphics;
using Unity.Mathematics;
namespace YukiFrameWork.Entities
{
    public partial struct RenderMeshJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Entity> entities;

        public bool IsFilter;
        public RenderFilterSettings filterSettings;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public int renderIndex;
        public void Execute(int index)
        {           
            var entity = entities[index];
            Ecb.AddComponent(index, entity, MaterialMeshInfo.FromRenderMeshArrayIndices(renderIndex, renderIndex));
            if(IsFilter)
                Ecb.AddSharedComponent(index, entity, filterSettings);
        }
    }
}
