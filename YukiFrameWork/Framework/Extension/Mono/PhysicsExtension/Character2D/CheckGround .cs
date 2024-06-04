///=====================================================
/// - FileName:      CheckGround .cs
/// - NameSpace:     YukiFrameWork.Physics.Character2D
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/4 17:38:32
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Character2D
{
	public class CheckGround 
	{
		private ContactPoint2D[] contacts = null;

		private int contactsCount;

		private CharacterController2D character;

		private float groundAngle = 35f;

		private float lastCheckTime;

		private float lastUpdateGroundInfoTime;

		public bool IsGroundByContacts { get; protected set; }

		//每秒检测十五次
		private float timeInterval = 1.0f / 15f;

		internal void Check(ContactPoint2D[] contacts, CharacterController2D character,int count, float groundAngle)
		{
			this.contacts = contacts;
			this.character = character;
			this.groundAngle = groundAngle;
			contactsCount = count;

			IsGroundByContacts = GroundByContacts(count);

			if (IsGroundByContacts != character.IsGrounded)
				CheckGrounded();

			if (IsGroundByContacts)
			{
				UpdateGroupInfo();
			}
		}

        private void UpdateGroupInfo()
        {
			if (!character.IsGrounded)
				return;

			if (Time.time - lastUpdateGroundInfoTime < 0.1f)
				return;

			for (int i = 0; i < contactsCount; i++)
			{
				bool dis = Vector3.Distance(contacts[i].point, character.GroundInfo.point) < 0.06f;
				bool angle = Vector3.Angle(contacts[i].normal, character.GroundInfo.normal) < 3;

				if (dis && angle)
					return;
			}

			CheckGrounded();
        }

        private bool GroundByContacts(int count)
		{			
			float min_angle = 90f;

			for (int i = 0; i < count; i++)
			{
				if (contacts[i].collider == null)
					continue;

				if (contacts[i].collider.isTrigger)
					continue;

				//如果游戏物体在检测体上方则不执行。
				if (character.bottom.bounds.min.y < contacts[i].collider.bounds.min.y)
					continue;

				float angle = Vector2.Angle(contacts[i].normal, Vector2.up);

				if (angle < min_angle)
				{
					min_angle = angle;
				}
			}

			if (min_angle < groundAngle)
			{
				return true;
			}
			return false;
		}

		private void CheckGrounded()
		{
			if (!character.IsCheckGround)
				return;

			if (Time.time - lastCheckTime < timeInterval)
			{
				return;
			}

			lastCheckTime = Time.time;
			
			character.checkCount++;

			Vector2 dir = -character.bottom.transform.up;

			int count = 0;

			if (character.bottom is CircleCollider2D circle)
			{
				count = character.Raycast(circle, dir);
			}
			else if (character.bottom is BoxCollider2D box)
			{
				count = character.Raycast(box, dir);
			}
			else if (character.bottom is CapsuleCollider2D capsule)
			{
				count = character.Raycast(capsule, dir);
			}

			CaculateIsGrounded(count);
		}

		private void CaculateIsGrounded(int count)
		{
			character.IsGrounded = false;

			if (character.speedVertical > 0) return;

			for (int i = 0; i < count; i++)
			{
				RaycastHit2D hit = CharacterController2D.hits[i];

				if (hit.collider == null)
					continue;

				if (hit.collider.isTrigger)
					continue;

				if (hit.collider.transform == character.transform || hit.collider.transform.IsChildOf(character.transform))
					continue;

				float angle = Vector2.Angle(hit.normal,Vector2.up);

				if (angle < groundAngle)
				{
					character.IsGrounded = true;
					character.GroundInfo = hit;
					break;
				}
			}
		}

        public bool IsHaveColliderOnBelow(ContactPoint2D[] contacts, int count)
        {

            for (int i = 0; i < count; i++)
            {
                if (contacts[i].collider == null)
                    continue;

                if (contacts[i].collider.isTrigger)
                    continue;
                // 如果该游戏物体的最低点比该碰撞体的最低点小，说明该碰撞体不在该游戏物体下面
                if (character.bottom.bounds.min.y < contacts[i].collider.bounds.min.y)
                    continue;

                float angle = Vector2.Angle(contacts[i].normal, Vector2.up);

                // 下方有碰撞体
                if (angle < 35)
                    return true;
            }

            return false;
        }
    }
}
