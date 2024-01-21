using UnityEngine;

public enum TowerType
{
    Laser, Mortar,
}

public abstract class Tower : GameTileContent
{
    public abstract TowerType TowerType { get; }

    [SerializeField, Range(1.5f, 10.5f)] protected float targettingRange = 1.5f;

    private static Collider[] targetsBuffer = new Collider[100];
    private const int enemyLayerMask = 1 << 9; // equivalent to 2^9 (because 1 == 2^0)

    protected bool AcquireTarget(out TargetPoint target)
    {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 3f;
        int hits = Physics.OverlapCapsuleNonAlloc(a, b, targettingRange, targetsBuffer, enemyLayerMask);
        if (hits > 0)
        {
            target = targetsBuffer[Random.Range(0, hits)].GetComponent<TargetPoint>();
            Debug.Assert(target != null, "Targeted non-enemy!", targetsBuffer[0]);
            return true;
        }
        target = null;
        return false;
    }

    protected bool TrackTarget(ref TargetPoint target)
    {
        if (target == null)
        {
            return false;
        }


        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targettingRange + 0.125f * target.Enemy.Scale;

        bool targetTooFar = x * x + z * z > r * r;
        if (targetTooFar)
        {
            target = null;
            return false;
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targettingRange);
    }
}
