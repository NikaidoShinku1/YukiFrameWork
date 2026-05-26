#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace YukiFrameWork.Events.Editor
{
    public class EventAnalysisWindow : EditorWindow
    {
        private const string AutoRefreshKey = "YukiFrameWork.EventAnalysis.AutoRefresh";
        private const string SafetyFilterKey = "YukiFrameWork.EventAnalysis.SafetyFilter";

        private TextField searchField;
        private Button filterAllButton;
        private Button filterSafeButton;
        private Button filterRiskButton;
        private Label summaryLabel;
        private ListView channelListView;
        private ListView subscriberListView;
        private ListView lifecycleListView;
        private ListView historyListView;
        private ScrollView detailScrollView;
        private Label detailLabel;
        private bool autoRefresh;
        private EventAnalysisSafetyFilter safetyFilter = EventAnalysisSafetyFilter.All;

        private EventChannelSnapshot[] filteredChannels = Array.Empty<EventChannelSnapshot>();
        private EventLifecycleReport[] filteredLifecycleReports = Array.Empty<EventLifecycleReport>();
        private EventDiagnosticRecord[] filteredHistory = Array.Empty<EventDiagnosticRecord>();
        private EventChannelSnapshot selectedChannel;
        private EventStaticScanResult staticScanResult;
        private bool useStaticData;
        private bool staticHistoryCleared;

        [MenuItem("YukiFrameWork/LocalWindow/事件分析器", false, 1200)]
        public static void OpenWindow()
        {
            var window = GetWindow<EventAnalysisWindow>();
            window.titleContent = new GUIContent(EventEditorInfo.AnalysisWindowTitle);
            window.minSize = new Vector2(1080f, 620f);
            window.Show();
        }

        private void OnEnable()
        {
            autoRefresh = EditorPrefs.GetBool(AutoRefreshKey, true);
            safetyFilter = (EventAnalysisSafetyFilter)Mathf.Clamp(
                EditorPrefs.GetInt(SafetyFilterKey, (int)EventAnalysisSafetyFilter.All),
                0,
                2);
            EventDiagnostics.Changed += OnDiagnosticsChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EventDiagnostics.Changed -= OnDiagnosticsChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        public void CreateGUI()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            BuildToolbar();
            BuildSummary();

            var splitContainer = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            splitContainer.style.flexGrow = 1;
            rootVisualElement.Add(splitContainer);
            splitContainer.Add(BuildChannelPane());

            var rightPane = new VisualElement { style = { flexGrow = 1 } };
            rightPane.style.flexDirection = FlexDirection.Column;
            splitContainer.Add(rightPane);

            var upperSplit = new TwoPaneSplitView(0, 220, TwoPaneSplitViewOrientation.Vertical);
            upperSplit.style.flexGrow = 1;
            rightPane.Add(upperSplit);
            upperSplit.Add(BuildSubscriberPane());
            upperSplit.Add(BuildLifecyclePane());

            var lowerSplit = new TwoPaneSplitView(0, 220, TwoPaneSplitViewOrientation.Vertical);
            lowerSplit.style.flexGrow = 1;
            rightPane.Add(lowerSplit);
            lowerSplit.Add(BuildHistoryPane());
            lowerSplit.Add(BuildDetailPane());

            staticScanResult = EventStaticAnalyzer.GetCachedResult();
            RefreshDisplay();
        }

        private void BuildToolbar()
        {
            var toolbar = EventAnalysisStyles.CreateToolbarRow();

            toolbar.Add(EventAnalysisStyles.CreateActionButton(
                EventEditorInfo.AnalysisRefresh,
                RefreshDisplay,
                EventAnalysisToolbarActionKind.Neutral,
                "Refresh"));
            toolbar.Add(EventAnalysisStyles.CreateActionButton(
                EventEditorInfo.AnalysisScanProject,
                ScanProject,
                EventAnalysisToolbarActionKind.Primary,
                "d_Search Icon"));
            toolbar.Add(EventAnalysisStyles.CreateActionButton(
                EventEditorInfo.AnalysisClearHistory,
                ClearHistory,
                EventAnalysisToolbarActionKind.Danger,
                "TreeEditor.Trash"));

            toolbar.Add(EventAnalysisStyles.CreateToolbarSeparator());

            var autoRefreshToggle = new Toggle(EventEditorInfo.AnalysisAutoRefresh) { value = autoRefresh };
            autoRefreshToggle.style.flexShrink = 0;
            autoRefreshToggle.style.marginLeft = 6;
            autoRefreshToggle.style.marginRight = 6;
            autoRefreshToggle.RegisterValueChangedCallback(evt =>
            {
                autoRefresh = evt.newValue;
                EditorPrefs.SetBool(AutoRefreshKey, autoRefresh);
            });
            toolbar.Add(autoRefreshToggle);

            toolbar.Add(EventAnalysisStyles.CreateToolbarSeparator());

            BuildSafetyFilterToolbar(toolbar);

            searchField = new TextField();
            searchField.style.flexGrow = 1;
            searchField.style.flexShrink = 1;
            searchField.style.minWidth = 120;
            searchField.style.marginLeft = 6;
            searchField.style.marginRight = 4;
            searchField.RegisterValueChangedCallback(_ => RefreshDisplay());
            toolbar.Add(searchField);

            rootVisualElement.Add(toolbar);
        }

        private void BuildSafetyFilterToolbar(VisualElement toolbar)
        {
            filterAllButton = EventAnalysisStyles.CreateFilterButton(
                EventEditorInfo.AnalysisFilterAll,
                () => SetSafetyFilter(EventAnalysisSafetyFilter.All));
            filterSafeButton = EventAnalysisStyles.CreateFilterButton(
                EventEditorInfo.AnalysisFilterSafe,
                () => SetSafetyFilter(EventAnalysisSafetyFilter.Safe));
            filterRiskButton = EventAnalysisStyles.CreateFilterButton(
                EventEditorInfo.AnalysisFilterRisk,
                () => SetSafetyFilter(EventAnalysisSafetyFilter.Risk));

            toolbar.Add(EventAnalysisStyles.CreateFilterGroup(filterAllButton, filterSafeButton, filterRiskButton));
            UpdateFilterButtonStyles();
        }

        private void UpdateFilterButtonStyles()
        {
            EventAnalysisStyles.ApplyFilterButton(filterAllButton, safetyFilter == EventAnalysisSafetyFilter.All);
            EventAnalysisStyles.ApplyFilterButton(filterSafeButton, safetyFilter == EventAnalysisSafetyFilter.Safe);
            EventAnalysisStyles.ApplyFilterButton(filterRiskButton, safetyFilter == EventAnalysisSafetyFilter.Risk);
        }

        private void SetSafetyFilter(EventAnalysisSafetyFilter filter)
        {
            safetyFilter = filter;
            EditorPrefs.SetInt(SafetyFilterKey, (int)safetyFilter);
            UpdateFilterButtonStyles();
            RefreshDisplay();
        }

        private void BuildSummary()
        {
            summaryLabel = new Label();
            summaryLabel.style.marginLeft = 8;
            summaryLabel.style.marginRight = 8;
            summaryLabel.style.marginTop = 4;
            summaryLabel.style.marginBottom = 4;
            summaryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            rootVisualElement.Add(summaryLabel);
        }

        private VisualElement BuildChannelPane()
        {
            var pane = new VisualElement { style = { flexGrow = 1 } };
            pane.Add(CreateTitle(EventEditorInfo.AnalysisChannelTitle));
            channelListView = CreateListView(BindChannelItem, OnChannelSelectionChanged);
            pane.Add(channelListView);
            return pane;
        }

        private VisualElement BuildSubscriberPane()
        {
            var pane = new VisualElement { style = { flexGrow = 1 } };
            pane.Add(CreateTitle(EventEditorInfo.AnalysisSubscriberTitle));
            subscriberListView = CreateListView(BindSubscriberItem, OnSubscriberSelectionChanged);
            pane.Add(subscriberListView);
            return pane;
        }

        private VisualElement BuildLifecyclePane()
        {
            var pane = new VisualElement { style = { flexGrow = 1 } };
            pane.Add(CreateTitle(EventEditorInfo.AnalysisLifecycleTitle));
            lifecycleListView = CreateListView(BindLifecycleItem, OnLifecycleSelectionChanged);
            pane.Add(lifecycleListView);
            return pane;
        }

        private VisualElement BuildHistoryPane()
        {
            var pane = new VisualElement { style = { flexGrow = 1 } };
            pane.Add(CreateTitle(EventEditorInfo.AnalysisHistoryTitle));
            historyListView = CreateListView(BindHistoryItem, OnHistorySelectionChanged);
            pane.Add(historyListView);
            return pane;
        }

        private VisualElement BuildDetailPane()
        {
            var pane = new VisualElement { style = { flexGrow = 1 } };
            pane.Add(CreateTitle(EventEditorInfo.AnalysisDetailTitle));

            detailScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            detailScrollView.style.flexGrow = 1;
            detailScrollView.style.marginLeft = 8;
            detailScrollView.style.marginRight = 8;
            detailScrollView.style.marginBottom = 8;
            detailScrollView.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);

            detailLabel = new Label(EventEditorInfo.AnalysisStackTracePlaceholder);
            detailLabel.style.whiteSpace = WhiteSpace.Normal;
            detailLabel.style.marginLeft = 6;
            detailLabel.style.marginRight = 6;
            detailLabel.style.marginTop = 6;
            detailLabel.style.marginBottom = 6;
            detailScrollView.Add(detailLabel);
            pane.Add(detailScrollView);
            return pane;
        }

        private static Label CreateTitle(string text)
        {
            var title = new Label(text);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginLeft = 8;
            title.style.marginTop = 6;
            title.style.marginBottom = 4;
            return title;
        }

        private static ListView CreateListView(
            Action<VisualElement, int> bindItem,
            Action<IEnumerable<object>> selectionChanged)
        {
            var listView = new ListView
            {
                selectionType = SelectionType.Single,
                reorderable = false,
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                fixedItemHeight = 24
            };
            listView.style.flexGrow = 1;
            listView.style.marginLeft = 8;
            listView.style.marginRight = 8;
            listView.style.marginBottom = 8;
            listView.makeItem = () =>
            {
                var row = new VisualElement { style = { flexGrow = 1 } };
                var label = new Label { style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
                label.style.marginLeft = 4;
                row.Add(label);
                return row;
            };
            listView.bindItem = bindItem;
#if UNITY_2022_3_OR_NEWER
            listView.selectionChanged += selectionChanged;
#else
            listView.onSelectionChange += selectionChanged;
#endif
            return listView;
        }

        private static Label GetRowLabel(VisualElement element)
            => element.Q<Label>();

        private void BindChannelItem(VisualElement element, int index)
        {
            var label = GetRowLabel(element);
            if (label == null || index < 0 || index >= filteredChannels.Length)
                return;

            var channel = filteredChannels[index];
            var riskCount = channel.Subscriptions?.Count(x => EventEditorInfo.IsRiskStatus(x.SafetyStatus)) ?? 0;
            label.text = riskCount > 0
                ? $"{channel.Key.DisplayName}  [{channel.ListenerCount}]  !{riskCount}"
                : $"{channel.Key.DisplayName}  [{channel.ListenerCount}]";

            if (safetyFilter == EventAnalysisSafetyFilter.All && riskCount > 0)
                EventAnalysisStyles.ApplyRiskText(label);
            else
                EventAnalysisStyles.ClearTextStyle(label);
        }

        private void BindSubscriberItem(VisualElement element, int index)
        {
            var label = GetRowLabel(element);
            if (label == null
                || subscriberListView.itemsSource is not IList items
                || index < 0
                || index >= items.Count
                || items[index] is not EventSubscriptionSnapshot subscription)
                return;

            var targetName = subscription.TargetObject != null ? subscription.TargetObject.name : subscription.TargetTypeName;
            var aliveMark = subscription.TargetObject == null || subscription.IsAlive ? string.Empty : EventEditorInfo.AnalysisDestroyedMark;
            var safety = EventEditorInfo.GetSafetyLabel(subscription.SafetyStatus);
            label.text = $"#{subscription.Id}  [{safety}]  {subscription.HandlerName}  @ {targetName}{aliveMark}";
            ApplySafetyRowStyle(label, subscription.SafetyStatus);
        }

        private void BindLifecycleItem(VisualElement element, int index)
        {
            var label = GetRowLabel(element);
            if (label == null || index < 0 || index >= filteredLifecycleReports.Length)
                return;

            var report = filteredLifecycleReports[index];
            var targetName = report.TargetObject != null ? report.TargetObject.name : report.TargetTypeName;
            var activeMark = report.IsActive ? "●" : "○";
            var safety = EventEditorInfo.GetSafetyLabel(report.SafetyStatus);
            var unregisterMark = report.HasMatchingUnregister ? " ✓" : report.IsActive ? " ✗" : string.Empty;
            label.text = $"{activeMark} #{report.SubscriptionId}  [{safety}]{unregisterMark}  {report.ChannelKey.DisplayName}  {report.HandlerName}  @ {targetName}";
            ApplySafetyRowStyle(label, report.SafetyStatus);
        }

        private void BindHistoryItem(VisualElement element, int index)
        {
            var label = GetRowLabel(element);
            if (label == null || index < 0 || index >= filteredHistory.Length)
                return;

            var record = filteredHistory[index];
            var actionName = GetActionLabel(record);
            var targetName = record.TargetObject != null ? record.TargetObject.name : record.TargetTypeName;
            var idMark = record.SubscriptionId > 0 ? $"#{record.SubscriptionId}  " : string.Empty;
            label.text = $"{record.Timestamp:HH:mm:ss.fff}  {idMark}{actionName}  {record.ChannelKey.DisplayName}  {record.HandlerName}  @ {targetName}";

            var status = TryGetSafetyStatus(record.SubscriptionId);
            if (status.HasValue)
                ApplySafetyRowStyle(label, status.Value);
            else
                EventAnalysisStyles.ClearTextStyle(label);
        }

        private EventLifecycleSafetyStatus? TryGetSafetyStatus(int subscriptionId)
        {
            if (subscriptionId <= 0)
                return null;

            var reports = useStaticData
                ? staticScanResult?.LifecycleReports
                : EventManager.GetLifecycleReports();
            if (reports == null)
                return null;

            var report = reports.FirstOrDefault(x => x.SubscriptionId == subscriptionId);
            return report.SubscriptionId == subscriptionId ? report.SafetyStatus : null;
        }

        private void ApplySafetyRowStyle(Label label, EventLifecycleSafetyStatus status)
        {
            if (safetyFilter != EventAnalysisSafetyFilter.All)
            {
                EventAnalysisStyles.ClearTextStyle(label);
                return;
            }

            if (EventEditorInfo.IsRiskStatus(status))
                EventAnalysisStyles.ApplyRiskText(label);
            else
                EventAnalysisStyles.ClearTextStyle(label);
        }

        private void OnChannelSelectionChanged(IEnumerable<object> selectedItems)
        {
            var index = channelListView.selectedIndex;
            selectedChannel = index >= 0 && index < filteredChannels.Length
                ? filteredChannels[index]
                : default;
            subscriberListView.itemsSource = GetFilteredSubscriptions(selectedChannel);
            subscriberListView.Rebuild();
        }

        private List<EventSubscriptionSnapshot> GetFilteredSubscriptions(EventChannelSnapshot channel)
        {
            if (channel.Subscriptions == null)
                return new List<EventSubscriptionSnapshot>();

            return channel.Subscriptions
                .Where(x => EventEditorInfo.MatchesSafetyFilter(x.SafetyStatus, (int)safetyFilter))
                .ToList();
        }

        private void OnSubscriberSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (subscriberListView.selectedIndex < 0
                || subscriberListView.itemsSource is not IList items
                || subscriberListView.selectedIndex >= items.Count
                || items[subscriberListView.selectedIndex] is not EventSubscriptionSnapshot subscription)
                return;

            var report = useStaticData
                ? (staticScanResult?.LifecycleReports ?? Array.Empty<EventLifecycleReport>())
                    .FirstOrDefault(x => x.SubscriptionId == subscription.Id)
                : EventManager.GetLifecycleReports().FirstOrDefault(x => x.SubscriptionId == subscription.Id);
            if (report.SubscriptionId == subscription.Id)
                ShowDetail(BuildLifecycleDetail(report));
            else
                ShowDetail(BuildSubscriptionDetail(subscription.RegisterStackTrace, null, subscription.SafetyStatus, string.Empty));

            if (subscription.TargetObject != null)
            {
                EditorGUIUtility.PingObject(subscription.TargetObject);
                Selection.activeObject = subscription.TargetObject;
            }
            else if (useStaticData)
            {
                OpenStaticSource(subscription.Id);
            }
        }

        private void OnLifecycleSelectionChanged(IEnumerable<object> selectedItems)
        {
            var index = lifecycleListView.selectedIndex;
            if (index < 0 || index >= filteredLifecycleReports.Length)
                return;

            ShowDetail(BuildLifecycleDetail(filteredLifecycleReports[index]));
            if (useStaticData)
                OpenStaticSource(filteredLifecycleReports[index].SubscriptionId);
        }

        private void OnHistorySelectionChanged(IEnumerable<object> selectedItems)
        {
            var index = historyListView.selectedIndex;
            if (index < 0 || index >= filteredHistory.Length)
                return;

            var record = filteredHistory[index];
            EventLifecycleReport matchedReport = default;
            if (record.SubscriptionId > 0)
            {
                matchedReport = useStaticData
                    ? (staticScanResult?.LifecycleReports ?? Array.Empty<EventLifecycleReport>())
                        .FirstOrDefault(x => x.SubscriptionId == record.SubscriptionId)
                    : EventManager.GetLifecycleReports().FirstOrDefault(x => x.SubscriptionId == record.SubscriptionId);
            }

            var builder = new StringBuilder();
            builder.AppendLine($"[{GetActionLabel(record)}]  {record.ChannelKey.DisplayName}");
            builder.AppendLine($"{record.HandlerName}  @ {(record.TargetObject != null ? record.TargetObject.name : record.TargetTypeName)}");
            builder.AppendLine(record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            if (record.LifecycleBind != EventLifecycleBindType.None)
                builder.AppendLine($"Lifecycle: {record.LifecycleBind}");
            builder.AppendLine();
            builder.AppendLine(EventEditorInfo.AnalysisRegisterStack);
            builder.AppendLine(string.IsNullOrEmpty(record.StackTrace) ? "-" : record.StackTrace);

            if (matchedReport.SubscriptionId > 0 && !string.IsNullOrEmpty(matchedReport.UnregisterStackTrace))
            {
                builder.AppendLine();
                builder.AppendLine(EventEditorInfo.AnalysisUnregisterStack);
                builder.AppendLine(matchedReport.UnregisterStackTrace);
            }

            if (matchedReport.SubscriptionId > 0 && !string.IsNullOrEmpty(matchedReport.AnalysisMessage))
            {
                builder.AppendLine();
                builder.AppendLine(matchedReport.AnalysisMessage);
            }

            ShowDetail(builder.ToString());
            if (useStaticData && record.SubscriptionId > 0)
                OpenStaticSource(record.SubscriptionId);
        }

        private void OpenStaticSource(int subscriptionId)
        {
            if (staticScanResult == null)
                return;

            var entry = staticScanResult.Entries.FirstOrDefault(x => x.Id == subscriptionId);
            if (entry.Id != subscriptionId || string.IsNullOrEmpty(entry.FilePath))
                return;

            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(entry.FilePath);
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset, entry.Line);
                return;
            }

            InternalEditorUtility.OpenFileAtLineExternal(entry.FilePath, entry.Line);
        }

        private void RefreshDisplay()
        {
            if (channelListView == null || historyListView == null || subscriberListView == null || lifecycleListView == null || summaryLabel == null)
                return;

            useStaticData = !EditorApplication.isPlaying;
            if (useStaticData)
                staticScanResult ??= EventStaticAnalyzer.GetCachedResult();

            var keyword = searchField?.value?.Trim() ?? string.Empty;
            IReadOnlyList<EventChannelSnapshot> channels = useStaticData
                ? staticScanResult?.Channels ?? Array.Empty<EventChannelSnapshot>()
                : EventManager.GetChannelSnapshots();
            filteredChannels = FilterByKeyword(channels, keyword, x => x.Key.DisplayName)
                .Select(FilterChannelBySafety)
                .Where(x => x.ListenerCount > 0 || safetyFilter == EventAnalysisSafetyFilter.All)
                .ToArray();

            IEnumerable<EventLifecycleReport> lifecycleQuery = useStaticData
                ? staticScanResult?.LifecycleReports ?? Array.Empty<EventLifecycleReport>()
                : EventManager.GetLifecycleReports();
            lifecycleQuery = lifecycleQuery.Where(x =>
                EventEditorInfo.MatchesSafetyFilter(x.SafetyStatus, (int)safetyFilter));

            filteredLifecycleReports = FilterByKeyword(lifecycleQuery, keyword,
                    x => x.ChannelKey.DisplayName,
                    x => x.HandlerName,
                    x => x.TargetTypeName,
                    x => x.AnalysisMessage)
                .ToArray();

            IReadOnlyList<EventDiagnosticRecord> history = useStaticData
                ? GetStaticHistory()
                : EventManager.GetDiagnosticHistory();
            filteredHistory = FilterByKeyword(history, keyword,
                    x => x.ChannelKey.DisplayName,
                    x => x.HandlerName,
                    x => x.TargetTypeName,
                    x => x.StackTrace)
                .Where(MatchesHistorySafetyFilter)
                .ToArray();

            channelListView.itemsSource = filteredChannels;
            channelListView.Rebuild();

            lifecycleListView.itemsSource = filteredLifecycleReports;
            lifecycleListView.Rebuild();

            historyListView.itemsSource = filteredHistory;
            historyListView.Rebuild();

            if (channelListView.selectedIndex >= filteredChannels.Length)
            {
                channelListView.ClearSelection();
                selectedChannel = default;
            }

            subscriberListView.itemsSource = GetFilteredSubscriptions(selectedChannel);
            subscriberListView.Rebuild();

            UpdateSummaryLabel();
        }

        private IReadOnlyList<EventDiagnosticRecord> GetStaticHistory()
        {
            if (staticHistoryCleared || staticScanResult == null)
                return Array.Empty<EventDiagnosticRecord>();

            return staticScanResult.History;
        }

        private void UpdateSummaryLabel()
        {
            var filterLabel = EventEditorInfo.GetSafetyFilterLabel((int)safetyFilter);

            if (useStaticData)
            {
                if (staticScanResult == null)
                {
                    summaryLabel.text = EventEditorInfo.AnalysisNoScanSummary;
                    return;
                }

                var allReports = staticScanResult.LifecycleReports;
                var safeCount = allReports.Count(x => EventEditorInfo.IsSafeStatus(x.SafetyStatus));
                var riskCount = allReports.Count(x => EventEditorInfo.IsRiskStatus(x.SafetyStatus));

                summaryLabel.text = string.Format(
                    EventEditorInfo.AnalysisStaticSummaryFormat,
                    EventEditorInfo.AnalysisStaticMode,
                    staticScanResult.ScannedFileCount,
                    safeCount,
                    riskCount,
                    staticScanResult.Entries.Count,
                    filterLabel,
                    staticScanResult.ScanTime.ToString("HH:mm:ss"));
                return;
            }

            var runtimeReports = EventManager.GetLifecycleReports();
            var runtimeSafeCount = runtimeReports.Count(x => EventEditorInfo.IsSafeStatus(x.SafetyStatus));
            var runtimeRiskCount = runtimeReports.Count(x => EventEditorInfo.IsRiskStatus(x.SafetyStatus));
            summaryLabel.text = string.Format(
                EventEditorInfo.AnalysisSummaryFormat,
                EventEditorInfo.AnalysisPlayMode,
                filteredChannels.Length,
                runtimeSafeCount,
                runtimeRiskCount,
                filteredHistory.Length,
                filterLabel);
        }

        private EventChannelSnapshot FilterChannelBySafety(EventChannelSnapshot channel)
        {
            var subscriptions = GetFilteredSubscriptions(channel);
            return new EventChannelSnapshot(channel.Key, subscriptions.Count, subscriptions);
        }

        private bool MatchesHistorySafetyFilter(EventDiagnosticRecord record)
        {
            if (safetyFilter == EventAnalysisSafetyFilter.All)
                return true;

            var status = TryGetSafetyStatus(record.SubscriptionId);
            if (!status.HasValue)
                return safetyFilter == EventAnalysisSafetyFilter.Safe;

            return EventEditorInfo.MatchesSafetyFilter(status.Value, (int)safetyFilter);
        }

        private void ScanProject()
        {
            staticHistoryCleared = false;
            staticScanResult = EventStaticAnalyzer.ScanProject(forceRefresh: true);
            RefreshDisplay();
        }

        private static IEnumerable<T> FilterByKeyword<T>(IEnumerable<T> source, string keyword, params Func<T, string>[] selectors)
        {
            if (string.IsNullOrEmpty(keyword))
                return source;

            return source.Where(item =>
                selectors.Any(selector =>
                    (selector(item) ?? string.Empty).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private void ClearHistory()
        {
            if (!EditorApplication.isPlaying)
            {
                staticHistoryCleared = true;
            }
            else
            {
                EventManager.ClearDiagnosticHistory();
            }

            detailLabel.text = EventEditorInfo.AnalysisStackTracePlaceholder;
            RefreshDisplay();
        }

        private void OnDiagnosticsChanged()
        {
            if (!autoRefresh || useStaticData)
                return;

            RefreshDisplay();
            Repaint();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EventDiagnostics.ResetAll();
                staticHistoryCleared = false;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
                staticScanResult = EventStaticAnalyzer.GetCachedResult();

            RefreshDisplay();
        }

        private void ShowDetail(string text)
        {
            detailLabel.text = string.IsNullOrEmpty(text)
                ? EventEditorInfo.AnalysisStackTracePlaceholder
                : text;
        }

        private static string BuildSubscriptionDetail(
            string registerStack,
            string unregisterStack,
            EventLifecycleSafetyStatus status,
            string message)
        {
            var builder = new StringBuilder();
            builder.AppendLine(EventEditorInfo.GetSafetyLabel(status));
            if (!string.IsNullOrEmpty(message))
            {
                builder.AppendLine(message);
                builder.AppendLine();
            }

            builder.AppendLine(EventEditorInfo.AnalysisRegisterStack);
            builder.AppendLine(string.IsNullOrEmpty(registerStack) ? "-" : registerStack);

            if (!string.IsNullOrEmpty(unregisterStack))
            {
                builder.AppendLine();
                builder.AppendLine(EventEditorInfo.AnalysisUnregisterStack);
                builder.AppendLine(unregisterStack);
            }

            return builder.ToString();
        }

        private static string BuildLifecycleDetail(EventLifecycleReport report)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"#{report.SubscriptionId}  {report.ChannelKey.DisplayName}");
            builder.AppendLine($"{report.HandlerName}  @ {(report.TargetObject != null ? report.TargetObject.name : report.TargetTypeName)}");
            builder.AppendLine($"{EventEditorInfo.GetSafetyLabel(report.SafetyStatus)}  |  Active: {report.IsActive}  |  Matched Unregister: {report.HasMatchingUnregister}");
            if (report.LifecycleBind != EventLifecycleBindType.None)
                builder.AppendLine($"Lifecycle Bind: {report.LifecycleBind}  @ {(report.LifecycleOwner != null ? report.LifecycleOwner.name : "-")}");
            if (!string.IsNullOrEmpty(report.AnalysisMessage))
            {
                builder.AppendLine();
                builder.AppendLine(report.AnalysisMessage);
            }

            builder.AppendLine();
            builder.AppendLine(EventEditorInfo.AnalysisRegisterStack);
            builder.AppendLine(string.IsNullOrEmpty(report.RegisterStackTrace) ? "-" : report.RegisterStackTrace);

            if (!string.IsNullOrEmpty(report.UnregisterStackTrace))
            {
                builder.AppendLine();
                builder.AppendLine(EventEditorInfo.AnalysisUnregisterStack);
                builder.AppendLine(report.UnregisterStackTrace);
            }

            return builder.ToString();
        }

        private string GetActionLabel(EventDiagnosticRecord record)
        {
            if (useStaticData && record.HandlerName == "Send")
                return EventEditorInfo.AnalysisActionSend;

            return record.Action switch
            {
                EventDiagnosticAction.Register => EventEditorInfo.AnalysisActionRegister,
                EventDiagnosticAction.UnRegister => EventEditorInfo.AnalysisActionUnRegister,
                EventDiagnosticAction.UnRegisterAll => EventEditorInfo.AnalysisActionUnRegisterAll,
                EventDiagnosticAction.LifecycleBind => EventEditorInfo.AnalysisActionLifecycleBind + $"({record.LifecycleBind})",
                _ => record.Action.ToString()
            };
        }
    }
}
#endif
