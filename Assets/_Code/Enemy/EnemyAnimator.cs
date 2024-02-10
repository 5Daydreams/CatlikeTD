using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;



[System.Serializable]
public struct EnemyAnimator
{
    public enum Clip { Move, Spawn, ReachDestination, Dying }
    public Clip CurrentClip { get; private set; }

    public bool IsDone => GetPlayable(CurrentClip).IsDone();

    Clip previousClip;

    const float transitionSpeed = 5f;
    float transitionProgress;

    PlayableGraph graph;
    AnimationMixerPlayable mixer;

    public void Configure(Animator animator, EnemyAnimationConfig config)
    {
        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, 4);
        // 3 is the number of clips which the mixer will take care of

        AnimationClipPlayable clip = AnimationClipPlayable.Create(graph, config.Move);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Move, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Spawn);
        clip.SetDuration(config.Spawn.length);
        mixer.ConnectInput((int)Clip.Spawn, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.ReachDestination);
        clip.SetDuration(config.ReachDestination.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.ReachDestination, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Dying);
        clip.SetDuration(config.Dying.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Dying, clip, 0);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Enemy", animator);
        output.SetSourcePlayable(mixer);
    }

    Playable GetPlayable(Clip clip)
    {
        return mixer.GetInput((int)clip);
    }

    void BeginTransition(Clip nextClip)
    {
        previousClip = CurrentClip;
        CurrentClip = nextClip;
        transitionProgress = 0f;
        GetPlayable(nextClip).Play();
    }

    void SetWeight(Clip clip, float weight)
    {
        mixer.SetInputWeight((int)clip, weight);
    }

    public void GameUpdate()
    {
        if (transitionProgress >= 0f)
        {

            transitionProgress += Time.deltaTime * transitionSpeed;
            if (transitionProgress >= 1f)
            {
                transitionProgress = -1f;
                SetWeight(CurrentClip, 1f);
                SetWeight(previousClip, 0f);
                GetPlayable(previousClip).Pause();
            }
            else
            {
                SetWeight(CurrentClip, transitionProgress);
                SetWeight(previousClip, 1f - transitionProgress);
            }
        }
    }

    public void PlayIntro()
    {
        SetWeight(Clip.Spawn, 1f);
        CurrentClip = Clip.Spawn;
        graph.Play();
        transitionProgress = -1f;
    }

    public void PlayMove(float speed)
    {
        GetPlayable(Clip.Move).SetSpeed(speed);
        BeginTransition(Clip.Move);
    }
    public void PlayOutro()
    {
        BeginTransition(Clip.ReachDestination);
    }

    public void PlayDying()
    {
        BeginTransition(Clip.Dying);
    }
    public void Stop()
    {
        graph.Stop();
    }

    public void Destroy()
    {
        graph.Destroy();
    }
}
