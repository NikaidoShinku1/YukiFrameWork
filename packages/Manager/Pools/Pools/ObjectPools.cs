using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.Pools
{
    /// <summary>
    /// ������ع��ӿڣ������ܶ�����������
    /// </summary>
    public interface IInitializeObject
    {
        void Initialize(ObjectPools pools);
        void Close();
    }

    /// <summary>
    /// �������
    /// </summary>
    public class ObjectPools
    {
        //���Ӷ���
        private Queue<Object> poolTeam;

        //������������Ԥ����
        private  Object prefab;
           
        //�˶���ػ�����Ʒ������ͼ�б���ĸ�����λ��
        private GameObject parentObject;
        
        //�Ƿ���ɶԶ���صĳ�ʼ��
        public bool IsInit { get; private set; }
        /// <summary>
        /// ���캯����ʼ��
        /// </summary>
        /// <param name="type">����</param>
        /// <param name="prefab">Ԥ����</param>     
        public ObjectPools(GameObject parentObject, Object prefab,int initSize)
        {           
            this.prefab = prefab;            
            this.parentObject = parentObject;
            this.poolTeam = new Queue<Object>(initSize);
            IsInit = true;
        }

        public ObjectPools() { }

        public void RegisterObject(GameObject prefab,GameObject parentObject,int initSize)
        {
            if (IsInit) return;
            this.prefab = prefab;
            this.parentObject = parentObject;
            poolTeam = new Queue<Object>(initSize);
            IsInit = true;
        }

        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="target">λ��</param>
        /// <param name="rotate">��ת�Ƕ�</param>
        /// <returns>����һ��ָ�����͵Ķ���</returns>
        public GameObject GetPoolObject(Vector3 target,Quaternion rotate)
        {
            GameObject obj;
            if (parentObject == null)
            {
                parentObject = new GameObject();
                parentObject.name = prefab.name + "Manager";
            }

            if (poolTeam.Count > 0)
            {
                obj = poolTeam.Dequeue() as GameObject;
                obj.transform.SetParent(null);
                obj.transform.SetPositionAndRotation(target, rotate);               
            }
            else
            {
                obj = Object.Instantiate(prefab,target,rotate) as GameObject;
            }            
            obj.SetActive(true);
            if (obj.GetComponent<IInitializeObject>() == null)
            {
                throw new System.NullReferenceException("���ע��Ķ��󲻴��ڳ�ʼ���ӿ��볢����������ԣ�");
            }
            obj.GetComponent<IInitializeObject>().Initialize(this);

            return obj;
        }

        /// <summary>
        /// �ж������Ƿ�����ʾ״̬
        /// </summary>
        /// <param name="item">����</param>
        /// <returns>�����ͬ����True</returns>
        public bool Contains(GameObject item)
        {
            if(item.activeInHierarchy) return true;
            return false;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="item">����</param>
        public void RecyleObject(GameObject item)
        {
            if (Contains(item))
            {
                item.SetActive(false);
                item.transform.SetParent(parentObject.transform);
                item.GetComponent<IInitializeObject>().Close();
                poolTeam.Enqueue(item);
            }
        }
    }
   
}
