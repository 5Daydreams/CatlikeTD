using UnityEngine;

public enum EnemyType
{
    Small, Medium, Large
}


public class Enemy : GameBehavior
{
    [SerializeField] private Transform model = default;
    [SerializeField] private EnemyAnimationConfig animationConfig = default;

    EnemyAnimator animator;

    public float Scale { get; private set; }

    private EnemyFactory originFactory;

    private GameTile tileFrom, tileTo;
    private Vector3 positionFrom, positionTo;
    private float progress = 0.0f, progressFactor = 1.0f, speed;

    private Direction direction;
    private DirectionChange directionChange;
    private float directionAngleFrom, directionAngleTo;
    private float pathOffset;
    private float Health { get; set; }


    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public void SpawnOn(GameTile tile)
    {
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        PrepareIntro();
        progress = 0f;
    }

    void Awake()
    {
        animator.Configure(
            model.GetChild(0).gameObject.AddComponent<Animator>(),
            animationConfig
        );
    }

    public void Initialize(float scale, float speed, float pathOffset, float health)
    {
        model.localScale = new Vector3(scale, scale, scale);
        Scale = scale;
        Health = health;
        this.speed = speed / Mathf.Max(1.0f, scale);
        this.pathOffset = pathOffset;
        animator.PlayIntro();
    }

    void PrepareNextState()
    {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;
        positionFrom = positionTo;
        if (tileTo == null)
        {
            PrepareOutro();
            return;
        }

        positionTo = tileFrom.ExitPoint;
        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;
        switch (directionChange)
        {
            case DirectionChange.None:
                PrepareForward();
                break;
            case DirectionChange.TurnRight:
                PrepareTurnRight();
                break;
            case DirectionChange.TurnLeft:
                PrepareTurnLeft();
                break;
            default:
                PrepareTurnAround();
                break;
        }
    }

    void PrepareIntro()
    {
        progressFactor = 2f * speed;
        positionFrom = tileFrom.transform.localPosition;
        transform.localPosition = positionFrom; // but why...?
        positionTo = tileFrom.ExitPoint;
        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
    }

    void PrepareForward()
    {
        progressFactor = speed;
        transform.localRotation = direction.GetRotation();
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
    }

    void PrepareTurnRight()
    {
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
        directionAngleTo = directionAngleFrom + 90f;
        model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
    }

    void PrepareTurnLeft()
    {
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
        directionAngleTo = directionAngleFrom - 90f;
        model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
    }

    void PrepareTurnAround()
    {
        float angleByOffset = (pathOffset < 0f ? 180f : -180f);
        directionAngleTo = directionAngleFrom + angleByOffset;
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localPosition = positionFrom;
        progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.1f));
    }

    void PrepareOutro()
    {
        positionTo = tileFrom.transform.localPosition;
        directionChange = DirectionChange.None;
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }

    public override bool GameUpdate()
    {
        animator.GameUpdate();
        if (animator.CurrentClip == EnemyAnimator.Clip.Spawn)
        {
            if (!animator.IsDone)
            {
                return true;
            }
            animator.PlayMove(speed / Scale);
        }
        // note that the usage here requires SETTING ENUMS IN THE CORRECT ORDER.
        else if (animator.CurrentClip >= EnemyAnimator.Clip.ReachDestination)
        {
            if (animator.IsDone)
            {
                Recycle();
                return false;
            }
            return true;
        }

        if (Health <= 0f)
        {
            animator.PlayDying();
            return true;
        }

        progress += Time.deltaTime * progressFactor;
        while (progress >= 1f)
        {
            if (tileTo == null) // to stop if the destination is found
            {
                Game.EnemyReachedDestination();
                //Recycle();
                animator.PlayOutro();
                return true;
            }

            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }

        if (directionChange == DirectionChange.None)
        {
            transform.localPosition =
                Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }
        else
        {
            float angle = Mathf.LerpUnclamped(
                directionAngleFrom, directionAngleTo, progress
            );
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        return true;
    }

    public void ApplyDamage(float damage)
    {
        Debug.Assert(damage >= 0f, "Negative damage applied.");
        Health -= damage;
    }

    public override void Recycle()
    {
        animator.Stop();
        OriginFactory.Reclaim(this);
    }

    private void OnDestroy()
    {
        animator.Destroy();
    }
}