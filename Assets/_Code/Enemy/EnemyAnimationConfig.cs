using UnityEngine;

[CreateAssetMenu]
public class EnemyAnimationConfig : ScriptableObject
{

    [SerializeField] private AnimationClip 
        move = default, 
        spawn = default, 
        reachDestination = default,
        dying = default;

    public AnimationClip Move => move;
    public AnimationClip Spawn => spawn;
    public AnimationClip ReachDestination => reachDestination;
    public AnimationClip Dying => dying;
}

