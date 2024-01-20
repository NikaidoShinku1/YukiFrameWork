using UnityEngine;

namespace YukiFrameWork.Events
{
    public class EventEditorInfo
    {
        public static bool IsEN 
        {
            get => PlayerPrefs.GetInt("EditorIsEN") == 1;
            set => PlayerPrefs.SetInt("EditorIsEN",value ? 1 : 0);
        }

        public static string Tip => IsEN ? "Visual Event Registry (added only in edit mode)" : "可视化事件注册器(仅在编辑模式下添加)";

        public static string RegisterTypeInfo => IsEN ? "Please select identification type :" : "请选择标识类型:";

        public static string StringDescriptionInfo => IsEN ? "Enter the Name of each event registration in the NAME field" : "在Name输入框中填写每一个事件注册的名称";

        public static string EnumDescriptionInfo => IsEN ? "Fill in the full enumeration type, including the namespace, in the Name field and click Update enumeration data. \nTip: An update is required every time an enumeration type is changed." : "在Name输入框中填写包括命名空间在内的完整枚举类型,然后点击更新枚举数据即可。\nTip:每次修改枚举类型后都需要进行一次更新";

        public static string Update_EnumBtnInfo => IsEN ? "Update enumeration data" : "更新枚举数据";

        public static string Update_SuccessInfo => IsEN ? "Update enumeration data successfully! Currently registered enumeration type:" : "更新枚举数据成功!当前注册的枚举类型:";

        public static string Update_ErrorInfo => IsEN ? "Failed to update enumeration data! A nonexistent or incorrect enumeration may have been registered:" : "更新枚举数据失败!可能是注册了不存在或者错误的枚举:";
    }
}
