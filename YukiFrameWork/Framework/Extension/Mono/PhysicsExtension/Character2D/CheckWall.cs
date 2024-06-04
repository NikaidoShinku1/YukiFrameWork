///=====================================================
/// - FileName:      CheckWall.cs
/// - NameSpace:     YukiFrameWork.Physics.Character2D
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/4 18:10:42
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================

using UnityEngine;
namespace YukiFrameWork.Character2D
{

    public class CheckWall
    {
        private ContactPoint2D[] contacts = null;

        private CharacterController2D characher;

        private float wallAngle = 20;

        private float lastCheckTime;

        private float lastUpdateWallInfoTime;

        private float timeInterval = 1.0f / 10.0f; // 每秒最多检测30次

        private int contactsCount;

        internal void Check(ContactPoint2D[] contacts, CharacterController2D characher, int count, float wallAngle)
        {
            this.contacts = contacts;
            this.characher = characher;
            this.wallAngle = wallAngle;

            this.contactsCount = count;

            int wallIndex = 0;

            bool isWallByContacts = IsWallByContacts(count, out wallIndex);

            // 检测墙面
            if (isWallByContacts != characher.IsTouchWall)
                CheckWallTouch();

            if (isWallByContacts)
                UpdateWallInfo(wallIndex);
        }

        private bool IsWallByContacts(int count, out int index)
        {
            bool isWall = false;
            float min_angle = 90;
            int min_index = 0;

            for (int i = 0; i < count; i++)
            {
                ContactPoint2D raycast = contacts[i];
                if (raycast.collider == null)
                    continue;
                if (raycast.collider.isTrigger)
                    continue;

                if (raycast.collider is BoxCollider2D || raycast.collider is CapsuleCollider2D)
                {
                    if (Mathf.Abs(Vector2.Dot(raycast.normal, raycast.collider.transform.up)) > 0.01f)
                    {
                        continue;
                    }
                }

                float angle = Vector2.Angle(raycast.normal, raycast.normal.x < 0 ? -Vector2.right : Vector2.right);

                if (angle < min_angle)
                {
                    min_angle = angle;
                    min_index = i;
                }
            }


            if (min_angle < wallAngle)
            {
                isWall = true;
            }

            index = min_index;
            return isWall;
        }

        private void UpdateWallInfo(int index)
        {
            if (!characher.IsTouchWall)
                return;

            if (Time.time - lastUpdateWallInfoTime < 0.1f)
                return;

            for (int i = 0; i < contactsCount; i++)
            {
                // 只要有一个点 和 当前法线比较接近 就不需要更新
                bool d = Vector3.Distance(contacts[i].point, characher.WallInfo.point) < 0.1f;
                bool a = Vector3.Angle(contacts[i].normal, characher.WallInfo.normal) < 3;
                // 认为法线区别不大 不需要更新
                if (d && a)
                    return;
            }

            CheckWallTouch();


        }


        private void CheckWallTouch()
        {
            if (!characher.IsCheckWall) return;

            if (Time.time - lastCheckTime < timeInterval)
                return;

            lastCheckTime = Time.time;

#if UNITY_EDITOR
            characher.checkCount++;
#endif

            Vector2 dir = characher.right.transform.right;

            // 如果缩放值小于0 翻转方向
            if (characher.right.transform.lossyScale.x < 0)
                dir = -dir;

            int count = 0;

            if (characher.right is CircleCollider2D)
            {
                count = characher.Raycast(characher.right as CircleCollider2D, dir);
            }
            else if (characher.right is BoxCollider2D)
            {
                count = characher.Raycast(characher.right as BoxCollider2D, dir);

            }
            else if (characher.right is CapsuleCollider2D)
            {
                count = characher.Raycast(characher.right as CapsuleCollider2D, dir);
            }

            CaculateIsWallTouch(count);

            if (characher.IsTouchWall) return;

            dir = -characher.left.transform.right;

            // 如果缩放值小于0 翻转方向
            if (characher.left.transform.lossyScale.x < 0)
                dir = -dir;

            if (characher.left is CircleCollider2D)
            {
                count = characher.Raycast(characher.left as CircleCollider2D, dir);
            }
            else if (characher.left is BoxCollider2D)
            {
                count = characher.Raycast(characher.left as BoxCollider2D, dir);
            }
            else if (characher.left is CapsuleCollider2D)
            {
                count = characher.Raycast(characher.left as CapsuleCollider2D, dir);
            }

            CaculateIsWallTouch(count);
        }

        private void CaculateIsWallTouch(int count)
        {
            characher.IsTouchWall = false;

            for (int i = 0; i < count; i++)
            {
                RaycastHit2D raycast = CharacterController2D.hits[i];
                if (raycast.collider == null)
                    continue;
                if (raycast.collider.isTrigger)
                    continue;

                if (raycast.transform == characher.transform || raycast.transform.IsChildOf(characher.transform))
                    continue;

                if (raycast.collider is BoxCollider2D || raycast.collider is CapsuleCollider2D)
                {
                    if (Mathf.Abs(Vector2.Dot(raycast.normal, raycast.transform.up)) > 0.01f)
                    {
                        continue;
                    }
                }

                float angle = Vector2.Angle(raycast.normal, raycast.normal.x < 0 ? -Vector2.right : Vector2.right);

                if (angle < wallAngle)
                {
                    characher.IsTouchWall = true;
                    characher.WallInfo = raycast;
                    break;
                }
            }


        }

    }
}

