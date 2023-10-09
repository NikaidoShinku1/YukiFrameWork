using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    /// <summary>
    /// ���������߱�׼��ʽ��B(t) = P0 + (P1-P0)t
    /// </summary>
    public class BezierManager
    {
        #region ���������� һ�׶�������
        /// <summary>
        /// һ�ױ��������� ��ʽ��B(t) = (1-t)P0 +tP1 Mathf.Clamp(t,0,1)
        /// </summary>
        /// <param name="p0">P0�����</param>
        /// <param name="p1">P1�����</param>
        /// <param name="t">��㵽�յ�֮����ȡһ��</param>
        /// <returns>�������ջ���</returns>
        public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, float t)
        {
            //Vector3 p0p1 = (1 - t) * p0 + t * p1;
            Vector3 p0p1 = p0 + (p1 - p0) * t;
            return p0p1;
        }

        /// <summary>
        /// ���ױ��������� ��ʽ��B(t) = (1-t)(1-t)p0 + 2t(1-t)P1 + t*t*P2 Mathf.Clamp(t,0,1)
        /// </summary>
        /// <param name="p0">P0�����</param>
        /// <param name="p1">P1�����</param>
        /// <param name="p2">P2�����</param>
        /// <param name="t">��㵽�յ�֮����ȡһ��</param>
        /// <returns>�������ջ���</returns>
        public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            //����������ʽ����
            //Vector3 p0p1p2 = (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2; 

            //��׼��ʽǶ��
            Vector3 p0p1 = p0 + (p1 - p0) * t;
            Vector3 p1p2 = p1 + (p2 - p1) * t;

            Vector3 p0p1p2 = p0p1 + (p1p2 - p0p1) * t;
            return p0p1p2;
        }

        /// <summary>
        /// ���ױ��������� ��ʽ�� B(t) = (1-t)(1-t)(1-t)P0 + 3P1 * t *(1-t)*(1-t) + 3P2 * t * t * (1-t)+P3* t * t * t Mathf.Clamp(t,0,1)
        /// </summary>
        /// <param name="p0">P0�����</param>
        /// <param name="p1">P1�����</param>
        /// <param name="p2">P2�����</param>
        /// <param name="p3">P3�����</param>
        /// <param name="t">��㵽�յ�֮����ȡһ��</param>
        /// <returns>�������ջ���</returns>
        public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            //Vector3 p0p1p2p3 = (1 - t) * (1 - t) * (1 - t) * p0 + p1 * 3 * t * (1 - t) * (1 - t) + p2 * 3 * t * t * (1 - t) + p3 * t * t * t;
            Vector3 p0p1 = p0 + (p1 - p0) * t;
            Vector3 p1p2 = p1 + (p2 - p1) * t;
            Vector3 p2p3 = p2 + (p3 - p2) * t;

            Vector3 p0p1p2 = p0p1 + (p1p2 - p0p1) * t;
            Vector3 p1p2p3 = p1p2 + (p2p3 - p1p2) * t;

            Vector3 p0p1p2p3 = p0p1p2 + (p1p2p3 - p0p1p2) * t;
            return p0p1p2p3;
        }
        #endregion

        #region ��ֵ�汴�������� һ�׶�������
        /// <summary>
        /// ��ֵ��һ�ױ���������
        /// </summary>
        /// <param name="p0">P0�����</param>
        /// <param name="p1">P1�����</param>
        /// <param name="t">��㵽�յ�֮����ȡһ��</param>
        /// <returns>�������ջ���</returns>
        public static Vector3 BezierLerp(Vector3 p0, Vector3 p1, float t)
        {
            return Vector3.Lerp(p0, p1, t);
        }

        /// <summary>
        /// ��ֵ����ױ���������
        /// </summary>
        /// <param name="p0">P0�����</param>
        /// <param name="p1">P1�����</param>
        /// <param name="p2">P2�����</param>
        /// <param name="t">��㵽�յ�֮����ȡһ��</param>
        /// <returns>�������ջ���</returns>
        public static Vector3 BezierLerp(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 p0p1 = Vector3.Lerp(p0, p1, t);
            Vector3 p1p2 = Vector3.Lerp(p1, p2, t);
            Vector3 p0p1p2 = Vector3.Lerp(p0p1, p1p2, t);
            return p0p1p2;
        }

        /// <summary>
        /// ��ֵ�����ױ���������
        /// </summary>
        /// <param name="p0">P0�����</param>
        /// <param name="p1">P1�����</param>
        /// <param name="p2">P2�����</param>
        /// <param name="p3">P3�����</param>
        /// <param name="t">��㵽�յ�֮����ȡһ��</param>
        /// <returns>�������ջ���</returns>
        public static Vector3 BezierLerp(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 p0p1 = Vector3.Lerp(p0, p1, t);
            Vector3 p1p2 = Vector3.Lerp(p1, p2, t);
            Vector3 p2p3 = Vector3.Lerp(p2, p3, t);

            Vector3 p0p1p2 = Vector3.Lerp(p0p1, p1p2, t);
            Vector3 p1p2p3 = Vector3.Lerp(p1p2, p2p3, t);
            return Vector3.Lerp(p0p1p2, p1p2p3, t);
        }
        #endregion

        #region ��ȡ���������ߵ���������
        public static List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, float t)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < t; i++)
            {
                result.Add(BezierCurve(p0, p1, i / t));
            }
            return result;
        }

        public static List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < t; i++)
            {
                result.Add(BezierCurve(p0, p1, p2, i / t));
            }
            return result;
        }

        public static List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < t; i++)
            {
                result.Add(BezierCurve(p0, p1, p2, p3, i / t));
            }
            return result;
        }
        #endregion
    }
}
