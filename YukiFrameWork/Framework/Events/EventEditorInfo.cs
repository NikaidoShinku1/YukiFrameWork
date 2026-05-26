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

        public static string AnalysisWindowTitle => IsEN ? "Event Analysis" : "事件注册分析";

        public static string AnalysisRefresh => IsEN ? "Refresh" : "刷新";

        public static string AnalysisScanProject => IsEN ? "Scan Project" : "扫描工程";

        public static string AnalysisScanProgressTitle => IsEN ? "Event Static Scan" : "事件静态扫描";

        public static string AnalysisScanProgressFormat => IsEN
            ? "Scanning scripts ({0}/{1})  {2}"
            : "正在扫描脚本 ({0}/{1})  {2}";

        public static string AnalysisScanAnalyzing => IsEN
            ? "Analyzing lifecycle safety..."
            : "正在分析生命周期安全...";

        public static string AnalysisScanBuilding => IsEN
            ? "Building scan result..."
            : "正在汇总扫描结果...";

        public static string AnalysisClearHistory => IsEN ? "Clear History" : "清空历史";

        public static string AnalysisAutoRefresh => IsEN ? "Auto Refresh" : "自动刷新";

        public static string AnalysisChannelTitle => IsEN ? "Event Channels" : "事件通道";

        public static string AnalysisSubscriberTitle => IsEN ? "Registered Objects" : "注册对象";

        public static string AnalysisLifecycleTitle => IsEN ? "Lifecycle Safety" : "生命周期安全";

        public static string AnalysisHistoryTitle => IsEN ? "Register / Unregister Timeline" : "注册 / 注销时间线";

        public static string AnalysisDetailTitle => IsEN ? "Call Stack Detail" : "调用栈详情";

        public static string AnalysisRegisterStack => IsEN ? "[Register Stack]" : "[注册调用栈]";

        public static string AnalysisUnregisterStack => IsEN ? "[Unregister Stack]" : "[注销调用栈]";

        public static string AnalysisStackTracePlaceholder => IsEN ? "Select a history, subscriber or lifecycle item to inspect call stacks." : "选择历史记录、注册对象或生命周期项以查看调用栈。";

        public static string AnalysisDestroyedMark => IsEN ? " [Destroyed]" : " [已销毁]";

        public static string AnalysisPlayMode => IsEN ? "Play Mode" : "运行中";

        public static string AnalysisEditMode => IsEN ? "Edit Mode" : "编辑模式";

        public static string AnalysisSummaryFormat => IsEN
            ? "{0}  |  Channels: {1}  |  Safe: {2}  |  Risk: {3}  |  History: {4}  |  Filter: {5}"
            : "{0}  |  通道数: {1}  |  安全: {2}  |  风险: {3}  |  历史: {4}  |  筛选: {5}";

        public static string AnalysisStaticSummaryFormat => IsEN
            ? "{0}  |  Files: {1}  |  Safe: {2}  |  Risk: {3}  |  Calls: {4}  |  Filter: {5}  |  Scan: {6}"
            : "{0}  |  文件: {1}  |  安全: {2}  |  风险: {3}  |  调用: {4}  |  筛选: {5}  |  扫描: {6}";

        public static string AnalysisStaticMode => IsEN ? "Static Scan" : "静态扫描";

        public static string AnalysisNoScanSummary => IsEN
            ? "Edit Mode  |  No scan yet. Click \"Scan Project\" to analyze event registrations."
            : "编辑模式  |  尚未扫描，请点击「扫描工程」分析事件注册。";

        public static string AnalysisActionRegister => IsEN ? "Register" : "注册";

        public static string AnalysisActionUnRegister => IsEN ? "Unregister" : "注销";

        public static string AnalysisActionUnRegisterAll => IsEN ? "Unregister All" : "全部注销";

        public static string AnalysisActionLifecycleBind => IsEN ? "Lifecycle Bind" : "生命周期绑定";

        public static string AnalysisActionSend => IsEN ? "Send" : "发送";

        public static string AnalysisOnlyRiskToggle => IsEN ? "Risk Only" : "仅显示风险";

        public static string AnalysisFilterAll => IsEN ? "All" : "全部";

        public static string AnalysisFilterSafe => IsEN ? "Safe" : "安全";

        public static string AnalysisFilterRisk => IsEN ? "Risk" : "风险";

        public static bool IsRiskStatus(EventLifecycleSafetyStatus status)
        {
            return status == EventLifecycleSafetyStatus.ActiveOrphanRisk
                   || status == EventLifecycleSafetyStatus.LeakSuspect;
        }

        public static bool IsSafeStatus(EventLifecycleSafetyStatus status)
            => !IsRiskStatus(status);

        public static bool MatchesSafetyFilter(EventLifecycleSafetyStatus status, int filterValue)
        {
            return filterValue switch
            {
                1 => IsSafeStatus(status),
                2 => IsRiskStatus(status),
                _ => true
            };
        }

        public static string GetSafetyFilterLabel(int filterValue)
        {
            return filterValue switch
            {
                1 => AnalysisFilterSafe,
                2 => AnalysisFilterRisk,
                _ => AnalysisFilterAll
            };
        }

        public static string GetSafetyLabel(EventLifecycleSafetyStatus status)
        {
            if (IsEN)
            {
                return status switch
                {
                    EventLifecycleSafetyStatus.ActiveBoundDestroy => "Safe / Destroy",
                    EventLifecycleSafetyStatus.ActiveBoundDisable => "Safe / Disable",
                    EventLifecycleSafetyStatus.ActiveBoundScene => "Safe / Scene",
                    EventLifecycleSafetyStatus.ActiveStatic => "Static",
                    EventLifecycleSafetyStatus.ActiveOrphanRisk => "Risk / Orphan",
                    EventLifecycleSafetyStatus.LeakSuspect => "Leak",
                    EventLifecycleSafetyStatus.UnregisteredManual => "Closed / Manual",
                    EventLifecycleSafetyStatus.UnregisteredAll => "Closed / All",
                    EventLifecycleSafetyStatus.UnregisteredByLifecycle => "Closed / Lifecycle",
                    _ => status.ToString()
                };
            }

            return status switch
            {
                EventLifecycleSafetyStatus.ActiveBoundDestroy => "安全 / 销毁绑定",
                EventLifecycleSafetyStatus.ActiveBoundDisable => "安全 / 失活绑定",
                EventLifecycleSafetyStatus.ActiveBoundScene => "安全 / 场景绑定",
                EventLifecycleSafetyStatus.ActiveStatic => "静态方法",
                EventLifecycleSafetyStatus.ActiveOrphanRisk => "风险 / 未绑定",
                EventLifecycleSafetyStatus.LeakSuspect => "泄漏",
                EventLifecycleSafetyStatus.UnregisteredManual => "已关闭 / 手动注销",
                EventLifecycleSafetyStatus.UnregisteredAll => "已关闭 / 全部注销",
                EventLifecycleSafetyStatus.UnregisteredByLifecycle => "已关闭 / 生命周期",
                _ => status.ToString()
            };
        }
    }
}
