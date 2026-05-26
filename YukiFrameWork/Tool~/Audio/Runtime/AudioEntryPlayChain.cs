///=====================================================
/// - FileName:      AudioEntryPlayChain.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   AudioKit 播放链构建（与 AudioPlayer API 对标）
/// - Creation Time: 2025/5/26
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using System.Text;
using UnityEngine;

namespace YukiFrameWork.Audio
{
    public static class AudioEntryPlayChain
    {
        public static AudioCodeGenPlayStyle GetActivePlayStyle(AudioEntryData entry)
        {
            entry.NormalizePlayStyle();
            return entry.useAsync ? entry.asyncPlayStyle : entry.playStyle;
        }

        public static string BuildPlayExpression(
            string groupExpr,
            string nameExpr,
            AudioEntryData entry,
            AudioGroupData group,
            string parentParam = null,
            string callbackParam = null,
            string soundSettingParam = null)
        {
            entry.NormalizePlayStyle();
            if (entry.useAsync)
                return BuildAsyncExpression(groupExpr, nameExpr, entry, group, parentParam, callbackParam, soundSettingParam);

            var style = entry.playStyle;
            if (style == AudioCodeGenPlayStyle.Play && !HasPlayerChainModifiers(entry, group))
                return $"{groupExpr}.Play({nameExpr})";

            var chain = new StringBuilder();
            chain.Append(groupExpr).Append(".Build(").Append(nameExpr).Append(')');
            AppendPlayerChainModifiers(chain, entry, group, parentParam, soundSettingParam);
            chain.Append(".Play()");
            return chain.ToString();
        }

        public static string BuildAsyncExpression(string groupExpr, string nameExpr, AudioEntryData entry) =>
            BuildAsyncExpression(groupExpr, nameExpr, entry, null);

        public static string BuildAsyncExpression(
            string groupExpr,
            string nameExpr,
            AudioEntryData entry,
            AudioGroupData group,
            string parentParam = null,
            string callbackParam = null,
            string soundSettingParam = null)
        {
            entry.NormalizePlayStyle();
            var style = entry.asyncPlayStyle;
            if (style == AudioCodeGenPlayStyle.PlayAsync && !HasPlayerChainModifiers(entry, group))
            {
                if (!string.IsNullOrEmpty(callbackParam))
                    return $"{groupExpr}.PlayAsync({nameExpr}, {callbackParam})";
                return $"{groupExpr}.PlayAsync({nameExpr})";
            }

            var playerChain = BuildPlayerChainExpression("player", entry, group, parentParam, soundSettingParam);
            var playExpr = $"{playerChain}.Play()";
            if (!string.IsNullOrEmpty(callbackParam))
                return $"{groupExpr}.BuildAsync({nameExpr}, player => {{ var ready = {playExpr}; {callbackParam}?.Invoke(ready); }})";

            return $"{groupExpr}.BuildAsync({nameExpr}, player => {playExpr})";
        }

        public static bool HasPlayerChainModifiers(AudioEntryData entry, AudioGroupData group)
        {
            if (entry.bindParent)
                return true;
            if (entry.loopByDefault)
                return true;
            if (entry.useInterval && group != null && group.playType == AudioPlayType.Sound)
                return true;
            if (entry.useRealTime)
                return true;
            if (entry.use3DSetting)
                return true;
            return false;
        }

        public static string BuildPlayerChainExpression(
            string playerExpr,
            AudioEntryData entry,
            AudioGroupData group,
            string parentParam = null,
            string soundSettingParam = null)
        {
            var chain = new StringBuilder(playerExpr);
            AppendPlayerChainModifiers(chain, entry, group, parentParam, soundSettingParam);
            return chain.ToString();
        }

        private static void AppendPlayerChainModifiers(
            StringBuilder chain,
            AudioEntryData entry,
            AudioGroupData group,
            string parentParam = null,
            string soundSettingParam = null)
        {
            if (entry.loopByDefault)
                chain.Append(".Loop()");

            if (entry.useInterval && group != null && group.playType == AudioPlayType.Sound)
                chain.Append(".Interval()");

            if (entry.useRealTime)
                chain.Append(".IsRealTime()");

            if (entry.use3DSetting)
            {
                if (entry.external3DSetting && !string.IsNullOrEmpty(soundSettingParam))
                    chain.Append(".AudioSourceSoundSetting(").Append(soundSettingParam).Append(')');
                else if (entry.soundSetting != null)
                    chain.Append(".AudioSourceSoundSetting(").Append(ToSettingInitializer(entry.soundSetting)).Append(')');
            }

            if (entry.bindParent && !string.IsNullOrEmpty(parentParam))
                chain.Append(".Parent(").Append(parentParam).Append(')');
        }

        public static void ApplyToAudioSource(AudioSource source, AudioClip clip, AudioEntryData entry)
        {
            if (!source || !clip) return;

            entry.NormalizePlayStyle();
            source.clip = clip;
            source.loop = entry.loopByDefault;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.priority = 128;
            source.panStereo = 0f;
            source.reverbZoneMix = 1f;
            source.dopplerLevel = 1f;
            source.spread = 0;
            source.minDistance = 1f;
            source.maxDistance = 500f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;

            if (entry.use3DSetting && !entry.external3DSetting && entry.soundSetting != null)
            {
                var s = entry.soundSetting;
                source.priority = s.Priority;
                source.pitch = s.Pitch;
                source.panStereo = s.StereoPan;
                source.spatialBlend = s.SpatitalBlend;
                source.reverbZoneMix = s.ReverbZoneMix;
                source.dopplerLevel = s.DopplerLevel;
                source.spread = s.Spread;
                source.minDistance = s.MinDistance;
                source.maxDistance = s.MaxDistance;
                source.rolloffMode = s.VolumeRolloff;
            }
        }

        private static string ToSettingInitializer(AudioSourceSoundSetting s)
        {
            return "new AudioSourceSoundSetting" +
                   "{ Priority = " + s.Priority +
                   ", Pitch = " + s.Pitch + "f" +
                   ", StereoPan = " + s.StereoPan + "f" +
                   ", SpatitalBlend = " + s.SpatitalBlend + "f" +
                   ", ReverbZoneMix = " + s.ReverbZoneMix + "f" +
                   ", DopplerLevel = " + s.DopplerLevel + "f" +
                   ", Spread = " + s.Spread +
                   ", VolumeRolloff = AudioRolloffMode." + s.VolumeRolloff +
                   ", MinDistance = " + s.MinDistance + "f" +
                   ", MaxDistance = " + s.MaxDistance + "f }";
        }
    }
}
