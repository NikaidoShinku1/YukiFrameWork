using YukiFrameWork.Events;

namespace YukiFrameWork.MVC
{
    public interface IController : 
        IGetCommandCenter,IGetArchitecture,IGetModel ,IGetView , 
        IGetRegisterEvent, IGetEventTrigger,IUIPanelController
    { 
        
    }
}
