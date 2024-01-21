using UnityEngine;

public class MortarShell : WarEntity
{
    private float age, blastRadius, damage;

    private Vector3 launchPoint, targetPoint, launchVelocity;

    public void Initialize(Vector3 launchPoint, Vector3 targetPoint, Vector3 launchVelocity, float blastRadius, float damage)
    {
        this.launchPoint = launchPoint;
        this.targetPoint = targetPoint;
        this.launchVelocity = launchVelocity;
        this.blastRadius = blastRadius;
        this.damage = damage;
    }

    public override bool GameUpdate()
    {
        age += Time.deltaTime;
        Vector3 p = launchPoint + launchVelocity * age;
        p.y -= 0.5f * 9.81f * age * age;

        if (!CheckForGroundHit(p))
        {
            return false;
        }

        transform.localPosition = p;

        Vector3 d = launchVelocity;
        d.y -= 9.81f * age;
        transform.localRotation = Quaternion.LookRotation(d);

        return true;
    }

    private bool CheckForGroundHit(Vector3 position)
    {
        if (position.y <= 0f)
        {
            // // No longer necessary
            //TargetPoint.FillBuffer(targetPoint, blastRadius);
            //for (int i = 0; i < TargetPoint.BufferedCount; i++)
            //{
            //    TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            //}
            Game.SpawnExplosion().Initialize(targetPoint, blastRadius, damage);

            OriginFactory.Reclaim(this);
            return false;
        }

        return true;
    }
}