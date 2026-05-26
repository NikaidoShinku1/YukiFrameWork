using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YukiFramework.AddressableExtension.Editor
{
    [CreateAssetMenu(
        fileName = "ResourcesConditionerRuleSet",
        menuName = "YukiFrameWork/Addressable/Resources Conditioner Rule Set")]
    public sealed class ResourcesConditionerRuleSet : ScriptableObject
    {
        public const string DefaultAssetPath =
            "Assets/YukiFramework/AddressableExtension/Editor/Data/ResourcesConditionerRuleSet.asset";

        public const string DefaultGeneratedOutputFolder =
            "Assets/YukiFramework/AddressableExtension/Runtime";

        public const string DefaultGeneratedFileClassName = "GeneratedResourcesConditioners";

        public const string DefaultGeneratedFilePath =
            DefaultGeneratedOutputFolder + "/" + DefaultGeneratedFileClassName + ".cs";

        public const string DefaultGeneratedNamespace = "YukiFrameWork.AddressExtension";

        [SerializeField] private List<ResourcesConditionerRuleData> rules = new List<ResourcesConditionerRuleData>();

        [Header("代码生成")]
        [SerializeField] private string generatedOutputFolder = DefaultGeneratedOutputFolder;
        [SerializeField] private string generatedFileClassName = DefaultGeneratedFileClassName;
        [SerializeField] private string generatedNamespace = DefaultGeneratedNamespace;

        [SerializeField] private string generatedFilePath;

        public IReadOnlyList<ResourcesConditionerRuleData> Rules => rules;

        public List<ResourcesConditionerRuleData> RulesMutable => rules;

        public string GeneratedOutputFolder
        {
            get
            {
                MigrateGenerationPathsIfNeeded();
                return string.IsNullOrWhiteSpace(generatedOutputFolder)
                    ? DefaultGeneratedOutputFolder
                    : generatedOutputFolder.Trim().TrimEnd('/');
            }
        }

        public string GeneratedFileClassName
        {
            get
            {
                MigrateGenerationPathsIfNeeded();
                return string.IsNullOrWhiteSpace(generatedFileClassName)
                    ? DefaultGeneratedFileClassName
                    : generatedFileClassName.Trim();
            }
        }

        public string GeneratedFilePath => CombineOutputPath(GeneratedOutputFolder, GeneratedFileClassName);

        public string GeneratedNamespace =>
            string.IsNullOrWhiteSpace(generatedNamespace) ? DefaultGeneratedNamespace : generatedNamespace.Trim();

        public static string CombineOutputPath(string folder, string className)
        {
            var safeFolder = string.IsNullOrWhiteSpace(folder) ? DefaultGeneratedOutputFolder : folder.Trim().TrimEnd('/');
            var safeName = string.IsNullOrWhiteSpace(className) ? DefaultGeneratedFileClassName : className.Trim();
            if (!safeName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                safeName += ".cs";
            return safeFolder + "/" + safeName;
        }

        public void MigrateGenerationPathsIfNeeded()
        {
            if (!string.IsNullOrWhiteSpace(generatedOutputFolder) &&
                !string.IsNullOrWhiteSpace(generatedFileClassName))
                return;

            if (!string.IsNullOrWhiteSpace(generatedFilePath))
            {
                var legacy = generatedFilePath.Replace('\\', '/').Trim();
                generatedOutputFolder = Path.GetDirectoryName(legacy)?.Replace('\\', '/');
                generatedFileClassName = Path.GetFileNameWithoutExtension(legacy);
                generatedFilePath = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(generatedOutputFolder))
                generatedOutputFolder = DefaultGeneratedOutputFolder;
            if (string.IsNullOrWhiteSpace(generatedFileClassName))
                generatedFileClassName = DefaultGeneratedFileClassName;
        }

        public void ResetGenerationDefaults()
        {
            generatedOutputFolder = DefaultGeneratedOutputFolder;
            generatedFileClassName = DefaultGeneratedFileClassName;
            generatedNamespace = DefaultGeneratedNamespace;
            generatedFilePath = string.Empty;
            MarkDirty();
        }

        public void SetRules(List<ResourcesConditionerRuleData> newRules)
        {
            rules = newRules ?? new List<ResourcesConditionerRuleData>();
            MarkDirty();
        }

        public void MarkDirty()
        {
            EditorUtility.SetDirty(this);
        }
    }

    [Serializable]
    public sealed class ResourcesConditionerRuleData
    {
        public bool enabled = true;
        public string rulePath = string.Empty;
        public string suffix = string.Empty;
        public string className = string.Empty;
        public string sourceGroup = string.Empty;
        public string note = string.Empty;

        [Tooltip("勾选后，扫描 Addressables 时不会覆盖类名、RulePath 与后缀")]
        public bool lockCustomNaming;

        public string RuleKey => ResConditionerScanner.BuildRuleKey(rulePath, suffix);

        public string PreviewPath(string objectName = "Example")
        {
            if (string.IsNullOrEmpty(objectName))
                objectName = "Example";

            var path = rulePath ?? string.Empty;
            if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
                path += "/";
            path += objectName;

            if (!string.IsNullOrEmpty(suffix))
                path += suffix.StartsWith(".") ? suffix : "." + suffix;

            return path;
        }

        public ResourcesConditionerRuleData Clone()
        {
            return new ResourcesConditionerRuleData
            {
                enabled = enabled,
                rulePath = rulePath,
                suffix = suffix,
                className = className,
                sourceGroup = sourceGroup,
                note = note,
                lockCustomNaming = lockCustomNaming
            };
        }
    }
}
