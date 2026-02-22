using UnityEngine;
using UnityEngine.AI;

namespace YukiFrameWork.AI
{
    /// <summary>
    /// AI服务接口,使用该接口默认关闭NavMeshAgent的对象自动跟随路径以及自动旋转，控制权交由对象本身
    /// </summary>
    public interface IAIServices
    {
        /// <summary>
        /// 寻路网格
        /// </summary>
        NavMeshAgent Agent { get; }

        /// <summary>
        /// 终点位置
        /// </summary>
        Vector3 EndPos { get; }

        /// <summary>
        /// 当服务被注册时触发
        /// </summary>
        void ServicesInit();
        
        /// <summary>
        /// 当开始运行
        /// </summary>
        void Enable();

        /// <summary>
        /// 当结束运行
        /// </summary>
        void Disable();

        /// <summary>
        /// 当手动同步Agent后IsOnNavMesh为False时触发
        /// <para>触发时机:在通过NavMeshKit进行动态烘焙或清空烘焙时触发</para>
        /// </summary>
        void NavMeshClean();

        /// <summary>
        /// 当手动同步Agent后IsOnNavMesh为true时触发
        /// </summary>
        void NavMeshSync();

        /// <summary>
        /// 当服务启动后持续更新
        /// </summary>
        /// <param name="direction">寻路数据给出的下一个方向</param>
        void NavMeshUpdate(Vector3 direction);
        /// <summary>
        /// 当服务启动后持续更新
        /// </summary>
        /// <param name="direction">寻路数据给出的下一个方向</param>
        void NavMeshFixedUpdate(Vector3 direction);
        /// <summary>
        /// 当服务启动后持续更新
        /// </summary>
        /// <param name="direction">寻路数据给出的下一个方向</param>
        void NavMeshLateUpdate(Vector3 direction);
    }
}