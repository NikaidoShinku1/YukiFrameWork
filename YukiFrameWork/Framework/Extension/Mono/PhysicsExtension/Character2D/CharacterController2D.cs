///=====================================================
/// - FileName:      CharacherController2D.cs
/// - NameSpace:     YukiFrameWork.Physics.Character2D
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/4 17:41:19
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace YukiFrameWork.Character2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {

        #region 常量
        /// <summary>
        /// 这个值不能再小了,否则会影响检测!!!
        /// </summary>
        private const float DISTANCE = 0.1F;

        private const string DEFAULT = "默认设置";
        private const string GROUND = "检测地面设置";
        private const string WALL = "墙体设置";
        private const string TOP = "检测上方设置";
        #endregion


        #region 字段

        internal static RaycastHit2D[] hits = new RaycastHit2D[10];

        internal static ContactPoint2D[] contacts = new ContactPoint2D[10];

        private int contactCount;

        internal LayerMask checkLayerMask;

        private int currentLayer = 0;

        private Rigidbody2D _rigidbody2d;

        internal int checkCount; // 测试用
        private int lastSecond;

        internal Collider2D top = null;
        internal Collider2D bottom = null;
        internal Collider2D left = null;
        internal Collider2D right = null;

        [InfoBox("是否自动旋转?如果开启，当前角色会根据地面倾斜角度自动旋转,始终保持与地面垂直"),BoxGroup(DEFAULT)]
        public bool autoRotation = false;

        // 是否在空中 用于自动控制旋转角度
        private bool inTheAir = false;
#if UNITY_EDITOR
        [SerializeField]
        [InfoBox("是否绘制调试的线条?如果开启，可以在场景中看到接触点的法线，角度等信息"), BoxGroup(DEFAULT)]
        private bool showDebugLine = true;
#endif
        [InfoBox("是否检测角色当前是否在地面上(默认检测)"), BoxGroup(GROUND), PropertySpace(15)]
        [SerializeField]
        private bool isCheckGround = true;

        [InfoBox("是否检测角色当前是否接触墙面(默认不检测)"),BoxGroup(WALL), PropertySpace(15)]
        [SerializeField]
        private bool isCheckWall = false;

        [InfoBox(" 是否检测角色头顶是否碰到游戏物体(默认不检测)"),BoxGroup(TOP),PropertySpace(15)]
        [SerializeField]
        private bool isCheckTop = false;

        private Vector2 moveDelta;

        private Vector2 lastPosition;

        internal float speedVertical;

        [InfoBox("当前角色受到的重力大小(仅在开启检测地面时有效,如果未开启检测地面,当前角色不会受到重力影响!)"),BoxGroup(GROUND),ShowIf(nameof(isCheckGround))]
        public float gravityScale = 1;
        [InfoBox("下落的最大速度(默认:20)"), BoxGroup(GROUND), ShowIf(nameof(isCheckGround))]
        [Range(1, 100)]
        public float MaxFallingVelocity = 20;

        private float rigidbodyGravityScale = 0;

        private CheckGround checkGround;

        private CheckWall checkWall;

        private CheckTop checkTop;

        [InfoBox("地面倾斜角度,0代表水平无倾斜,如果为30,则是倾斜角度在30以内的地面,才会被认为是地面!"), BoxGroup(GROUND), ShowIf(nameof(isCheckGround))]
        [Range(1, 45)]
        public float groundAngle = 35;

        [InfoBox("墙面倾斜角度,0代表竖直向上,如果为30,则是竖直向上往左或往右偏移30度以内认为是墙面!")]
        [Range(1, 45),BoxGroup(WALL),ShowIf(nameof(isCheckWall))]
        public float wallAngle = 20;


        [InfoBox("角色头顶碰到的游戏物体的倾斜角度,在该角度范围内，认为碰到游戏物体!"),BoxGroup(TOP),ShowIf(nameof(isCheckTop))]
        [Range(1, 60)]
        public float topAngle = 45;

        private bool isGrounded;

        private bool isTouchWall;

        private bool isTop;

        private Vector2 velocity;

        [InfoBox("角色最多连续跳跃次数"), BoxGroup(DEFAULT)]
        public int MaxJumpCount = 1;

        private int currentJumpCount = 0;

        private bool jumping = false;
        private float jumpAbortSpeedReduction = 0;
        #endregion

        #region 属性

        /// <summary>
        /// 是否在地面上
        /// </summary>
        public bool IsGrounded
        {
            get
            {
                if (speedVertical > 0)
                {
                    isGrounded = false;
                    return false;
                }

                return isGrounded;
            }
            internal protected set
            {
                if (isGrounded != value)
                    onGroundValueChange?.Invoke(value);

                isGrounded = value;
            }
        }

        /// <summary>
        /// 与地面的接触信息(比如 接触点 法线 等)
        /// </summary>
        public RaycastHit2D GroundInfo { get; internal protected set; }

        /// <summary>
        /// 是否接触到墙面
        /// </summary>
        public bool IsTouchWall
        {
            get
            {
                return isTouchWall;
            }
            internal protected set
            {
                if (isTouchWall != value)
                    onTouchWallValueChange?.Invoke(value);
                isTouchWall = value;
            }
        }

        /// <summary>
        /// 与墙面的接触信息(比如 接触点 法线 等)
        /// </summary>
        public RaycastHit2D WallInfo { get; internal protected set; }

        /// <summary>
        /// 头顶是否接触到游戏物体
        /// </summary>
        public bool IsTop
        {
            get
            {
                return isTop;
            }
            internal protected set
            {

                if (isTop != value)
                    onTopValueChange?.Invoke(value);

                isTop = value;

            }
        }

        /// <summary>
        /// 与顶部的接触信息(比如 接触点 法线 等)
        /// </summary>
        public RaycastHit2D TopInfo { get; internal protected set; }


        /// <summary>
        /// 移动速度
        /// </summary>
        public Vector2 Velocity
        {
            get
            {
                return velocity;
            }
            internal protected set
            {
                if (velocity != value)
                    onVelocityValueChange?.Invoke(value);
                velocity = value;
            }
        }

        public Rigidbody2D Rigidbody
        {
            get
            {
                if (_rigidbody2d == null)
                    _rigidbody2d = GetComponent<Rigidbody2D>();
                return _rigidbody2d;
            }
        }


        /// <summary>
        /// 是否检测角色当前是否在地面上(默认检测)
        /// </summary>
        public bool IsCheckGround
        {
            get
            {
                return isCheckGround;
            }
            set
            {
                isCheckGround = value;
                if (isCheckGround == false)
                {
                    IsGrounded = false;
                    ResetSpeedVertical();
                    Rigidbody.gravityScale = rigidbodyGravityScale;
                }
            }
        }

        /// <summary>
        /// 是否检测角色当前是否接触墙面(默认不检测)
        /// </summary>  
        public bool IsCheckWall
        {
            get
            {
                return isCheckWall;
            }
            set
            {
                isCheckWall = value;
                if (isCheckWall == false)
                {
                    IsTouchWall = false;
                }
            }
        }

        /// <summary>
        /// 是否检测角色头顶是否碰到游戏物体(默认不检测)
        /// </summary> 
        public bool IsCheckTop
        {
            get
            {
                return isCheckTop;
            }
            set
            {
                isCheckTop = value;
                if (isCheckTop == false)
                {
                    IsTop = false;
                }
            }
        }

        /// <summary>
        /// 当前已经跳跃的数量
        /// </summary>
        public int CurrentJumpCount
        {
            get
            {
                return currentJumpCount;
            }

            set
            {
                if (currentJumpCount != value)
                    onJumpCountValueChange?.Invoke(value);
                currentJumpCount = value;
            }

        }

        #endregion


        #region 事件

        [InfoBox("是否在地面的值发生改变时触发")]
        [BoxGroup(GROUND), ShowIf(nameof(isCheckGround))]
        public UnityEvent<bool> onGroundValueChange;
        [InfoBox("是否接触墙面的值发生改变时触发")]
        [BoxGroup(WALL), ShowIf(nameof(isCheckWall))]
        public UnityEvent<bool> onTouchWallValueChange;
        [InfoBox("头顶是否接触游戏物体的值发生改变时触发"), BoxGroup(TOP), ShowIf(nameof(isCheckTop))]   
        public UnityEvent<bool> onTopValueChange;
        [InfoBox("移动速度改变时触发")]
        public UnityEvent<Vector2> onVelocityValueChange;
        [InfoBox("已跳跃的数量发生改变时触发")]
        public UnityEvent<int> onJumpCountValueChange;

#if UNITY_EDITOR
        [OnInspectorGUI,FoldoutGroup("infos")]
        void DrawResult()
        {
            GUI.color = Color.gray;

            EditorGUILayout.LabelField("IsGrounded:",IsGrounded.ToString());
            EditorGUILayout.LabelField("IsTouchWall:",IsTouchWall.ToString());
            EditorGUILayout.LabelField("IsTop:",ToString());
            EditorGUILayout.LabelField("Velocity:",Velocity.ToString());

            EditorGUILayout.LabelField("CurrentJumpCount:",CurrentJumpCount.ToString());     
            EditorGUILayout.LabelField("每秒射线检测次数:", checkCount.ToString());
        }
#endif

#endregion

        #region 生命周期

        private void Awake()
        {
            if (Rigidbody == null)
                throw new System.Exception("未查询到刚体组件!");
            rigidbodyGravityScale = Rigidbody.gravityScale;
            InitCollider();

            currentLayer = gameObject.layer;
            checkLayerMask.Physics2DSetting(gameObject.layer);
        }

        private void Reset()
        {
            if (Rigidbody == null)
                throw new System.Exception("未查询到刚体组件!");

            gravityScale = Rigidbody.gravityScale;
        }

        private void FixedUpdate()
        {
            UpdateSpeedVertical();
            Move();
            JumpHeld();
        }

        private void OnEnable()
        {
            lastPosition = Rigidbody.position;
        }

        private void OnDisable()
        {
            Rigidbody.gravityScale = rigidbodyGravityScale;
        }

        private void Update()
        {
            if (autoRotation)
                // 修改当前人物旋转角度 
                AutoRotation();

            if (currentLayer != gameObject.layer)
            {
                checkLayerMask.Physics2DSetting(gameObject.layer);
                currentLayer = gameObject.layer;
            }

            PhysicsCheck();

#if UNITY_EDITOR  
            DrawDebugLine();
#endif

        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!showDebugLine) return;
            if (!Application.isPlaying) return;

            if (IsGrounded)
            {
                Handles.color = Color.yellow;
                float angle = Vector2.Angle(GroundInfo.normal, Vector2.up);
                Handles.Label(GroundInfo.point, string.Format("地面角度:{0}", angle));
            }


            if (IsTouchWall)
            {
                Handles.color = Color.red;
                float angle = Vector2.Angle(WallInfo.normal, WallInfo.normal.x > 0 ? Vector2.right : Vector2.left);
                Handles.Label(WallInfo.point, string.Format("墙面角度:{0}", angle));
            }

            if (IsTop)
            {
                Handles.color = Color.green;
                float angle = Vector2.Angle(TopInfo.normal, Vector2.down);
                Handles.Label(TopInfo.point, string.Format("头顶角度:{0}", angle));
            }
        }

