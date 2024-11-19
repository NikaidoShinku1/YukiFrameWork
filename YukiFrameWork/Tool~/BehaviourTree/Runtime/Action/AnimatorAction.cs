///=====================================================
/// - FileName:      AnimatorAction.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/17 1:16:42
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
namespace YukiFrameWork.Behaviours
{
	public class AnimatorAction : Action
	{		
		protected Animator animator;

        [InfoBox("校验运行状态机，当运行组件的状态机中运行状态机参数相同才可以使用。")]
        public RuntimeAnimatorController animatorController;

        public override void OnInit()
        {
            base.OnInit();
            animator = transform.GetComponentInChildren<Animator>();
            if (!animator)
                throw new Exception("行为树运行对象不存在Animator组件!");
        }
        public override void OnStart()
        {
            base.OnStart();
            var clipName = this.clipName;
            animator.speed = animSpeed;
            AnimatorAction.SetBlendTreeParameter(this, animator);
            if (isCrossFade)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                if (stateInfo.normalizedTime >= 1f)
                    animator.Play(clipName, layer, 0f);
                else
                    animator.CrossFade(clipName, duration);
            }
            else animator.Play(clipName, layer, 0f);

        }
#if UNITY_EDITOR
        private IEnumerable animClips
        {
            get
            {
                if (!animatorController) return default;

                if (animatorController is UnityEditor.Animations.AnimatorController controller)
                {
                    return controller.layers[layer].stateMachine.states.Select(x => new ValueDropdownItem() { Text = x.state.name, Value = x.state.name });
                }

                return default;
            }
        }
#endif
        [HideInInspector]
        public List<string> clips = new List<string>();
        [HideInInspector]
        public int selectIndex;
        [LabelText("动画层级")]
        public int layer = 0;
        [LabelText("动画剪辑名称")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(animClips))]
#endif     
        public string clipName;
        [LabelText("动画是否有过渡")]
        public bool isCrossFade;
        [LabelText("过渡持续时间")]
        [ShowIf(nameof(isCrossFade))]
        public float duration = 0.25f;
        [LabelText("动画循环?")]
        [InfoBox("当动画不循环时，完成动画后节点会返回成功")]
        public bool animLoop;
        [LabelText("动画速度")]
        public float animSpeed = 1;

        [LabelText("动画时间"),PropertySpace]
        public float animTime = 0;

        [LabelText("动画长度")]
        public float maxAnimTime = 100;

        /// <summary>
        /// 如果动画是混合树，该参数则为混合树的1D参数名称
        /// </summary>
        [HideInInspector]
        public string blendParameterName;

        /// <summary>
        /// 如果动画是混合树,该参数为混合树的2D参数名称
        /// </summary>
        [HideInInspector]
        public string blendParameterYName;

        /// <summary>
        /// 如果动画是混合树，该参数则为混合树的1D参数值
        /// </summary>
        [HideInInspector]
        public float blendParameter;

        /// <summary>
        /// 如果动画是混合树，该参数则为混合树的2D参数值
        /// </summary>
        [HideInInspector]
        public float blendParameterY;
        public override BehaviourStatus OnUpdate()
        {
            if (animTime >= maxAnimTime)
            {
                OnAnimStop();
                if (animLoop)
                {
                    animTime = 0; 
                    return BehaviourStatus.Running;
                }
                return BehaviourStatus.Success;
            }
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            animTime += stateInfo.normalizedTime / 1f * 100f;

            return BehaviourStatus.Running;
        }
        public override void OnSuccess()
        {
            base.OnSuccess();
            OnAnimStop();
        }
        public override void OnFailed()
        {
            base.OnFailed();
            OnAnimStop();
        }
        /// <summary>
        /// 当动画播放完成时调用，如果是循环动画，则每次循环动画后播放
        /// </summary>
        protected virtual void OnAnimStop()
        {
            
        }

        internal static void SetBlendTreeParameter(AnimatorAction action, UnityEngine.Animator animator)
        {
            if (!animator) return;
            if (!action.blendParameterName.IsNullOrEmpty())
                animator.SetFloat(action.blendParameterName,action.blendParameter);

            if (!action.blendParameterYName.IsNullOrEmpty())
                animator.SetFloat(action.blendParameterYName, action.blendParameterY);
        }
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            if (!animatorController) return;
            if (animatorController is UnityEditor.Animations.AnimatorController controller)
            {               
                int hash = Animator.StringToHash(clipName);
                var stateMachine = controller.layers[layer].stateMachine;

                bool isBlendTree = false;
                UnityEditor.Animations.BlendTree blendTree = null;
                foreach (var state in stateMachine.states)
                {
                    if (state.state.motion is UnityEditor.Animations.BlendTree tree && state.state.nameHash == hash)
                    {
                        isBlendTree = true;
                        blendTree = tree;
                        break;
                    }
                }

                if (isBlendTree)
                {
                    blendParameterName = blendTree.blendParameter;
                    blendParameterYName = blendTree.blendParameterY;
                    blendParameter = UnityEditor.EditorGUILayout.FloatField(new GUIContent("混合参数" + blendTree.blendParameterY), blendParameter);
                    switch (blendTree.blendType)
                    {
                        case UnityEditor.Animations.BlendTreeType.Simple1D:
                            //1D不需要参数Y
                            blendParameterYName = string.Empty;
                            break;
                        case UnityEditor.Animations.BlendTreeType.SimpleDirectional2D:
                            blendParameterY =  UnityEditor.EditorGUILayout.FloatField(new GUIContent("混合参数Y" + blendTree.blendParameterY),blendParameterY);
                            break;
                        case UnityEditor.Animations.BlendTreeType.FreeformDirectional2D:
                            blendParameterY = UnityEditor.EditorGUILayout.FloatField(new GUIContent("混合参数Y" + blendTree.blendParameterY), blendParameterY);
                            break;
                        case UnityEditor.Animations.BlendTreeType.FreeformCartesian2D:
                            blendParameterY = UnityEditor.EditorGUILayout.FloatField(new GUIContent("混合参数Y" + blendTree.blendParameterY), blendParameterY);
                            break;
                    }
                }
                else
                {
                    blendParameterName = string.Empty;
                    blendParameterYName = string.Empty;
                    blendParameter = 0;
                    blendParameterY = 0;
                }

            }
        }
#endif
    }
}
