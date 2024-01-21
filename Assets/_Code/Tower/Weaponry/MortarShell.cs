using UnityEngine;

public class MortarShell : WarEntity
{
    float age;

    Vector3 launchPoint, targetPoint, launchVelocity;

    public void Initialize(Vector3 launchPoint, Vector3 targetPoint, Vector3 launchVelocity)
    {
        this.launchPoint = launchPoint;
        this.targetPoint = targetPoint;
        this.launchVelocity = launchVelocity;
    }

    public override bool GameUpdate()
    {
        age += Time.deltaTime;
        Vector3 p = launchPoint + launchVelocity * age;
        p.y -= 0.5f * 9.81f * age * age;
        transform.localPosition = p;
        return true;
    }
}