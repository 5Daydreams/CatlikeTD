using UnityEngine;

public class LaserTower : Tower
{
    [SerializeField] private Transform turret = default, laserBeam = default;
    [SerializeField, Range(1f, 100f)] private float damagePerSecond = 10f;

    private static Collider[] targetsBuffer = new Collider[100];
    private const int enemyLayerMask = 1 << 9; // equivalte to 2^9 (because 1 == 2^0)

    private Vector3 laserBeamScale;
    private TargetPoint target;

    void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    public override void GameUpdate()
    {
        if (TrackTarget(ref target) || AcquireTarget(out target))
        {
            Shoot();
        }
        else
        {
            laserBeam.localScale = Vector3.zero;
        }
    }

    void Shoot()
    {
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;

        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward; // omg, this is def something to work on by adding VFX to it XD

        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}
