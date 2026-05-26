#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using YukiFrameWork;
using YukiFrameWork.Events;

namespace YukiFrameWork.Events.Editor
{
    public enum EventStaticEntryKind
    {
        Register,
        Unregister,
        Send,
        ComponentListener
    }

    public readonly struct EventStaticScanEntry
    {
        public int Id { get; }
        public EventStaticEntryKind Kind { get; }
        public EventChannelKey ChannelKey { get; }
        public string HandlerName { get; }
        public string ClassName { get; }
        public string MethodName { get; }
        public string FilePath { get; }
        public int Line { get; }
        public string Statement { get; }
        public EventLifecycleBindType LifecycleBind { get; }
        public EventLifecycleSafetyStatus SafetyStatus { get; }
        public bool HasMatchingUnregister { get; }
        public string UnregisterLocation { get; }
        public string AnalysisMessage { get; }

        internal EventStaticScanEntry(
            int id,
            EventStaticEntryKind kind,
            EventChannelKey channelKey,
            string handlerName,
            string className,
            string methodName,
            string filePath,
            int line,
            string statement,
            EventLifecycleBindType lifecycleBind,
            EventLifecycleSafetyStatus safetyStatus,
            bool hasMatchingUnregister,
            string unregisterLocation,
            string analysisMessage)
        {
            Id = id;
            Kind = kind;
            ChannelKey = channelKey;
            HandlerName = handlerName;
            ClassName = className;
            MethodName = methodName;
            FilePath = filePath;
            Line = line;
            Statement = statement;
            LifecycleBind = lifecycleBind;
            SafetyStatus = safetyStatus;
            HasMatchingUnregister = hasMatchingUnregister;
            UnregisterLocation = unregisterLocation;
            AnalysisMessage = analysisMessage;
        }

        public string Location => $"{FilePath}:{Line}";

        public string RegisterStackTrace => $"{Location}\n{ClassName}.{MethodName}()\n{Statement}";
    }

    public sealed class EventStaticScanResult
    {
        public IReadOnlyList<EventStaticScanEntry> Entries { get; internal set; } = Array.Empty<EventStaticScanEntry>();
        public IReadOnlyList<EventChannelSnapshot> Channels { get; internal set; } = Array.Empty<EventChannelSnapshot>();
        public IReadOnlyList<EventLifecycleReport> LifecycleReports { get; internal set; } = Array.Empty<EventLifecycleReport>();
        public IReadOnlyList<EventDiagnosticRecord> History { get; internal set; } = Array.Empty<EventDiagnosticRecord>();
        public int TotalRegisterCount { get; internal set; }
        public int RiskCount { get; internal set; }
        public DateTime ScanTime { get; internal set; }
        public int ScannedFileCount { get; internal set; }
    }

    public static class EventStaticAnalyzer
    {
        private static readonly string[] ScanRoots = { "Assets", "Packages" };
        private static EventStaticScanResult cachedResult;

        private static readonly Regex ClassRegex = new Regex(
            @"class\s+(?<name>[\w\.]+)",
            RegexOptions.Compiled);

        private static readonly Regex MethodRegex = new Regex(
            @"(?:(?:public|private|protected|internal|static|override|virtual|async|sealed|new|partial)\s+)*[\w<>\[\],\s.?]+\s+(?<name>[\w\.]+)\s*\(",
            RegexOptions.Compiled);

        private static readonly Regex AddListenerRegex = new Regex(
            @"\.AddListener(?:_Task)?\s*<\s*(?<type>[\w\.]+)\s*>\s*\(",
            RegexOptions.Compiled);

        private static readonly Regex RemoveListenerRegex = new Regex(
            @"\.RemoveListener(?:_Task)?\s*<\s*(?<type>[\w\.]+)\s*>\s*\((?<args>[^\)]*)\)",
            RegexOptions.Compiled);

        private static readonly Regex RemoveAllRegex = new Regex(
            @"\.RemoveAllListeners(?:_Task)?\s*<\s*(?<type>[\w\.]+)\s*>\s*(?:\((?<args>[^\)]*)\))?",
            RegexOptions.Compiled);

