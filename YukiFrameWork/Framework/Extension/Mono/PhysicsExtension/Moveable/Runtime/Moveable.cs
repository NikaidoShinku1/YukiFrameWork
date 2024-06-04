using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace YukiFrameWork.Platform2D
{

    internal struct PositionInfo
    {
        public float percent;
        public Vector3 position;

        public PositionInfo(float percent, Vector3 position)
        {
            this.percent = percent;
            this.position = position;
        }

    }

    public enum PositionType
    {
        [LabelText("使用坐标点作为路径")]
        Positions,
        [LabelText("使用transform作为路径")]
        Transforms
    }

    public enum MoveType
    { 
        [LabelText("在起点和终点之间来回移动")]
        PingPong, 
        [LabelText("循环移动")]
        Loop, 
        [LabelText("移动到终点之后自动停止")]
        Once
    }

    public enum UpdateMode 
    {
        [LabelText("正常模式")]
        Normal,
        [LabelText("忽略时间缩放")]
        UnscaledTime
    }

    public class Moveable : MonoBehaviour
    {
        #region 字段 

        [SerializeField]
        [InfoBox("移动速度")]
        private float speed = 4;

        [SerializeField]
        [InfoBox("更新模式,Normal:普通模式 UnscaledTime:忽略时间缩放")]
        private UpdateMode updateMode = UpdateMode.Normal;

        [SerializeField]
        [InfoBox("移动类型")]
        private MoveType moveType;
         
        [SerializeField]
        [InfoBox("路径类型,Positions:使用坐标点作为路径 Transforms:使用transform作为路径")]
        private PositionType positionType;

        [SerializeField,ShowIf(nameof(positionType),PositionType.Transforms)]
        private List<Transform> transforms = new List<Transform>();
        [SerializeField,ShowIf(nameof(positionType),PositionType.Positions)]
        private List<Vector3> positions = new List<Vector3>();
         
        private List<PositionInfo> paths = new List<PositionInfo>();

        private float timer;

        [SerializeField]
        [InfoBox("延迟时间")]
        private float delay = 0;
        private float delayTimer = 0;

        [SerializeField]
        [InfoBox("是否自动运行?")]
        private bool runOnAwake = true;

        private bool isRunning = false;

        #endregion

        #region 属性
         
        /// <summary>
        /// 移动速度
        /// </summary>
        public float Speed 
        {
            get { 
                return speed;
            }
            set { 
                speed = value;
            }
        }

        /// <summary>
        /// 移动类型
        /// </summary>
        public MoveType MoveType
        {
            get {
                return moveType;
            }
            set {
                moveType = value; 
            }
        }

        /// <summary>
        /// 路径点
        /// </summary>
        public List<Transform> Transforms => transforms;
        /// <summary>
        /// 路径点
        /// </summary>
        public List<Vector3> Positions => positions;

        /// <summary>
        /// 延迟时间
        /// </summary>
        public float Delay
        {
            get {
                return delay;
            }
            set { 
                delay = value;
            }
        }
         
        /// <summary>
        /// 是否正在移动中
        /// </summary>
        public bool IsRunning => isRunning;

        /// <summary>
        /// 路径总长度
        /// </summary>
        public float Distance 
        { 
            get 
            { 
                float distance = 0;

                if (positionType == PositionType.Positions)
                { 
                    for (int i = 0; i < positions.Count - 1; i++)
                    {
                        Vector3 from = positions[i];
                        Vector3 to = positions[i + 1];
                        distance += Vector3.Distance(from, to);
                    } 
                }
                else
                {
                    for (int i = 0; i < transforms.Count - 1; i++)
                    {
                        Vector3 from = transforms[i].position;
                        Vector3 to = transforms[i + 1].position;
                        distance += Vector3.Distance(from, to);
                    } 
                }

                return distance;
            }
        }
        
        /// <summary>
        /// 从起点移动到终点所需要的时间
        /// </summary>
        public float Time => Distance / speed;

         
        #endregion

        #region 事件

        [Header("开始移动时触发")]
        [InfoBox("开始移动时触发"),FoldoutGroup("事件")]
        public UnityEvent onStartRun;

        [Header("移动时触发")]
        [InfoBox("移动时触发"),FoldoutGroup("事件")]
        public Vector3UnityEvent onMove;

        [Header("暂停时或取消暂停时触发")]
        [InfoBox("暂停时或取消暂停时触发"),FoldoutGroup("事件")]
        public BoolUnityEvent onPause;

        [Header("结束移动时触发")]
        [InfoBox("结束移动时触发"),FoldoutGroup("事件")]
        public UnityEvent onStop;

        #endregion

        #region 生命周期

        protected virtual void Awake()
        {
            if (runOnAwake)
                Run();
        }

        protected virtual void FixedUpdate()
        {
            if (!isRunning) return;

            if (delayTimer >= delay)
                Move();
            else {

                switch (updateMode)
                {
                    case UpdateMode.Normal:
                        delayTimer += UnityEngine.Time.fixedDeltaTime;
                        break;
                    case UpdateMode.UnscaledTime:
                        delayTimer += UnityEngine.Time.fixedUnscaledDeltaTime;
                        break; 
                }

                
            }
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            switch (positionType)
            {
                case PositionType.Positions:

                    for (int i = 0; i < positions.Count - 1; i++)
                    {
                        Handles.color = Color.yellow;
                        Handles.DrawDottedLine(positions[i], positions[i + 1], 4);
                    }

                    break;
                case PositionType.Transforms:

                    for (int i = 0; i < transforms.Count - 1; i++)
                    {
                        if(transforms[i] == null || transforms[i + 1] == null)
                            continue;

                        Handles.color = Color.yellow;
                        Handles.DrawDottedLine(transforms[i].position, transforms[i + 1].position, 4);
                    }

                    break; 
            }
        }
         
       
#endif

        #endregion

        #region 方法

        private void Init()
        {
            if (speed <= 0)
                throw new System.Exception("速度必须大于0!");
             
            paths.Clear();

            if (positionType == PositionType.Positions)
            { 
                for (int i = 0; i < positions.Count; i++)
                {
                    Vector3 current = positions[i];

                    if (i == 0)
                    {
                        paths.Add(new PositionInfo(0, current));
                        continue;
                    }

                    Vector3 last = positions[i - 1]; 
                    float percent = Vector3.Distance(current, last) / Distance; 
                    PositionInfo info = new PositionInfo(paths[paths.Count - 1].percent + percent, current);
                    paths.Add(info); 
                }

            }
            else
            { 
                for (int i = 0; i < transforms.Count; i++)
                {
                    Vector3 current = transforms[i].position;

                    if (i == 0)
                    {
                        paths.Add(new PositionInfo(0, current));
                        continue;
                    }

                    Vector3 last = transforms[i - 1].position;
                    float percent = Vector3.Distance(current, last) / Distance;
                    PositionInfo info = new PositionInfo(paths[paths.Count - 1].percent + percent, current);
                    paths.Add(info);
                }

            }
             
        }


        private void Move()
        {
            if (Time <= 0) return;

            // 向目标位置移动
            switch (updateMode)
            {
                case UpdateMode.Normal:
                    timer += UnityEngine.Time.fixedDeltaTime;
                    break;
                case UpdateMode.UnscaledTime: 
                    timer += UnityEngine.Time.fixedUnscaledDeltaTime;
                    break;
            }
              
            float t = 0;

            switch (moveType)
            {
                case MoveType.PingPong:
                    t = Mathf.PingPong(timer / Time, 1);
                    break;
                case MoveType.Loop:
                    t = timer / Time % 1;
                    break;
                case MoveType.Once:
                    t = timer / Time;
                    break;
            }

            Vector3 target = GetPosition(t); 
            onMove?.Invoke(target); 
            Move(target); 
            if (moveType == MoveType.Once && t >= 1)  
                Stop(); 
        }

        /// <summary>
        /// 移动时触发
        /// </summary>
        /// <param name="target"></param>
        protected virtual void Move(Vector3 target)
        { 
            transform.position = target; 
        }

        /// <summary>
        /// 开始移动
        /// </summary>
        public void Run()
        {
            // 正在运行中 不需要再次运行
            if (isRunning) 
                return;

            timer = 0;
            delayTimer = 0;
            Init();
            isRunning = true;
            onStartRun?.Invoke();
        }

        /// <summary>
        /// 暂停移动
        /// </summary>
        /// <param name="pause"></param>
        public void Pause(bool pause)
        {  
            isRunning = !pause;
            onPause?.Invoke(pause);
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void Stop()
        { 
            isRunning = false;
            delayTimer = 0;
            timer = 0;
            onStop?.Invoke();
        }

        /// <summary>
        /// 添加一个路径点
        /// </summary>
        /// <param name="position"></param>
        public void AddPosition(Vector3 position)
        { 
            positions.Add(position);
            Init();
        }

        /// <summary>
        /// 添加一个路径点
        /// </summary>
        /// <param name="transform"></param>
        public void AddPosition(Transform transform)
        {
            transforms.Add(transform);
            Init();
        }

        /// <summary>
        /// 移除一个路径点
        /// </summary>
        /// <param name="index">路径的下标</param>
        public void RemovePosition(int index)
        {
            if (positionType == PositionType.Positions)
            {
                if(index >= 0 && index < positions.Count)
                    positions.RemoveAt(index);
            }
            else
            {
                if (index >= 0 && index < transforms.Count)
                    transforms.RemoveAt(index);
            }

            Init();
        }

        /// <summary>
        /// 清空所有的路径点
        /// </summary>
        public void ClearPosition() 
        { 
            if (positionType == PositionType.Positions) 
                positions.Clear(); 
            else 
                transforms.Clear(); 

            Init();
        }

        private Vector3 GetPosition(float t) 
        { 
            
            for (int i = 0; i < paths.Count; i++) 
            {
                if (t < paths[i].percent)
                {
                    float p = paths[i].percent - paths[i - 1].percent; 
                    return Vector3.Lerp(paths[i-1].position, paths[i].position, (t - paths[i - 1].percent) / p);
                }    
            } 

            // 返回最后一个
            return paths[paths.Count - 1].position;
        }
         
        #endregion

    }

}

