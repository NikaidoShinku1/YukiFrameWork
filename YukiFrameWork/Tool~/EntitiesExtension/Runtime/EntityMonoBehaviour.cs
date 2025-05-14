///=====================================================
/// - FileName:      EntityMonoBehaviour.cs
/// - NameSpace:     YukiFrameWork.Entities
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/25 13:51:24
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Transforms;
using System.Linq;
using Unity.Rendering;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Burst;
namespace YukiFrameWork.Entities
{   
 
    public abstract partial class EntityMonoBehaviour : ViewController
    {
        private Entity entity;      
       
        [SerializeField, LabelText("默认对象缩放")]
        private float defaultScale = 1;
        private EntityArchetype defaultArchetype;

        /// <summary>
        /// 可重写的实体原型
        /// <para>默认的实体原型提供:</para>
        /// <list type="number">
        /// <item>LocalToWorld</item>
        /// <item>LocalTransform</item>
        /// </list>
        /// </summary>
        protected virtual EntityArchetype Archetype => defaultArchetype;      

        /// <summary>
        /// 当EntityMonoBehaviour的转换模式并不是一对一转换时，可重写的实例化实体位置回调
        /// </summary>
        protected virtual InstantiatePositionCallBack InstantiatePositionCallBack => (count, row) =>
        {
            return LocalTransform.WithPosition(new float3(count,this.Position.y,this.Position.z));
        };

        public Entity Entity => entity;
        public World World => World.DefaultGameObjectInjectionWorld;
        public EntityManager EntityManager => World.EntityManager;

       
        protected override void Awake()
        {            
            base.Awake();           
            Init();       
            
        }

        private void Init()
        {
            defaultArchetype = EntityManager.CreateArchetype(typeof(LocalToWorld), typeof(LocalTransform));

            entity = GetOrCreateEntity();
            EntityManager.AddComponentObject(entity, transform);
            this.Position = transform.position;
            this.Rotation = transform.rotation;
            this.Scale = defaultScale;
            UpdateRendering();            
        }

        /// <summary>
        /// 在对象Awake初始化更新渲染的方法,如GameObject仍打算使用传统渲染方式例如MeshRenderer组件，则不需要在意该方法。
        /// </summary>
        protected abstract void UpdateRendering();

        /// <summary>
        /// 实体EntityMonoBehaviour应该访问的名称属性，这用于同步修改Entity的名称
        /// </summary>
        public string Name
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
                EntityManager.SetName(entity, value);
            }
        }

        /// <summary>
        /// 当GameObject挂载EntityMonoBehaviour后，Transform会被默认禁用，如要在主线程(非ECS)下进行坐标修改 请调用该API并以new的形式进行赋值 如希望仍使用Transform，可为实体添加EntityGameObjectPhysicsTag组件。
        /// <para>当Entity存在Parent时，此组件相对于父级Entity。否则相对于世界空间。</para>
        /// <code>this.LocalTransform = new LocalTransform();</code>
        /// </summary>
        public LocalTransform LocalTransform
        {
            get
            {
                return EntityManager.GetComponentData<LocalTransform>(entity);
            }
            set
            {
                EntityManager.SetComponentData<LocalTransform>(entity, value);             
            }
        }

        /// <summary>
        /// 对于LocalTransform --> Vector3的位置封装
        /// </summary>
        public Vector3 Position 
        {
            get
            {
                LocalTransform localTransform = this.LocalTransform;
                return new Vector3(localTransform.Position.x, localTransform.Position.y, localTransform.Position.z);
            }
            set
            {
                LocalTransform localTransform = this.LocalTransform;
                localTransform.Position = new float3(value.x, value.y, value.z);
                this.LocalTransform = localTransform;
            }
        }

        /// <summary>
        /// 对于LocalTransform ---> Quaternion的封装
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                LocalTransform localTransform = this.LocalTransform;
                return localTransform.Rotation;
            }
            set
            {
                var localTransform = this.LocalTransform;
                localTransform.Rotation = value;
                this.LocalTransform = localTransform;
            }
        }

        /// <summary>
        /// 对于已经被转换为Entity的GameObject，仅能等比缩放大小
        /// </summary>
        public float Scale
        {
            get => this.LocalTransform.Scale;
            set
            {
                var localTransform = this.LocalTransform;
                localTransform.Scale = value;
                this.LocalTransform = localTransform;
            }
        }
       
        internal Entity GetOrCreateEntity()
        {
            if (entity != Entity.Null)
                return entity;

            entity = EntityManager.CreateEntity(Archetype);
            EntityManager.SetName(entity, gameObject.name);
            return entity;
        }    
        
        protected virtual void OnEnable()
        {           
            if (World == null) return;
            EntityManager.SetEnabled(entity, true);
        }

        protected virtual void OnDisable()
        {
            if (World == null) return;
            EntityManager.SetEnabled(entity,false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (World == null) return;
            EntityManager.DestroyEntity(entity);
        }

#if UNITY_EDITOR
        [SerializeField,HideInInspector]
        internal bool IsWarningTip;
        public override bool OnInspectorGUI()
        {            
            if (!IsWarningTip)
            {
                UnityEditor.EditorGUILayout.HelpBox("EntityMonoBehaviour为GameObject进行对实体的转换。以下提示仅对于GameObject转换个体实体有效:" +
                    "\n\n如在主线程/Mono中访问，请访问该组件提供的LocalTransform属性" +
                    "\n\n请注意:当该组件被挂载后，默认情况下对于该GameObject的所有Transform操作均失效，对于已经被转换的实体，Transform父子结构/LocalPosition 包括Rigidbody设置操作等均以失效" +
                    "\n\n对于碰撞检测，因仍是GameObject固原有TriggerEnter、CollisionEnter等检测方法仍可由碰撞盒的存在而生效，但所有的物理模拟也会失效(如即便是两个非触发器也会正常穿过)" +
                    "\n\n如您对所有实体都不希望封锁Transform的写入，仍需依赖Mono进行位置变换操作，可调用YukiFrameWork.EntitiesGraphics.EntitiesAPI.BlockTransformForceProcessing属性进行对封锁的全面解禁操作。" +
                    "\n\n如您针对个别GameObject希望恢复Transform的使用，可为该GameObject所转换的Entity添加EntityGameObjectPhysicsTag组件。这代表该对象仍需Mono或依赖项的物理检测，从而个体恢复Transform并禁止LocalTransform的使用" +
                    "\n\n当看完该警告并按下确认后视为您已经了解实体版GameObject的区别", UnityEditor.MessageType.Warning);

                if (GUILayout.Button("我已知晓该组件所带来的变化",GUILayout.Height(30)))
                {
                    IsWarningTip = true;
                }

                UnityEditor.EditorGUILayout.Space(10);
            }
            return base.OnInspectorGUI();
        }
#endif

    }
}