        private static readonly Regex SendRegex = new Regex(
            @"(?:EventManager\.SendEvent(?:_Task)?|(?:^|[^\w])SendEvent(?:_Task|_Async)?|\.Send(?:_Task)?)\s*(?:<|\()",
            RegexOptions.Compiled);

        private static readonly Regex TypeEventListenerRegex = new Regex(
            @":\s*TypeEventListener\s*<\s*(?<type>[\w\.]+)\s*>",
            RegexOptions.Compiled);

        private static readonly Regex StringEventListenerRegex = new Regex(
            @":\s*StringEventListener\s*<\s*(?<type>[\w\.]+)\s*>",
            RegexOptions.Compiled);

        private static readonly Regex EnumEventListenerRegex = new Regex(
            @":\s*EnumEventListener\s*<\s*[\w\.]+\s*,\s*(?<type>[\w\.]+)\s*>",
            RegexOptions.Compiled);

        public static EventStaticScanResult ScanProject(bool forceRefresh = false)
        {
            if (!forceRefresh && cachedResult != null)
                return cachedResult;

            var files = CollectScanFiles();
            var entries = new List<EventStaticScanEntry>();
            var nextId = 1;
            var cancelled = false;

            try
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var progress = files.Count > 0 ? (i + 1) / (float)files.Count * 0.92f : 0f;
                    if (EditorUtility.DisplayCancelableProgressBar(
                            EventEditorInfo.AnalysisScanProgressTitle,
                            string.Format(
                                EventEditorInfo.AnalysisScanProgressFormat,
                                i + 1,
                                files.Count,
                                Path.GetFileName(file)),
                            progress))
                    {
                        cancelled = true;
                        break;
                    }

                    ScanFile(File.ReadAllText(file), file, entries, ref nextId);
                }

                if (cancelled)
                    return cachedResult ?? CreateEmptyResult();

                EditorUtility.DisplayProgressBar(
                    EventEditorInfo.AnalysisScanProgressTitle,
                    EventEditorInfo.AnalysisScanAnalyzing,
                    0.94f);
                AnalyzeLifecycle(entries);

                EditorUtility.DisplayProgressBar(
                    EventEditorInfo.AnalysisScanProgressTitle,
                    EventEditorInfo.AnalysisScanBuilding,
                    0.98f);
                cachedResult = BuildResult(entries, files.Count);
                return cachedResult;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static List<string> CollectScanFiles()
        {
            var files = new List<string>();
            foreach (var file in EnumerateScriptFiles())
            {
                if (ShouldSkipFile(file))
                    continue;

                files.Add(file);
            }

            return files;
        }

        private static EventStaticScanResult CreateEmptyResult()
        {
            return new EventStaticScanResult
            {
                Entries = Array.Empty<EventStaticScanEntry>(),
                Channels = Array.Empty<EventChannelSnapshot>(),
                LifecycleReports = Array.Empty<EventLifecycleReport>(),
                History = Array.Empty<EventDiagnosticRecord>(),
                ScanTime = DateTime.Now,
                ScannedFileCount = 0
            };
        }

        public static void InvalidateCache() => cachedResult = null;

        public static EventStaticScanResult GetCachedResult() => cachedResult;

