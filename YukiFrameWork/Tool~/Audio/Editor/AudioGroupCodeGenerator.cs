///=====================================================
/// - FileName:      AudioGroupCodeGenerator.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   AudioKit 调用代码自动生成
/// - Creation Time: 2025/5/26
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YukiFrameWork;
using YukiFrameWork.Extension;

#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork.Audio
{
    public static class AudioGroupCodeGenerator
    {
        public static void Generate(AudioGroupDatabase database)
        {
            if (!database)
            {
                Debug.LogError("AudioGroupDatabase 为空");
                return;
            }
            if (database.codeClassName.IsNullOrEmpty())
            {
                Debug.LogError("请填写类名");
                return;
            }
            if (database.nameSpace.IsNullOrEmpty())
            {
                Debug.LogError("请填写命名空间");
                return;
            }

            foreach (var group in database.groups)
                foreach (var entry in group.entries)
                    entry.SyncFromClip();

            var writer = new CodeWriter();
            var groupKeys = new HashSet<string>();
            var nameKeys = new HashSet<string>();
            var methodKeys = new HashSet<string>();

            writer.CustomCode("#region 分组");
            foreach (var group in database.groups)
            {
                var key = Unique(groupKeys, group.ResolvedCodeKey);
                writer.CustomCode($"public static AudioGroup {key} => {database.GetGroupAccessorExpression(group)};");
            }
            writer.CustomCode("#endregion");
            writer.CustomCode(string.Empty);

            writer.CustomCode("#region 名称");
            foreach (var group in database.groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.ResolvedAssetName.IsNullOrEmpty()) continue;
                    var key = Unique(nameKeys, entry.codeKey);
                    writer.CustomCode($"public const string {key} = \"{entry.ResolvedAssetName}\";");
                }
            }
            writer.CustomCode("#endregion");
            writer.CustomCode(string.Empty);

            writer.CustomCode("#region 播放");
            foreach (var group in database.groups)
            {
                var groupKey = group.ResolvedCodeKey;
                foreach (var entry in group.entries)
                {
                    if (entry.ResolvedAssetName.IsNullOrEmpty()) continue;
                    var method = Unique(methodKeys, "Play_" + entry.codeKey);
                    var signature = BuildMethodSignature(entry, method);
                    var body = BuildPlayBody(groupKey, entry.codeKey, entry, group);
                    writer.CustomCode($"{signature} => {body};");
                }
            }
            writer.CustomCode("#endregion");

            new CodeCore()
                .Using("System")
                .Using("UnityEngine")
                .Using("YukiFrameWork.Audio")
                .Descripton(database.codeClassName, database.nameSpace, "AudioGroupDatabase 自动生成")
                .CodeSetting(database.nameSpace, database.codeClassName, string.Empty, writer, true)
                .Create(database.codeClassName, database.codeFilePath);

            Debug.Log($"已生成: {database.codeFilePath}/{database.codeClassName}.cs");
        }

        private static string BuildPlayBody(string groupKey, string nameKey, AudioEntryData entry, AudioGroupData group)
        {
            var parentParam = entry.bindParent ? "parent" : null;
            var callbackParam = entry.useAsync ? "onReady" : null;
            var soundSettingParam = entry.use3DSetting && entry.external3DSetting ? "soundSetting" : null;
            return AudioEntryPlayChain.BuildPlayExpression(
                groupKey, nameKey, entry, group, parentParam, callbackParam, soundSettingParam);
        }

        private static string BuildMethodSignature(AudioEntryData entry, string methodName)
        {
            var parameters = new StringBuilder();
            if (entry.bindParent)
                AppendParameter(parameters, "Transform parent");
            if (entry.use3DSetting && entry.external3DSetting)
                AppendParameter(parameters, "AudioSourceSoundSetting soundSetting");
            if (entry.useAsync)
                AppendParameter(parameters, "Action<AudioPlayer> onReady = null");

            var ret = entry.useAsync ? "void" : "AudioPlayer";
            return parameters.Length == 0
                ? $"public static {ret} {methodName}()"
                : $"public static {ret} {methodName}({parameters})";
        }

        private static void AppendParameter(StringBuilder parameters, string parameter)
        {
            if (parameters.Length > 0)
                parameters.Append(", ");
            parameters.Append(parameter);
        }

        private static string Unique(HashSet<string> used, string key)
        {
            var result = AudioGroupDatabase.SanitizeCodeKey(key);
            if (used.Add(result)) return result;
            var i = 1;
            while (!used.Add(result + i)) i++;
            return result + i;
        }
    }
}
#endif
