///=====================================================
/// - FileName:      ColliderExtension.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   碰撞盒拓展
/// - Creation Time: 2024/7/13 21:14:27
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    using UnityEngine;

    public static class Collider2DExtensions
    {
        /// <summary>
        /// 获取2d碰撞盒所有的边顶点 处于测试阶段
        /// </summary>
        /// <param name="collider">盒子</param>
        /// <param name="position">对象的世界坐标</param>
        /// <returns></returns>
        public static Vector3[] GetColliderCorners2D(this Collider2D collider, Vector3 position)
        {
            if (collider is BoxCollider2D boxCollider)
            {
                return GetBoxColliderCorners2D(boxCollider, position);
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                return GetCircleColliderCorners2D(circleCollider, position);
            }
            else if (collider is CapsuleCollider2D capsuleCollider)
            {
                return GetCapsuleColliderCorners2D(capsuleCollider, position);
            }
            else if (collider is PolygonCollider2D polygonCollider)
            {
                return GetPolygonColliderCorners2D(polygonCollider, position);
            }
            else
            {
#if YukiFrameWork_DEBUGFULL
                LogKit.W("Unsupported collider type");
#endif
                return new Vector3[0];
            }
        }

        private static Vector3[] GetBoxColliderCorners2D(BoxCollider2D collider, Vector3 position)
        {
            Bounds bounds = collider.bounds;
            Vector3 halfSize = bounds.size / 2;

            Vector3[] corners = new Vector3[4];
            corners[0] = position + new Vector3(-halfSize.x, -halfSize.y, 0); 
            corners[1] = position + new Vector3(-halfSize.x, halfSize.y, 0);  
            corners[2] = position + new Vector3(halfSize.x, -halfSize.y, 0);  
            corners[3] = position + new Vector3(halfSize.x, halfSize.y, 0);  

            return corners;
        }

        private static Vector3[] GetCircleColliderCorners2D(CircleCollider2D collider, Vector3 position)
        {
            float radius = collider.radius * collider.transform.localScale.x;
            Vector3[] corners = new Vector3[4];
            corners[0] = position + new Vector3(-radius, -radius, 0); 
            corners[1] = position + new Vector3(-radius, radius, 0); 
            corners[2] = position + new Vector3(radius, -radius, 0);  
            corners[3] = position + new Vector3(radius, radius, 0);  

            return corners;
        }

        private static Vector3[] GetCapsuleColliderCorners2D(CapsuleCollider2D collider, Vector3 position)
        {
            Bounds bounds = collider.bounds;
            Vector3 halfSize = bounds.size / 2;

            Vector3[] corners = new Vector3[4];
            corners[0] = position + new Vector3(-halfSize.x, -halfSize.y, 0); 
            corners[1] = position + new Vector3(-halfSize.x, halfSize.y, 0);  
            corners[2] = position + new Vector3(halfSize.x, -halfSize.y, 0); 
            corners[3] = position + new Vector3(halfSize.x, halfSize.y, 0);  

            return corners;
        }

        private static Vector3[] GetPolygonColliderCorners2D(PolygonCollider2D collider, Vector3 position)
        {
            Vector3[] Corners = new Vector3[collider.points.Length];
            for (int i = 0; i < Corners.Length; i++)
            {
                Corners[i] = collider.transform.TransformPoint(collider.points[i]);
            }

            return Corners;
        }

        /// <summary>
        /// 获取碰撞盒所有的边顶点 处于测试阶段
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public static Vector3[] GetColliderCorners(this Collider collider)
        {
            switch (collider)
            {
                case BoxCollider box:
                    return GetBoxCorners(box);
                case SphereCollider sphere:
                    return GetSphereCorners(sphere);
                case CapsuleCollider capsule:
                    return GetCapsuleCorners(capsule);
                case MeshCollider mesh:
                    return GetMeshCorners(mesh);
                default:
#if YukiFrameWork_DEBUGFULL
                    LogKit.W("Collider type not supported.");
#endif
                    return new Vector3[0];
            }
        }

        private static Vector3[] GetBoxCorners(BoxCollider collider)
        {
            Vector3 center = collider.center;
            Vector3 size = collider.size / 2;

            Transform transform = collider.transform;
            Vector3[] Corners = new Vector3[8];
            Corners[0] = transform.TransformPoint(center + new Vector3(-size.x, -size.y, -size.z));
            Corners[1] = transform.TransformPoint(center + new Vector3(size.x, -size.y, -size.z));
            Corners[2] = transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z));
            Corners[3] = transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z));
            Corners[4] = transform.TransformPoint(center + new Vector3(-size.x, -size.y, size.z));
            Corners[5] = transform.TransformPoint(center + new Vector3(size.x, -size.y, size.z));
            Corners[6] = transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z));
            Corners[7] = transform.TransformPoint(center + new Vector3(size.x, size.y, size.z));
            return Corners;
        }

        private static Vector3[] GetSphereCorners(SphereCollider collider)
        {
            Vector3 center = collider.center;
            float radius = collider.radius;
            Transform transform = collider.transform;
            Vector3[] Corners = new Vector3[6];
            Corners[0] = transform.TransformPoint(center + new Vector3(radius, 0, 0));
            Corners[1] = transform.TransformPoint(center + new Vector3(-radius, 0, 0));
            Corners[2] = transform.TransformPoint(center + new Vector3(0, radius, 0));
            Corners[3] = transform.TransformPoint(center + new Vector3(0, -radius, 0));
            Corners[4] = transform.TransformPoint(center + new Vector3(0, 0, radius));
            Corners[5] = transform.TransformPoint(center + new Vector3(0, 0, -radius));
            return Corners;
        }

        private static Vector3[] GetCapsuleCorners(CapsuleCollider collider)
        {
            Vector3 center = collider.center;
            float height = collider.height / 2 - collider.radius;
            int direction = collider.direction; // 0=x, 1=y, 2=z
            Transform transform = collider.transform;
            Vector3 directionVector = Vector3.up * (direction == 1 ? height : 0) + Vector3.right * (direction == 0 ? height : 0) + Vector3.forward * (direction == 2 ? height : 0);
            Vector3[] Corners = new Vector3[2];
            Corners[0] = transform.TransformPoint(center + directionVector);
            Corners[1] = transform.TransformPoint(center - directionVector);
            return Corners;
        }

        private static Vector3[] GetMeshCorners(MeshCollider collider)
        {
            Bounds bounds = collider.bounds;
            Vector3[] Corners = new Vector3[2]; // This example simply returns bounds extremes, but can be extended.
            Corners[0] = bounds.min;
            Corners[1] = bounds.max;
            return Corners;
        }
    }

}