        private static IEnumerable<string> EnumerateScriptFiles()
        {
            foreach (var root in ScanRoots)
            {
                if (!Directory.Exists(root))
                    continue;

                foreach (var file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
                    yield return file.Replace('\\', '/');
            }
        }

        private static bool ShouldSkipFile(string file)
        {
            if (IsFrameworkInternalFile(file))
                return true;

            return file.Contains("/Editor/EventStaticAnalyzer.cs");
        }

        private static bool IsFrameworkInternalFile(string file)
        {
            return file.StartsWith("Packages/YukiFrameWork/", StringComparison.OrdinalIgnoreCase)
                   || file.IndexOf("/Packages/YukiFrameWork/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void ScanFile(string text, string file, List<EventStaticScanEntry> entries, ref int nextId)
        {
            ScanComponentListeners(text, file, entries, ref nextId);

            foreach (Match match in AddListenerRegex.Matches(text))
            {
                var statement = ExtractStatement(text, FindStatementStart(text, match.Index));
                var eventType = match.Groups["type"].Value;
                var isAsync = match.Value.Contains("_Task", StringComparison.Ordinal);
                var registerType = isAsync ? EventRegisterType.AsyncType : EventRegisterType.Type;
                var args = ExtractArgs(statement, eventType);
                var channelKey = BuildChannelKey(eventType, registerType, args, out var handlerName);
                entries.Add(CreateEntry(
                    ref nextId,
                    EventStaticEntryKind.Register,
                    channelKey,
                    handlerName,
                    text,
                    match.Index,
                    file,
                    statement,
                    DetectLifecycleBind(statement)));
            }

            foreach (Match match in RemoveListenerRegex.Matches(text))
            {
                var statement = ExtractStatement(text, FindStatementStart(text, match.Index));
                var eventType = match.Groups["type"].Value;
                var handler = ExtractFirstArg(match.Groups["args"].Value);
                var channelKey = EventChannelKey.ForType(ResolveType(eventType));
                entries.Add(CreateEntry(
                    ref nextId,
                    EventStaticEntryKind.Unregister,
                    channelKey,
                    handler,
                    text,
                    match.Index,
                    file,
                    statement,
                    EventLifecycleBindType.None));
            }

            foreach (Match match in RemoveAllRegex.Matches(text))
            {
                var statement = ExtractStatement(text, FindStatementStart(text, match.Index));
                var eventType = match.Groups["type"].Value;
                var args = match.Groups["args"].Success ? match.Groups["args"].Value : string.Empty;
                var channelKey = BuildChannelKey(eventType, EventRegisterType.Type, args, out _);
                entries.Add(CreateEntry(
                    ref nextId,
                    EventStaticEntryKind.Unregister,
                    channelKey,
                    "All",
                    text,
                    match.Index,
                    file,
                    statement,
                    EventLifecycleBindType.None));
            }

            foreach (Match match in SendRegex.Matches(text))
            {
                var statement = ExtractStatement(text, FindStatementStart(text, match.Index));
                var eventType = ExtractSendEventType(statement) ?? "Unknown";
                entries.Add(CreateEntry(
                    ref nextId,
                    EventStaticEntryKind.Send,
                    EventChannelKey.ForType(ResolveType(eventType)),
                    "Send",
                    text,
                    match.Index,
                    file,
                    statement,
                    EventLifecycleBindType.None));
            }
        }

        private static EventStaticScanEntry CreateEntry(
            ref int nextId,
            EventStaticEntryKind kind,
            EventChannelKey channelKey,
            string handlerName,
            string text,
            int index,
            string file,
            string statement,
            EventLifecycleBindType lifecycleBind)
        {
            return new EventStaticScanEntry(
                nextId++,
                kind,
                channelKey,
                handlerName,
                ExtractContainingClass(text, index) ?? Path.GetFileNameWithoutExtension(file),
                ExtractContainingMethod(text, index) ?? "Unknown",
                file,
                GetLineNumber(text, index),
                statement.Trim(),
                lifecycleBind,
                kind == EventStaticEntryKind.Unregister
                    ? EventLifecycleSafetyStatus.UnregisteredManual
                    : EventLifecycleSafetyStatus.ActiveOrphanRisk,
                kind == EventStaticEntryKind.Unregister,
                null,
                kind == EventStaticEntryKind.Unregister ? "Static unregister call." : string.Empty);
        }

        private static void ScanComponentListeners(string text, string file, List<EventStaticScanEntry> entries, ref int nextId)
        {
            ScanListenerMatches(text, file, TypeEventListenerRegex, "TypeEventListener", entries, ref nextId);
            ScanListenerMatches(text, file, StringEventListenerRegex, "StringEventListener", entries, ref nextId);
            ScanListenerMatches(text, file, EnumEventListenerRegex, "EnumEventListener", entries, ref nextId);
        }

        private static void ScanListenerMatches(
            string text,
            string file,
            Regex regex,
            string label,
            List<EventStaticScanEntry> entries,
            ref int nextId)
        {
            foreach (Match match in regex.Matches(text))
            {
                var eventType = match.Groups["type"].Value;
                var className = ExtractContainingClass(text, match.Index) ?? Path.GetFileNameWithoutExtension(file);
                var key = label switch
                {
                    "StringEventListener" => EventChannelKey.ForString(ResolveType(eventType), "*"),
                    "EnumEventListener" => EventChannelKey.ForEnum(ResolveType(eventType), null),
                    _ => EventChannelKey.ForType(ResolveType(eventType))
                };

                entries.Add(new EventStaticScanEntry(
                    nextId++,
                    EventStaticEntryKind.ComponentListener,
                    key,
                    "Trigger",
                    className,
                    "OnEnable/OnDisable",
                    file,
                    GetLineNumber(text, match.Index),
                    match.Value.Trim(),
                    EventLifecycleBindType.GameObjectDisable,
                    EventLifecycleSafetyStatus.ActiveBoundDisable,
                    true,
                    $"{file}:{GetLineNumber(text, match.Index)}",
                    $"{label} auto unregisters in OnDisable."));
            }
        }

        private static void AnalyzeLifecycle(List<EventStaticScanEntry> entries)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.Kind != EventStaticEntryKind.Register)
                    continue;

                var status = EvaluateRegisterSafety(entry, entries, out var message, out var hasUnregister, out var unregisterLocation);
                entries[i] = new EventStaticScanEntry(
                    entry.Id, entry.Kind, entry.ChannelKey, entry.HandlerName, entry.ClassName, entry.MethodName,
                    entry.FilePath, entry.Line, entry.Statement, entry.LifecycleBind,
                    status, hasUnregister, unregisterLocation, message);
            }
        }

