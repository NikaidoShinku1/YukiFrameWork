using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;

namespace YukiFramework.AddressableExtension.Editor
{
    /// <summary>
    /// Addressables 匹配索引，一次构建、多次查询，避免 OnGUI 每帧全量扫描。
    /// </summary>
    public sealed class ResConditionerMatchIndex
    {
        private readonly Dictionary<string, int> countByRuleKey =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, List<ResConditionerScanner.ScannedEntry>> entriesByRuleKey =
            new Dictionary<string, List<ResConditionerScanner.ScannedEntry>>(StringComparer.OrdinalIgnoreCase);

        public bool IsBuilt { get; private set; }

        public void Rebuild(AddressableAssetSettings settings)
        {
            countByRuleKey.Clear();
            entriesByRuleKey.Clear();
            IsBuilt = false;

            if (settings == null)
                return;

            ResConditionerScanner.BuildMatchIndex(settings, countByRuleKey, entriesByRuleKey);
            IsBuilt = true;
        }

        public int GetCount(string ruleKey)
        {
            if (string.IsNullOrEmpty(ruleKey))
                return 0;
            return countByRuleKey.TryGetValue(ruleKey, out var count) ? count : 0;
        }

        public int GetCount(ResourcesConditionerRuleData rule)
        {
            return rule == null ? 0 : GetCount(rule.RuleKey);
        }

        public List<ResConditionerScanner.ScannedEntry> GetEntries(string ruleKey)
        {
            if (string.IsNullOrEmpty(ruleKey))
                return new List<ResConditionerScanner.ScannedEntry>();

            if (entriesByRuleKey.TryGetValue(ruleKey, out var list))
                return list;

            return new List<ResConditionerScanner.ScannedEntry>();
        }

        public List<ResConditionerScanner.ScannedEntry> GetEntries(ResourcesConditionerRuleData rule)
        {
            return rule == null ? new List<ResConditionerScanner.ScannedEntry>() : GetEntries(rule.RuleKey);
        }
    }
}
