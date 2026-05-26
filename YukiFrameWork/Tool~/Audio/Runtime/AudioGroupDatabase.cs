///=====================================================
/// - FileName:      AudioGroupDatabase.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   音频分组配置与代码生成数据
/// - Creation Time: 2025/5/26
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YukiFrameWork.Audio
{
    public enum AudioCodeGenPlayStyle
    {
        [LabelText("Build().Play()")]
        BuildPlay = 0,
        [LabelText("Play() 简写")]
        Play = 1,
        [LabelText("PlayAsync()")]
        PlayAsync = 3,
        [LabelText("BuildAsync() 回调")]
        BuildAsyncCallback = 4
    }

    [Serializable]
    public class AudioEntryData
    {
        [HideInInspector]
        public string clipGuid;

        [LabelText("音频资源"), Required]
        public AudioClip clip;

        [LabelText("加载名称"), InfoBox("AudioKit.Build/Play 使用的名称，默认取 Clip.name")]
        public string assetName;

        [LabelText("代码标识"), InfoBox("生成代码中的常量/方法名，留空则自动从 Clip 名转换")]
        public string codeKey;

        [LabelText("默认循环"), Tooltip("AudioPlayer.Loop()")]
        public bool loopByDefault;

        [LabelText("间隔播放"), Tooltip("AudioPlayer.Interval()，仅 Sound 层生效")]
        public bool useInterval;

        [LabelText("异步播放"), Tooltip("AudioGroup.PlayAsync()")]
        public bool useAsync;

        [LabelText("真实时间"), Tooltip("AudioPlayer.IsRealTime()，不受 Time.timeScale 影响")]
        public bool useRealTime;

        [LabelText("绑定父节点"), Tooltip("生成代码时传入 Transform parent，并调用 AudioPlayer.Parent()")]
        public bool bindParent;

        [LabelText("3D 音效"), Tooltip("AudioPlayer.AudioSourceSoundSetting()")]
        public bool use3DSetting;

        [LabelText("3D 参数外部传入"), Tooltip("生成代码时通过 AudioSourceSoundSetting 参数传入，而非使用下方本地配置")]
        public bool external3DSetting;

        [LabelText("3D 参数"), ShowIf("@use3DSetting && !external3DSetting")]
        public AudioSourceSoundSetting soundSetting = new AudioSourceSoundSetting();

        [LabelText("异步风格"), ShowIf(nameof(useAsync))]
        public AudioCodeGenPlayStyle asyncPlayStyle = AudioCodeGenPlayStyle.PlayAsync;

        [LabelText("同步风格"), HideIf(nameof(useAsync))]
        public AudioCodeGenPlayStyle playStyle = AudioCodeGenPlayStyle.BuildPlay;

        [HideInInspector] public bool expandedInEditor;

        public string ResolvedAssetName => assetName.IsNullOrEmpty()
            ? clip ? clip.name : string.Empty
            : assetName;

        public string GetDisplayName()
        {
            if (clip) return clip.name;
            return codeKey.IsNullOrEmpty() ? "未命名音频" : codeKey;
        }

        internal void SyncFromClip()
        {
            if (!clip) return;
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(clip);
            if (!path.IsNullOrEmpty())
                clipGuid = AssetDatabase.AssetPathToGUID(path);
#endif
            if (assetName.IsNullOrEmpty())
                assetName = clip.name;
            if (codeKey.IsNullOrEmpty())
                codeKey = AudioGroupDatabase.SanitizeCodeKey(clip.name);
            NormalizePlayStyle();
        }

        /// <summary>迁移已移除的 BuildLoopPlay(=2) 为 BuildPlay + 默认循环。</summary>
        internal void NormalizePlayStyle()
        {
            if ((int)playStyle != 2) return;
            playStyle = AudioCodeGenPlayStyle.BuildPlay;
            loopByDefault = true;
        }

        internal void CopyPlaybackRulesFrom(AudioEntryData source)
        {
            if (source == null) return;
            loopByDefault = source.loopByDefault;
            useInterval = source.useInterval;
            useRealTime = source.useRealTime;
            bindParent = source.bindParent;
            use3DSetting = source.use3DSetting;
            external3DSetting = source.external3DSetting;
            useAsync = source.useAsync;
            playStyle = source.playStyle;
            asyncPlayStyle = source.asyncPlayStyle;
            NormalizePlayStyle();
            if (source.soundSetting != null)
            {
                soundSetting ??= new AudioSourceSoundSetting();
                AudioGroupSharedRuleSettings.CopySoundSetting(source.soundSetting, soundSetting);
            }
        }

        internal void ApplyPlaybackRulesFrom(AudioGroupSharedRuleSettings rules)
        {
            if (rules == null) return;
            loopByDefault = rules.loopByDefault;
            useInterval = rules.useInterval;
            useRealTime = rules.useRealTime;
            bindParent = rules.bindParent;
            use3DSetting = rules.use3DSetting;
            external3DSetting = rules.external3DSetting;
            useAsync = rules.useAsync;
            playStyle = rules.playStyle;
            asyncPlayStyle = rules.asyncPlayStyle;
            NormalizePlayStyle();
            if (rules.soundSetting != null)
            {
                soundSetting ??= new AudioSourceSoundSetting();
                AudioGroupSharedRuleSettings.CopySoundSetting(rules.soundSetting, soundSetting);
            }
        }
    }

    [Serializable]
    public class AudioGroupSharedRuleSettings
    {
        [LabelText("启用通用设置")]
        public bool enabled;

        [LabelText("默认循环")]
        public bool loopByDefault;

        [LabelText("间隔播放")]
        public bool useInterval;

        [LabelText("真实时间")]
        public bool useRealTime;

        [LabelText("绑定父节点")]
        public bool bindParent;

        [LabelText("3D 音效")]
        public bool use3DSetting;

        [LabelText("3D 参数外部传入")]
        public bool external3DSetting;

        [LabelText("3D 参数"), ShowIf("@use3DSetting && !external3DSetting")]
        public AudioSourceSoundSetting soundSetting = new AudioSourceSoundSetting();

        [LabelText("异步播放")]
        public bool useAsync;

        [LabelText("异步风格"), ShowIf(nameof(useAsync))]
        public AudioCodeGenPlayStyle asyncPlayStyle = AudioCodeGenPlayStyle.PlayAsync;

        [LabelText("同步风格"), HideIf(nameof(useAsync))]
        public AudioCodeGenPlayStyle playStyle = AudioCodeGenPlayStyle.BuildPlay;

        public AudioCodeGenPlayStyle GetPlayStyle() => useAsync ? asyncPlayStyle : playStyle;

        public void SetPlayStyle(AudioCodeGenPlayStyle style)
        {
            if (IsAsyncPlayStyle(style))
            {
                useAsync = true;
                asyncPlayStyle = style;
            }
            else
            {
                useAsync = false;
                playStyle = style;
            }
        }

        public void CopyFrom(AudioEntryData entry)
        {
            if (entry == null) return;
            loopByDefault = entry.loopByDefault;
            useInterval = entry.useInterval;
            useRealTime = entry.useRealTime;
            bindParent = entry.bindParent;
            use3DSetting = entry.use3DSetting;
            external3DSetting = entry.external3DSetting;
            useAsync = entry.useAsync;
            playStyle = entry.playStyle;
            asyncPlayStyle = entry.asyncPlayStyle;
            if (entry.soundSetting != null)
            {
                soundSetting ??= new AudioSourceSoundSetting();
                CopySoundSetting(entry.soundSetting, soundSetting);
            }
        }

        public void ApplyTo(AudioEntryData entry)
        {
            entry?.ApplyPlaybackRulesFrom(this);
        }

        internal static bool IsAsyncPlayStyle(AudioCodeGenPlayStyle style) =>
            style is AudioCodeGenPlayStyle.PlayAsync or AudioCodeGenPlayStyle.BuildAsyncCallback;

        internal static void CopySoundSetting(AudioSourceSoundSetting from, AudioSourceSoundSetting to)
        {
            if (from == null || to == null) return;
            to.Priority = from.Priority;
            to.Pitch = from.Pitch;
            to.StereoPan = from.StereoPan;
            to.SpatitalBlend = from.SpatitalBlend;
            to.ReverbZoneMix = from.ReverbZoneMix;
            to.DopplerLevel = from.DopplerLevel;
            to.Spread = from.Spread;
            to.VolumeRolloff = from.VolumeRolloff;
            to.MinDistance = from.MinDistance;
            to.MaxDistance = from.MaxDistance;
        }
    }

    [Serializable]
    public class AudioGroupData
    {
        [HideInInspector]
        public string id = Guid.NewGuid().ToString();

        [HideInInspector]
        public string displayName = "新分组";

        [LabelText("播放层级")]
        public AudioPlayType playType = AudioPlayType.Sound;

        [LabelText("组名"), InfoBox("留空则使用 AudioKit 默认分组 (Music()/Voice()/Sound())")]
        public string groupName;

        [LabelText("代码标识"), InfoBox("生成代码中分组属性的名称，留空则自动生成")]
        public string codeKey;

        [LabelText("默认播放风格")]
        public AudioCodeGenPlayStyle defaultPlayStyle = AudioCodeGenPlayStyle.BuildPlay;

        [LabelText("通用播放规则")]
        public AudioGroupSharedRuleSettings sharedRules = new AudioGroupSharedRuleSettings();

        [LabelText("分组内音频")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        public List<AudioEntryData> entries = new List<AudioEntryData>();

        public string GetDisplayName()
        {
            if (!displayName.IsNullOrEmpty()) return displayName;
            if (!groupName.IsNullOrEmpty()) return groupName;
            return $"{playType}(默认)";
        }

        public string ResolvedCodeKey =>
            codeKey.IsNullOrEmpty() ? AudioGroupDatabase.BuildGroupCodeKey(playType, groupName) : codeKey;

        public int ApplySharedRulesToAllEntries()
        {
            if (sharedRules == null || !sharedRules.enabled || entries == null) return 0;
            var count = 0;
            foreach (var entry in entries)
            {
                if (entry == null) continue;
                sharedRules.ApplyTo(entry);
                count++;
            }
            return count;
        }
    }

    [CreateAssetMenu(fileName = "AudioGroupDatabase", menuName = "YukiFrameWork/Audio Group Database")]
    [HideMonoScript]
    public class AudioGroupDatabase : ScriptableObject
    {
        [FoldoutGroup("扫描设置"), LabelText("扫描目录")]
        [FolderPath(ParentFolder = "Assets")]
        public string[] scanFolders = { "Assets" };

        [FoldoutGroup("扫描设置"), LabelText("排除目录关键字")]
        [InfoBox("路径中包含这些关键字的音频将被忽略，例如 Plugins、Editor")]
        public string[] excludePathKeywords = { "/Editor/", "/Plugins/", "/Gizmos/" };

        [FoldoutGroup("分组配置"), LabelText("音频分组")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, Expanded = true)]
        public List<AudioGroupData> groups = new List<AudioGroupData>();

        [FoldoutGroup("代码设置"), LabelText("类名")]
        public string codeClassName = "GameAudios";

        [FoldoutGroup("代码设置"), LabelText("输出路径"), FolderPath]
        public string codeFilePath = FrameworkDefaultPath;

        [FoldoutGroup("代码设置"), LabelText("命名空间")]
        public string nameSpace = FrameworkDefaultNamespace;

        public const string FrameworkDefaultPath = "Assets/Scripts";
        public const string FrameworkDefaultNamespace = "YukiFrameWork.Audio";

        public Action onValidate;

#if UNITY_EDITOR
        [HideInInspector, SerializeField]
        public int selectIndex;

        [Button("扫描项目音频", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f), FoldoutGroup("扫描设置")]
        void ScanProjectAudio()
        {
            AudioGroupDatabaseEditorWindow.ShowWindow();
            if (AudioGroupDatabaseEditorWindow.Instance)
                AudioGroupDatabaseEditorWindow.Instance.SetDatabase(this);
            AudioGroupDatabaseEditorWindow.Instance?.ScanAudioClips();
        }

        [Button("生成 AudioKit 调用代码", ButtonSizes.Large), GUIColor(0.3f, 0.9f, 0.4f), FoldoutGroup("代码设置")]
        void GenerateCode()
        {
            AudioGroupCodeGenerator.Generate(this);
        }

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(insId) as AudioGroupDatabase;
            if (obj != null)
            {
                AudioGroupDatabaseEditorWindow.ShowWindow();
                AudioGroupDatabaseEditorWindow.Instance?.SetDatabase(obj);
            }
            return obj != null;
        }

        public static IEnumerable<ValueDropdownItem<string>> AllGroupCodeKeys =>
            YukiAssetDataBase.FindAssets<AudioGroupDatabase>()
                .SelectMany(db => db.groups)
                .Select(g => new ValueDropdownItem<string>(g.GetDisplayName(), g.ResolvedCodeKey));
#endif

        public HashSet<string> GetAssignedClipGuids()
        {
            var set = new HashSet<string>();
            foreach (var group in groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.clip)
                    {
#if UNITY_EDITOR
                        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entry.clip));
                        if (!guid.IsNullOrEmpty())
                            set.Add(guid);
#endif
                    }
                    else if (!entry.clipGuid.IsNullOrEmpty())
                        set.Add(entry.clipGuid);
                }
            }
            return set;
        }

        public AudioGroupData FindGroupById(string id) => groups.FirstOrDefault(g => g.id == id);

        public AudioGroupData AddGroup(AudioPlayType playType = AudioPlayType.Sound, string groupName = null)
        {
            var group = new AudioGroupData
            {
                playType = playType,
                groupName = groupName ?? string.Empty,
                displayName = groupName.IsNullOrEmpty() ? $"{playType}(默认)" : groupName
            };
            groups.Add(group);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            onValidate?.Invoke();
            return group;
        }

        public void RemoveGroup(AudioGroupData group)
        {
            groups.Remove(group);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            onValidate?.Invoke();
        }

        public bool AssignClipToGroup(AudioGroupData group, AudioClip clip)
        {
            if (group == null || clip == null) return false;
#if UNITY_EDITOR
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
            if (GetAssignedClipGuids().Contains(guid)) return false;
#else
            return false;
#endif

            var entry = new AudioEntryData { clip = clip };
            entry.SyncFromClip();
            if (group.sharedRules != null && group.sharedRules.enabled)
                group.sharedRules.ApplyTo(entry);
            group.entries.Add(entry);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            onValidate?.Invoke();
            return true;
        }

        public void RemoveEntry(AudioGroupData group, AudioEntryData entry)
        {
            if (group == null || entry == null) return;
            group.entries.Remove(entry);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            onValidate?.Invoke();
        }

        public static string SanitizeCodeKey(string raw)
        {
            if (raw.IsNullOrEmpty()) return "Unnamed";
            var chars = raw.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray();
            var result = new string(chars);
            if (char.IsDigit(result[0]))
                result = "_" + result;
            return result;
        }

        public static string BuildGroupCodeKey(AudioPlayType playType, string groupName)
        {
            if (groupName.IsNullOrEmpty())
                return playType.ToString();
            return $"{playType}_{SanitizeCodeKey(groupName)}";
        }

        public string GetGroupAccessorExpression(AudioGroupData group)
        {
            var method = group.playType switch
            {
                AudioPlayType.Music => "Music",
                AudioPlayType.Voice => "Voice",
                AudioPlayType.Sound => "Sound",
                _ => "Sound"
            };
            return group.groupName.IsNullOrEmpty()
                ? $"AudioKit.{method}()"
                : $"AudioKit.{method}(\"{group.groupName}\")";
        }
    }
}