        private static EventLifecycleSafetyStatus EvaluateRegisterSafety(
            EventStaticScanEntry register,
            List<EventStaticScanEntry> all,
            out string message,
            out bool hasUnregister,
            out string unregisterLocation)
        {
            hasUnregister = false;
            unregisterLocation = null;

            switch (register.LifecycleBind)
            {
                case EventLifecycleBindType.GameObjectDestroy:
                    message = "Chained UnRegisterWaitGameObjectDestroy detected.";
                    hasUnregister = true;
                    return EventLifecycleSafetyStatus.ActiveBoundDestroy;
                case EventLifecycleBindType.GameObjectDisable:
                    message = "Chained UnRegisterWaitGameObjectDisable detected.";
                    hasUnregister = true;
                    return EventLifecycleSafetyStatus.ActiveBoundDisable;
                case EventLifecycleBindType.SceneUnload:
                    message = "Chained UnRegisterWaitSceneUnLoad detected.";
                    hasUnregister = true;
                    return EventLifecycleSafetyStatus.ActiveBoundScene;
            }

            foreach (var unreg in all.Where(x => x.Kind == EventStaticEntryKind.Unregister && x.FilePath == register.FilePath))
            {
                if (!SameChannel(unreg.ChannelKey, register.ChannelKey))
                    continue;

                if (unreg.HandlerName == "All"
                    || string.Equals(unreg.HandlerName, register.HandlerName, StringComparison.Ordinal))
                {
                    hasUnregister = true;
                    unregisterLocation = unreg.Location;
                    message = $"Matched unregister in same file: {unreg.Location}";
                    return EventLifecycleSafetyStatus.UnregisteredManual;
                }
            }

            if (all.Any(x => x.Kind == EventStaticEntryKind.ComponentListener
                             && x.FilePath == register.FilePath
                             && x.ClassName == register.ClassName))
            {
                message = "Component listener auto unregisters via OnDisable.";
                hasUnregister = true;
                return EventLifecycleSafetyStatus.ActiveBoundDisable;
            }

            message = register.HandlerName == "Lambda"
                ? "Lambda registration without lifecycle binding. Verify manual unregister."
                : "No lifecycle binding or matching unregister found in static scan.";
            return EventLifecycleSafetyStatus.ActiveOrphanRisk;
        }

