using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.903f, 0.87f)]
[TrackClipType(typeof(SkillEffectClip))]
[TrackBindingType(typeof(GameObject))]
public class SkillEffectTrack : TrackAsset
{
}

[System.Serializable]
public class SkillEffectClip : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<GameObject> effect;
    public EffectPlayMode effectPlayMode;
    public bool isEndPause;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SkillEffectBehaviour>.Create(graph);
        var audioBehaviour = playable.GetBehaviour();
        audioBehaviour.effect = effect.Resolve(graph.GetResolver());
        audioBehaviour.effectPlayMode = effectPlayMode;
        audioBehaviour.isEndPause = isEndPause;
        return playable;
    }
}

public enum EffectPlayMode
{
    SetActive,
    Instantiate
}

public class SkillEffectBehaviour : PlayableBehaviour
{
    public GameObject effect;
    public EffectPlayMode effectPlayMode;
    public bool isEndPause;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (effect == null)
            return;
        if (effectPlayMode == EffectPlayMode.SetActive)
            effect.SetActive(true);
        else
            Object.Instantiate(effect);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {

    }
}