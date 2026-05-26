using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace YukiFramework.AddressableExtension.Editor
{
    public static class ResConditionerScanner
    {
        public sealed class ScannedEntry
        {
            public string Address;
            public string AssetPath;
            public string GroupName;
            public string ObjectName;
            public string RulePath;
            public string Suffix;
            public string RuleKey;
        }

        public static List<ResourcesConditionerRuleData> ScanRules(AddressableAssetSettings settings)
        {
            var map = new Dictionary<string, ResourcesConditionerRuleData>(StringComparer.OrdinalIgnoreCase);
            var index = 0;

            foreach (var group in settings.groups)
            {
                if (group == null) continue;

                var groupName = NormalizeGroupName(group.Name);
                foreach (var entry in group.entries)
                {
                    if (entry == null) continue;

                    foreach (var address in CollectEntryAddresses(entry))
                    {
                        var fullAddress = NormalizeAddress(address);
                        if (string.IsNullOrEmpty(fullAddress)) continue;

                        var ext = NormalizeSuffix(Path.GetExtension(fullAddress));
                        if (string.IsNullOrEmpty(ext))
                        {
                            var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
                            ext = NormalizeSuffix(Path.GetExtension(assetPath));
                        }

                        if (string.IsNullOrEmpty(ext)) continue;

                        var rulePath = NormalizeRulePath(Path.GetDirectoryName(fullAddress));
                        if (string.IsNullOrEmpty(rulePath))
                            rulePath = NormalizeRulePath(GetEntryAssetDirectory(entry));

                        var key = BuildRuleKey(rulePath, ext);
                        if (map.ContainsKey(key))
                            continue;

                        map[key] = new ResourcesConditionerRuleData
                        {
                            enabled = true,
                            rulePath = rulePath,
                            suffix = ext,
                            className = BuildClassName(groupName, rulePath, ext, index++),
                            sourceGroup = groupName
                        };
                    }
                }
            }

            return map.Values
                .OrderBy(x => x.rulePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.suffix, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static void BuildMatchIndex(
            AddressableAssetSettings settings,
            Dictionary<string, int> countByRuleKey,
            Dictionary<string, List<ScannedEntry>> entriesByRuleKey)
        {
            if (settings == null || countByRuleKey == null || entriesByRuleKey == null)
                return;

            foreach (var group in settings.groups)
            {
                if (group == null) continue;

                var groupName = NormalizeGroupName(group.Name);
                foreach (var entry in group.entries)
                {
                    if (entry == null) continue;

                    foreach (var address in CollectEntryAddresses(entry))
                    {
                        if (!TryCreateScannedEntry(entry, groupName, address, out var scanned))
                            continue;

                        if (!countByRuleKey.ContainsKey(scanned.RuleKey))
                        {
                            countByRuleKey[scanned.RuleKey] = 0;
                            entriesByRuleKey[scanned.RuleKey] = new List<ScannedEntry>();
                        }

                        countByRuleKey[scanned.RuleKey]++;
                        entriesByRuleKey[scanned.RuleKey].Add(scanned);
                    }
                }
            }

            foreach (var list in entriesByRuleKey.Values)
            {
                list.Sort((a, b) => string.Compare(a.Address, b.Address, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static List<ScannedEntry> CollectEntriesForRule(
            AddressableAssetSettings settings,
            ResourcesConditionerRuleData rule)
        {
            if (settings == null || rule == null)
                return new List<ScannedEntry>();

            var countMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var entryMap = new Dictionary<string, List<ScannedEntry>>(StringComparer.OrdinalIgnoreCase);
            BuildMatchIndex(settings, countMap, entryMap);
            return entryMap.TryGetValue(rule.RuleKey, out var list)
                ? list
                : new List<ScannedEntry>();
        }

        private static bool TryCreateScannedEntry(
            AddressableAssetEntry entry,
            string groupName,
            string address,
            out ScannedEntry scanned)
        {
            scanned = null;
            var fullAddress = NormalizeAddress(address);
            if (string.IsNullOrEmpty(fullAddress))
                return false;

            var ext = NormalizeSuffix(Path.GetExtension(fullAddress));
            if (string.IsNullOrEmpty(ext))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
                ext = NormalizeSuffix(Path.GetExtension(assetPath));
            }

            if (string.IsNullOrEmpty(ext))
                return false;

            var rulePath = NormalizeRulePath(Path.GetDirectoryName(fullAddress));
            if (string.IsNullOrEmpty(rulePath))
                rulePath = NormalizeRulePath(GetEntryAssetDirectory(entry));

            var ruleKey = BuildRuleKey(rulePath, ext);
            scanned = new ScannedEntry
            {
                Address = fullAddress,
                AssetPath = AssetDatabase.GUIDToAssetPath(entry.guid),
                GroupName = groupName,
                ObjectName = Path.GetFileNameWithoutExtension(fullAddress),
                RulePath = rulePath,
                Suffix = ext,
                RuleKey = ruleKey
            };
            return true;
        }

        public static void MergeScannedRules(
            ResourcesConditionerRuleSet ruleSet,
            List<ResourcesConditionerRuleData> scanned,
            bool replaceExistingKeys)
        {
            if (ruleSet == null)
                return;

            var current = ruleSet.RulesMutable;
            var map = new Dictionary<string, ResourcesConditionerRuleData>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in current)
            {
                if (rule == null || string.IsNullOrEmpty(rule.RuleKey))
                    continue;
                map[rule.RuleKey] = rule;
            }

            foreach (var scannedRule in scanned)
            {
                if (scannedRule == null)
                    continue;

                if (map.TryGetValue(scannedRule.RuleKey, out var existing))
                {
                    if (existing.lockCustomNaming)
                    {
                        if (string.IsNullOrEmpty(existing.sourceGroup))
                            existing.sourceGroup = scannedRule.sourceGroup;
                    }
                    else if (replaceExistingKeys)
                    {
                        existing.rulePath = scannedRule.rulePath;
                        existing.suffix = scannedRule.suffix;
                        existing.sourceGroup = scannedRule.sourceGroup;
                        if (string.IsNullOrEmpty(existing.className))
                            existing.className = scannedRule.className;
                    }
                    else if (string.IsNullOrEmpty(existing.sourceGroup))
                    {
                        existing.sourceGroup = scannedRule.sourceGroup;
                    }

                    continue;
                }

                current.Add(scannedRule.Clone());
                map[scannedRule.RuleKey] = scannedRule;
            }

            ruleSet.MarkDirty();
        }

        public static string BuildRuleKey(string rulePath, string suffix)
        {
            return NormalizeRulePath(rulePath) + "|" + NormalizeSuffix(suffix);
        }

        private static IEnumerable<string> CollectEntryAddresses(AddressableAssetEntry entry)
        {
            if (entry == null)
                yield break;

            if (!entry.IsFolder)
            {
                var normalizedAddress = NormalizeAddress(entry.address);
                if (!string.IsNullOrEmpty(normalizedAddress))
                {
                    yield return normalizedAddress;
                    yield break;
                }

                var assetPath = NormalizeAddress(AssetDatabase.GUIDToAssetPath(entry.guid));
                if (!string.IsNullOrEmpty(assetPath))
                    yield return assetPath;
                yield break;
            }

            var folderPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
                yield break;

            var folderAddressPrefix = NormalizeAddress(entry.address);
            var guids = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath) || Directory.Exists(assetPath))
                    continue;

                if (assetPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) ||
                    assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                var relative = GetRelativeAssetPath(folderPath, assetPath);
                if (string.IsNullOrEmpty(relative))
                    continue;

                if (string.IsNullOrEmpty(folderAddressPrefix))
                    yield return relative;
                else
                    yield return folderAddressPrefix.TrimEnd('/') + "/" + relative.TrimStart('/');
            }
        }

        public static string NormalizeRulePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.Replace('\\', '/').Trim('/');
        }

        public static string NormalizeAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return string.Empty;

            var normalized = address.Replace('\\', '/').Trim();

            var queryIndex = normalized.IndexOf('?');
            if (queryIndex >= 0)
                normalized = normalized.Substring(0, queryIndex);

            var hashIndex = normalized.IndexOf('#');
            if (hashIndex >= 0)
                normalized = normalized.Substring(0, hashIndex);

            return normalized.Trim('/');
        }

        public static string NormalizeSuffix(string ext)
        {
            if (string.IsNullOrEmpty(ext)) return string.Empty;
            return ext.Trim().TrimStart('.').ToLowerInvariant();
        }

        private static string NormalizeGroupName(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                return "DefaultGroup";
            return groupName.Trim();
        }

        private static string GetRelativeAssetPath(string folderPath, string assetPath)
        {
            var root = folderPath.Replace('\\', '/').TrimEnd('/');
            var full = assetPath.Replace('\\', '/');
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return full.Substring(root.Length).TrimStart('/');
        }

        private static string GetEntryAssetDirectory(AddressableAssetEntry entry)
        {
            if (entry == null)
                return string.Empty;

            var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            if (string.IsNullOrEmpty(assetPath))
                return string.Empty;

            if (AssetDatabase.IsValidFolder(assetPath))
                return assetPath;

            return Path.GetDirectoryName(assetPath) ?? string.Empty;
        }

        public static string BuildClassName(string groupName, string rulePath, string suffix, int fallbackIndex)
        {
            var safeGroup = string.IsNullOrEmpty(groupName) ? "Group" : groupName;
            var safePath = string.IsNullOrEmpty(rulePath) ? "Root" : rulePath;
            var raw = $"{safeGroup}_{safePath}_{suffix}_Conditioner";
            var chars = raw.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray();
            var cleaned = new string(chars);
            var parts = cleaned.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            var pascal = string.Concat(parts.Select(ToPascal));

            if (string.IsNullOrEmpty(pascal))
                pascal = "GeneratedRule" + fallbackIndex;

            if (!char.IsLetter(pascal[0]) && pascal[0] != '_')
                pascal = "R_" + pascal;

            return pascal;
        }

        private static string ToPascal(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Length == 1) return value.ToUpperInvariant();
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }
    }
}