        private static bool SameChannel(EventChannelKey a, EventChannelKey b)
            => a.RegisterType == b.RegisterType
               && a.ArgType == b.ArgType
               && (a.Identifier == b.Identifier || a.Identifier == "*" || b.Identifier == "*");

        private static EventStaticScanResult BuildResult(List<EventStaticScanEntry> entries, int scannedFiles)
        {
            var registers = entries
                .Where(x => x.Kind == EventStaticEntryKind.Register || x.Kind == EventStaticEntryKind.ComponentListener)
                .ToList();

            var channels = registers
                .GroupBy(x => x.ChannelKey)
                .Select(group =>
                {
                    var subscriptions = group.Select(item => new EventSubscriptionSnapshot(
                        item.Id,
                        item.HandlerName,
                        item.ClassName,
                        null,
                        true,
                        item.LifecycleBind,
                        null,
                        item.SafetyStatus,
                        item.RegisterStackTrace,
                        item.HasMatchingUnregister)).ToList();

                    return new EventChannelSnapshot(group.Key, subscriptions.Count, subscriptions);
                })
                .OrderBy(x => x.Key.DisplayName)
                .ToList();

            var lifecycleReports = registers.Select(item => new EventLifecycleReport(
                item.Id,
                item.ChannelKey,
                item.HandlerName,
                item.ClassName,
                null,
                true,
                item.LifecycleBind,
                null,
                item.SafetyStatus,
                item.HasMatchingUnregister,
                item.RegisterStackTrace,
                item.UnregisterLocation,
                DateTime.Now,
                null,
                item.AnalysisMessage)).ToList();

            var history = entries.Select(item => new EventDiagnosticRecord(
                item.Id,
                item.Kind switch
                {
                    EventStaticEntryKind.Register => EventDiagnosticAction.Register,
                    EventStaticEntryKind.Unregister => EventDiagnosticAction.UnRegister,
                    EventStaticEntryKind.ComponentListener => EventDiagnosticAction.LifecycleBind,
                    EventStaticEntryKind.Send => EventDiagnosticAction.Register,
                    _ => EventDiagnosticAction.Register
                },
                item.ChannelKey,
                item.HandlerName,
                item.ClassName,
                item.LifecycleBind,
                item.RegisterStackTrace)).ToList();

            return new EventStaticScanResult
            {
                Entries = entries,
                Channels = channels,
                LifecycleReports = lifecycleReports,
                History = history,
                TotalRegisterCount = registers.Count,
                RiskCount = registers.Count(x => x.SafetyStatus == EventLifecycleSafetyStatus.ActiveOrphanRisk),
                ScanTime = DateTime.Now,
                ScannedFileCount = scannedFiles
            };
        }

        private static EventChannelKey BuildChannelKey(string eventTypeName, EventRegisterType registerType, string args, out string handlerName)
        {
            var argType = ResolveType(eventTypeName);
            args = args?.Trim() ?? string.Empty;

            if (args.StartsWith("\"", StringComparison.Ordinal))
            {
                var name = ExtractStringLiteral(args);
                handlerName = ExtractFirstArg(args[(name.Length + 2)..].TrimStart(',', ' '));
                return EventChannelKey.ForString(argType, name);
            }

            var firstArg = ExtractFirstArg(args);
            var secondArg = ExtractSecondArg(args);
            if (!string.IsNullOrEmpty(secondArg) && firstArg.Contains('.', StringComparison.Ordinal))
            {
                handlerName = secondArg;
                return EventChannelKey.ForEnum(argType, null);
            }

            handlerName = firstArg;
            return registerType == EventRegisterType.AsyncType
                ? EventChannelKey.ForAsyncType(argType)
                : EventChannelKey.ForType(argType);
        }