#endif

        #endregion


        #region 方法

        /// <summary>
        /// 移动(请在FixedUpdate中调用)
        /// </summary>
        /// <param name="delta">移动的增量</param>
        public void Move(Vector2 delta)
        {
            if (IsIgnoreMoveX(delta))
                delta.x = 0;

            moveDelta += delta;
        }


        /// <summary>
        /// 水平方向移动(请在FixedUpdate中调用)
        /// </summary>
        /// <param name="speed"></param>
        public void MoveHorizontal(float speed)
        {
            if (IsGrounded && IsCheckGround)
            {
                Vector3 dir = Vector3.Cross(GroundInfo.normal, Vector3.forward);
                Move(dir.normalized * speed * Time.fixedDeltaTime);
            }
            else
            {
                Move(Vector2.right * speed * Time.fixedDeltaTime);
            }
        }

        private void Move()
        {
            if (Mathf.Abs(moveDelta.x) <= 0.01f && Mathf.Abs(moveDelta.y) <= 0.01f)
            {
                Velocity = Vector3.zero;
                return;
            }

            // 移动到目标位置
            Rigidbody.MovePosition(Rigidbody.position + moveDelta);

            Velocity = (Rigidbody.position - lastPosition) / Time.fixedDeltaTime;

            lastPosition = Rigidbody.position;
            moveDelta = Vector2.zero;

            if (IsCheckTop)
            {
                if (IsTop && speedVertical > 0)
                    ResetSpeedVertical();
            }
            else
            {
                if (checkTop == null)
                    checkTop = new CheckTop();

                int count = Rigidbody.GetContacts(contacts);
                checkTop.Check(contacts, this, count, topAngle);

                if (checkTop.IsTopByContact && speedVertical > 0)
                    ResetSpeedVertical();
            }


        }

        /// <summary>
        /// 跳跃
        /// </summary>
        /// <param name="speed">跳跃初速度,值越大跳的越高</param>
        public void Jump(float speed)
        {
            if (!IsCheckGround)
            {
                Debug.LogWarning("是否检测地面的选项未开启，无法跳跃!");
                return;
            }

            if (CurrentJumpCount >= MaxJumpCount)
                return;
            CurrentJumpCount++;

            speedVertical = speed;
            if (speedVertical > 0)
                IsGrounded = false;

            // 重置速度
            Velocity = Vector2.zero;
            moveDelta = Vector2.zero;
            lastPosition = Rigidbody.position;
        }

        /// <summary>
        /// 跳跃(可提前中断,中断后跳跃高度会降低,适用于长按跳的更高的情况)
        /// </summary>
        /// <param name="speed">跳跃初速度,值越大跳的越高 </param>
        /// <param name="jumpAbortSpeedReduction">跳跃中断后的减速度, 值越大减速越快,跳的越低</param>
        public void JumpStart(float speed, float jumpAbortSpeedReduction)
        {
            if (CurrentJumpCount >= MaxJumpCount)
                return;

            Jump(speed);
            jumping = true;

            if (jumpAbortSpeedReduction < 0)
                jumpAbortSpeedReduction = 0;
            this.jumpAbortSpeedReduction = jumpAbortSpeedReduction;
        }

        private void JumpHeld()
        {
            if (jumpAbortSpeedReduction == 0)
                return;

            if (!jumping && speedVertical > 0)
                speedVertical -= jumpAbortSpeedReduction * Time.fixedDeltaTime;

            if (speedVertical <= 0)
            {
                jumpAbortSpeedReduction = 0;
                jumping = false;
            }

        }

        /// <summary>
        /// 中断跳跃
        /// </summary>
        public void JumpEnd()
        {
            jumping = false;
        }

        /// <summary>
        /// 重置竖直方向速度
        /// </summary>
        public void ResetSpeedVertical()
        {
            speedVertical = 0;
        }

        private void UpdateSpeedVertical()
        {
            // 如果不检测地面 则不需要控制竖直方向速度
            if (!IsCheckGround) return;

            if (checkGround != null && checkGround.IsGroundByContacts && speedVertical <= 0)
            {
                speedVertical = 0;
                Rigidbody.gravityScale = rigidbodyGravityScale;
            }
            else
            {
                Rigidbody.gravityScale = 0;

                // 如果下方有碰撞体 没有必要往下落 
                if (checkGround != null)
                {
                    contactCount = Rigidbody.GetContacts(contacts);
                    if (checkGround.IsHaveColliderOnBelow(contacts, contactCount) && speedVertical < 0)
                        return;
                }

                speedVertical += Physics2D.gravity.y * gravityScale * Time.fixedDeltaTime;

                if (speedVertical < -Mathf.Abs(MaxFallingVelocity))
                    speedVertical = -Mathf.Abs(MaxFallingVelocity);

                // 竖直方向的移动
                Move(Vector2.up * speedVertical * Time.fixedDeltaTime);
            }
        }

        private void InitCollider()
        {
            List<Collider2D> colliders = new List<Collider2D>();
            Rigidbody.GetAttachedColliders(colliders);

            for (int i = 0; i < colliders.Count; i++)
            {
                Collider2D collider = colliders[i];
                if (collider == null) continue;
                if (collider.isTrigger) continue;

                bool iscollider2d = !(collider is BoxCollider2D);
                bool iscirclecollider2d = !(collider is CircleCollider2D);
                bool iscapsulecollider2d = !(collider is CapsuleCollider2D);

                if (iscollider2d && iscirclecollider2d && iscapsulecollider2d)
                    continue;

                if (top == null)
                    top = collider;
                else
                {
                    if (collider.bounds.max.y > top.bounds.max.y)
                        top = collider;
                }

                if (bottom == null)
                    bottom = collider;
                else
                {
                    if (collider.bounds.min.y < bottom.bounds.min.y)
                        bottom = collider;
                }

                if (right == null)
                    right = collider;
                else
                {
                    if (collider.bounds.max.x > right.bounds.max.x)
                        right = collider;
                }

                if (left == null)
                    left = collider;
                else
                {
                    if (collider.bounds.min.x < left.bounds.min.x)
                        left = collider;
                }


            }

            if (left == null || right == null || bottom == null || top == null)
                throw new System.Exception("查询碰撞体失败!");

        }

        private void PhysicsCheck()
        {
            // 获取与当前刚体接触的游戏物体 

            if (IsCheckGround || IsCheckWall || IsCheckTop)
                contactCount = Rigidbody.GetContacts(contacts);


            if (IsCheckGround)
            {
                if (checkGround == null)
                    checkGround = new CheckGround();

                checkGround.Check(contacts, this, contactCount, groundAngle);

                if (IsGrounded)
                    CurrentJumpCount = 0;
            }


            if (IsCheckWall)
            {
                if (checkWall == null)
                    checkWall = new CheckWall();

                checkWall.Check(contacts, this, contactCount, wallAngle);
            }

            if (IsCheckTop)
            {
                if (checkTop == null)
                    checkTop = new CheckTop();

                checkTop.Check(contacts, this, contactCount, topAngle);
            }

#if UNITY_EDITOR

            //for (int i = 0; i < contactCount; i++)
            //{
            //    Debug.DrawLine(contacts[i].point, contacts[i].point + contacts[i].normal, Color.black);
            //}

            if (Mathf.FloorToInt(Time.time) != lastSecond)
            {
                //Debug.LogFormat("每秒检测数量:{0}",checkCount);
                lastSecond = Mathf.FloorToInt(Time.time);
                checkCount = 0;
            }
#endif
        }

        internal int Raycast(CircleCollider2D circle, Vector2 dir)
        {

            Vector2 point = circle.bounds.center;

            float r = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);

            return Physics2D.CircleCastNonAlloc(point, r, dir, hits, DISTANCE, checkLayerMask.value);
        }

        internal int Raycast(BoxCollider2D box, Vector2 dir)
        {
            Vector2 point = box.bounds.center;
            Vector2 size = box.bounds.size;
            float angle = box.transform.eulerAngles.z;

            return Physics2D.BoxCastNonAlloc(point, size, angle, dir, hits, DISTANCE, checkLayerMask.value);
        }

        internal int Raycast(CapsuleCollider2D capsule, Vector2 dir)
        {
            Vector2 point = capsule.bounds.center;
            Vector2 size = capsule.bounds.size;
            float angle = capsule.transform.eulerAngles.z;
            CapsuleDirection2D d = capsule.direction;


            return Physics2D.CapsuleCastNonAlloc(point, size, d, angle, dir, hits, DISTANCE, checkLayerMask.value);
        }

        private void AutoRotation()
        {

            if (!IsCheckGround)
                return;

            if (IsGrounded)
            {
                inTheAir = false;
                float angle = -Vector2.SignedAngle(GroundInfo.normal, Vector2.up);

                if (transform.right.x < 0)
                    angle = -angle;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle);
            }
            else
            {
                if (!inTheAir)
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
                inTheAir = true;
            }

        }

        private bool IsIgnoreMoveX(Vector2 delta)
        {
            if (!IsCheckWall) return false;

            if (!IsCheckGround) return false;

            if (IsTouchWall && !IsGrounded)
            {
                // 判断法线方向和移动方向是否相反 不能往墙上移动
                if ((delta.x > 0 && WallInfo.normal.x < 0) || (delta.x < 0 && WallInfo.normal.x > 0))
                {
                    return true;
                }
            }

            if (!IsTouchWall && !IsGrounded)
            {
                int count = Rigidbody.GetContacts(contacts);
                for (int i = 0; i < count; i++)
                {
                    if (contacts[i].collider == null)
                        continue;

                    if (contacts[i].collider.isTrigger)
                        continue;

                    if ((delta.x > 0 && contacts[i].normal.x < 0) || (delta.x < 0 && contacts[i].normal.x > 0))
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        private void DrawDebugLine()
        {
#if UNITY_EDITOR
            if (!showDebugLine) return;

            if (IsGrounded)
                Debug.DrawLine(GroundInfo.point, GroundInfo.point + GroundInfo.normal * 0.2f, Color.yellow);


            if (IsTouchWall)
                Debug.DrawLine(WallInfo.point, WallInfo.point + WallInfo.normal * 0.2f, Color.red);

            if (IsTop)
                Debug.DrawLine(TopInfo.point, TopInfo.point + TopInfo.normal * 0.2f, Color.green);
#endif

        }




        #endregion

    }
}


