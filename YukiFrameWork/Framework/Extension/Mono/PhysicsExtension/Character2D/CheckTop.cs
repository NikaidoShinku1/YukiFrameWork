///=====================================================
/// - FileName:      CheckTop.cs
/// - NameSpace:     YukiFrameWork.Physics.Character2D
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/4 18:09:46
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace YukiFrameWork.Character2D
{
    public class CheckTop
    {

        private ContactPoint2D[] contacts = null;

        private CharacterController2D characher;

        private float topAngle = 45;

        private float lastCheckTime;

        private float lastUpdateTopInfoTime;

        private float timeInterval = 1.0f / 10.0f; // 每秒最多检测10次

        private int contactsCount;

        public bool IsTopByContact { get; set; }

        internal void Check(ContactPoint2D[] contacts, CharacterController2D characher, int count, float topAngle)
        {
            this.contacts = contacts;
            this.characher = characher;
            this.topAngle = topAngle;
            this.contactsCount = count;
            // 检测地面

            // 检测头顶

            IsTopByContact = IsTopByContacts(count);

            if (IsTopByContact != characher.IsTop)
                CheckTopInfo();

            if (IsTopByContact)
                UpdateTopInfo();
        }

        private void CheckTopInfo()
        {
            if (!characher.IsCheckTop) return;

            if (Time.time - lastCheckTime < timeInterval)
                return;

            lastCheckTime = Time.time;

#if UNITY_EDITOR
            characher.checkCount++;
#endif

            Vector2 dir = characher.top.transform.up;

            int count = 0;

            if (characher.top is CircleCollider2D)
            {
                count = characher.Raycast(characher.top as CircleCollider2D, dir);
            }
            else if (characher.top is BoxCollider2D)
            {
                count = characher.Raycast(characher.top as BoxCollider2D, dir);
            }
            else if (characher.top is CapsuleCollider2D)
            {
                count = characher.Raycast(characher.top as CapsuleCollider2D, dir);
            }

            CalculateIsTop(count);
        }

        private void CalculateIsTop(int count)
        {
            characher.IsTop = false;

            if (characher.speedVertical < 0)
                return;

            for (int i = 0; i < count; i++)
            {
                RaycastHit2D raycast = CharacterController2D.hits[i];
                if (raycast.collider == null)
                    continue;
                if (raycast.collider.isTrigger)
                    continue;

                if (raycast.transform == characher.transform || raycast.transform.IsChildOf(characher.transform))
                    continue;

                float angle = Vector2.Angle(raycast.normal, Vector2.down);

                if (angle < topAngle)
                {
                    characher.IsTop = true;
                    characher.TopInfo = raycast;
                    break;
                }
            }

        }

        private bool IsTopByContacts(int count)
        {
            bool isTop = false;

            float min_angle = 90;
            //int min_index = 0;

            for (int i = 0; i < count; i++)
            {
                if (contacts[i].collider == null)
                    continue;

                if (contacts[i].collider.isTrigger)
                    continue;

                if (contacts[i].collider.usedByEffector)
                {
                    Vector3 center = contacts[i].collider.bounds.center;
                    Vector2 point = characher.Rigidbody.ClosestPoint(center);
                    if (contacts[i].collider.bounds.Contains(point))
                        continue;
                }

                float angle = Vector2.Angle(contacts[i].normal, Vector2.down);

                if (angle < min_angle)
                    min_angle = angle;
            }

            if (min_angle < topAngle)
            {
                isTop = true;
            }

            return isTop;
        }

        private void UpdateTopInfo()
        {
            if (!characher.IsTop)
                return;

            if (Time.time - lastUpdateTopInfoTime < 0.06f)
                return;

            for (int i = 0; i < contactsCount; i++)
            {
                // 只要有一个点 和 当前法线比较接近 就不需要更新
                bool d = Vector3.Distance(contacts[i].point, characher.TopInfo.point) < 0.1f;
                bool a = Vector3.Angle(contacts[i].normal, characher.TopInfo.normal) < 3;
                // 认为法线区别不大 不需要更新
                if (d && a)
                    return;
            }

            // 更新碰撞点 和 法线 
            CheckTopInfo();
        }

    }

}

