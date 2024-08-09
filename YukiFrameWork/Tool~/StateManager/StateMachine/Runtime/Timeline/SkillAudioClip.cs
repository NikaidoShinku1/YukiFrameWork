using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.903f, 0.87f)]
[TrackClipType(typeof(SkillAudioClip))]
[TrackBindingType(typeof(AudioSource))]
public class SkillAudioTrack : TrackAsset
{
}

public enum AudioPlayMode
{
    Play,
    PlayOneShot
}

[System.Serializable]
public class SkillAudioClip : PlayableAsset, ITimelineClipAsset
{
    public AudioClip clip;
    public ExposedReference<AudioSource> audioSource;
    public AudioPlayMode audioPlayMode;
    public bool isEndPause;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SkillAudioBehaviour>.Create(graph);
        var audioBehaviour = playable.GetBehaviour();
        audioBehaviour.clip = clip;
        audioBehaviour.audioSource = audioSource.Resolve(graph.GetResolver());
        audioBehaviour.audioPlayMode = audioPlayMode;
        audioBehaviour.isEndPause = isEndPause;
        return playable;
    }
}

public class SkillAudioBehaviour : PlayableBehaviour
{
    public AudioClip clip;
    public AudioSource audioSource;
    public AudioPlayMode audioPlayMode;
    public bool isEndPause;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (clip == null || audioSource == null)
            return;
        audioSource.clip = clip;
        if (audioPlayMode == AudioPlayMode.Play)
            audioSource.Play();
        else
            audioSource.PlayOneShot(clip);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (clip == null || audioSource == null)
            return;
        if (isEndPause)
            audioSource.Stop();
    }
}