        private static EventLifecycleBindType DetectLifecycleBind(string statement)
        {
            if (statement.Contains("UnRegisterWaitGameObjectDestroy", StringComparison.Ordinal))
                return EventLifecycleBindType.GameObjectDestroy;
            if (statement.Contains("UnRegisterWaitGameObjectDisable", StringComparison.Ordinal))
                return EventLifecycleBindType.GameObjectDisable;
            if (statement.Contains("UnRegisterWaitSceneUnLoad", StringComparison.Ordinal))
                return EventLifecycleBindType.SceneUnload;
            return EventLifecycleBindType.None;
        }

        private static Type ResolveType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;

                try
                {
                    type = assembly.GetTypes().FirstOrDefault(x => x.Name == typeName);
                    if (type != null)
                        return type;
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }

            return typeof(IEventArgs);
        }

        private static string ExtractSendEventType(string statement)
        {
            var match = Regex.Match(statement, @"Send(?:Event|_Task|_Async)?\s*<\s*(?<type>[\w\.]+)\s*>");
            return match.Success ? match.Groups["type"].Value : null;
        }

        private static string ExtractArgs(string statement, string typeName)
        {
            var marker = $"<{typeName}>(";
            var start = statement.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0)
                return string.Empty;

            start += marker.Length;
            var depth = 1;
            for (var i = start; i < statement.Length; i++)
            {
                var c = statement[i];
                if (c == '(') depth++;
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0)
                        return statement.Substring(start, i - start);
                }
            }

            return string.Empty;
        }

        private static string ExtractFirstArg(string args)
        {
            args = args.Trim();
            if (string.IsNullOrEmpty(args))
                return "Unknown";
            if (args.Contains("=>", StringComparison.Ordinal))
                return "Lambda";

            var comma = FindTopLevelComma(args);
            var first = (comma >= 0 ? args[..comma] : args).Trim();
            if (first.StartsWith("@", StringComparison.Ordinal))
                first = first[1..];
            return first;
        }

        private static string ExtractSecondArg(string args)
        {
            args = args.Trim();
            var comma = FindTopLevelComma(args);
            if (comma < 0)
                return null;

            var second = args[(comma + 1)..].Trim();
            comma = FindTopLevelComma(second);
            return comma >= 0 ? second[..comma].Trim() : second;
        }

        private static string ExtractStringLiteral(string args)
        {
            var end = args.IndexOf('"', 1);
            return end > 0 ? args.Substring(1, end - 1) : args.Trim('"');
        }

        private static int FindTopLevelComma(string text)
        {
            var depth = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '(' || c == '<') depth++;
                else if (c == ')' || c == '>') depth--;
                else if (c == ',' && depth == 0) return i;
            }

            return -1;
        }

        private static int FindStatementStart(string text, int index)
        {
            for (var i = index; i >= 0; i--)
            {
                if (text[i] == ';' || text[i] == '{')
                    return i + 1;
            }

            return 0;
        }

        private static string ExtractStatement(string text, int start)
        {
            start = Math.Max(0, Math.Min(start, text.Length - 1));
            var depth = 0;
            var started = false;
            for (var i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (!started && char.IsWhiteSpace(c))
                    continue;

                started = true;
                if (c == '(') depth++;
                else if (c == ')') depth--;
                else if (c == ';' && depth <= 0)
                    return text.Substring(start, i - start + 1);
            }

            return text.Substring(start);
        }

        private static string ExtractContainingClass(string text, int index)
        {
            var head = text[..Math.Min(index, text.Length)];
            return ClassRegex.Matches(head).Cast<Match>().LastOrDefault()?.Groups["name"].Value;
        }

        private static string ExtractContainingMethod(string text, int index)
        {
            var head = text[..Math.Min(index, text.Length)];
            var skip = new HashSet<string> { "if", "for", "foreach", "while", "switch", "catch", "using", "return", "new" };
            return MethodRegex.Matches(head).Cast<Match>()
                .Select(x => x.Groups["name"].Value)
                .Where(x => !skip.Contains(x))
                .LastOrDefault();
        }

        private static int GetLineNumber(string text, int index)
            => text[..Math.Min(index, text.Length)].Count(c => c == '\n') + 1;
    }
}
#endif
