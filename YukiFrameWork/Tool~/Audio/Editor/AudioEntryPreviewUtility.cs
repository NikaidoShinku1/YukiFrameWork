///=====================================================
/// - FileName:      AudioEntryPreviewUtility.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   编辑器音频预览（按 AudioEntry 规则）
/// - Creation Time: 2025/5/26
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace YukiFrameWork.Audio
{
    public static class AudioEntryPreviewUtility
    {
        private static GameObject previewRoot;
        private static AudioSource previewSource;
        private static AudioClip previewClip;

        public static bool IsPlaying => previewSource && previewSource.isPlaying;

        public static bool IsPreviewingEntry(AudioEntryData entry) =>
            entry?.clip != null && IsPlaying && previewClip == entry.clip;

        public static void Preview(AudioEntryData entry, AudioGroupData group, string groupAccessorCode)
        {
            if (entry?.clip == null) return;

            Stop();
            EnsureSource();

            AudioEntryPlayChain.ApplyToAudioSource(previewSource, entry.clip, entry);
            previewSource.Play();
            previewClip = entry.clip;

            var chain = AudioEntryPlayChain.BuildPlayExpression(
                groupAccessorCode,
                $"\"{entry.ResolvedAssetName}\"",
                entry,
                group);

            Debug.Log($"[AudioPreview] {entry.GetDisplayName()}\n{chain}");
        }

        public static void Stop()
        {
            if (previewSource)
            {
                previewSource.Stop();
                previewSource.clip = null;
            }
            previewClip = null;
        }

        public static void Toggle(AudioEntryData entry, AudioGroupData group, string groupAccessorCode)
        {
            if (IsPlaying && previewClip == entry.clip)
                Stop();
            else
                Preview(entry, group, groupAccessorCode);
        }

        private static void EnsureSource()
        {
            if (previewRoot) return;
            previewRoot = EditorUtility.CreateGameObjectWithHideFlags(
                "AudioKitPreview", HideFlags.HideAndDontSave);
            previewSource = previewRoot.AddComponent<AudioSource>();
            previewSource.playOnAwake = false;
        }

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            EditorApplication.playModeStateChanged += _ =>
            {
                if (previewRoot) Object.DestroyImmediate(previewRoot);
                previewRoot = null;
                previewSource = null;
                previewClip = null;
            };
        }
    }
}
#endif
