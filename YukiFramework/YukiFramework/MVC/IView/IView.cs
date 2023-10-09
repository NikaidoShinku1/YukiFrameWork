namespace YukiFrameWork.MVC
{
    public interface IView : IGetModel, IGetRegisterEvent,ISetArchitecture,IGetEventTrigger
    {               
        
        /// <summary>
        /// ��ͼ���ʼ���������ṩ��ϵģ�����
        /// </summary>
        /// <param name="Architecture">��ϵģ��</param>
        void Init();
         
        /// <summary>
        /// ͳһ������ͼ(�������ͼֻ���һ��Model�������ݵ���ͼ����ʱ���ڿ�����ע��ʹ��)
        /// </summary>
        void Unified_UpdateView(IModel model);
    }
}
