using UnityEngine;

public class MortarTower : Tower
{
    [SerializeField, Range(0.5f, 2f)] float shotsPerSecond = 1f; //rename into fire rate or something

    [SerializeField] Transform mortar = default;

    public override TowerType TowerType => TowerType.Mortar;

    private float launchSpeed;
    private float launchProgress;


    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        float x = targettingRange + 0.25001f;
        float y = -mortar.position.y;
        launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
    }

    public override void GameUpdate()
    {
        launchProgress += shotsPerSecond * Time.deltaTime;

        // This is essentially a simple time-based progress bar, using 1.0f as 100%
        while (launchProgress >= 1.0f)
        {
            if (AcquireTarget(out TargetPoint target))
            {
                Launch(target);
                launchProgress -= 1.0f;
            }
            else
            {
                launchProgress = 0.9999f;
            }

        }
    }

    public void Launch(TargetPoint target)
    {
        Vector3 launchPoint = mortar.position;
        Vector3 targetPoint = target.Position;
        targetPoint.y = 0f;

        Vector2 direction;
        direction.x = targetPoint.x - launchPoint.x;
        direction.y = targetPoint.z - launchPoint.z;

        float x = direction.magnitude;
        float y = -launchPoint.y;
        direction /= x; // now direction is normalized

        float g = 9.81f;
        float s = launchSpeed;
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);
        // Assert that the discriminator (Bhaskara's delta) is not an imaginary value
        Debug.Assert(r >= 0f, "Launch velocity insufficient for range!");

        float tanO = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosO = Mathf.Cos(Mathf.Atan(tanO));
        float sinO = cosO * tanO;

        mortar.localRotation = Quaternion.LookRotation(new Vector3(direction.x, tanO, direction.y));

        Game.SpawnShell().Initialize(
            launchPoint,
            targetPoint,
            new Vector3(s * cosO * direction.x, s * sinO, s * cosO * direction.y)
        );

        // I'm not a fan of this approach. If a for-loop is required, the differential form is more explicit
        Vector3 prev = launchPoint;
        Vector3 next;
        for (int i = 1; i <= 10; i++)
        {
            float t = i / 10f;
            float dx = s * cosO * t; // dx means "vx"
            float dy = s * sinO * t - 0.5f * g * t * t; // dy means "vy"
            next = launchPoint + new Vector3(direction.x * dx, dy, direction.y * dx);
            Debug.DrawLine(prev, next, Color.blue, 1.0f);
            prev = next;
        }

        Debug.DrawLine(launchPoint, targetPoint, Color.yellow, 1.0f);
        Debug.DrawLine(
            new Vector3(launchPoint.x, 0.01f, launchPoint.z),
            new Vector3(launchPoint.x + direction.x * x, 0.01f, launchPoint.z + direction.y * x),
            Color.white, 1.0f);
    }
}
