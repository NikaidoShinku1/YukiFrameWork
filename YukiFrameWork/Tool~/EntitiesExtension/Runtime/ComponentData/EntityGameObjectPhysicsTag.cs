///=====================================================
/// - FileName:      EntityPhysicsTag.cs
/// - NameSpace:     YukiFrameWork.Entities
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/5/1 2:14:14
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Unity.Entities;
using Sirenix.OdinInspector;
namespace YukiFrameWork.Entities
{
    /// <summary>
    /// 实体物理标签组件。标记该实体仍需以GameObject为主设计物理碰撞检测。
    /// <para>当实体拥有该组件且EntityMonoBehaviour的转换类型为GameObject个体转换时，个体将解禁对于Transform的写入，同时无法进行LocalTransform的写入更新。</para>
    /// <para>Tip:该组件仅对GameObject转换有效，当使用批量渲染纯实体时，该组件形同虚设</para>
    /// </summary>
    public struct EntityGameObjectPhysicsTag : IComponentData
    {
      
    }
}
