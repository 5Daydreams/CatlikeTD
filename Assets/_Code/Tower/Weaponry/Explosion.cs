using UnityEngine;

public class Explosion : WarEntity
{
    [SerializeField] private AnimationCurve opacityCurve = default;
    [SerializeField] private AnimationCurve scaleCurve = default;

    [SerializeField, Range(0f, 1f)] float duration = 0.5f;

    private static MaterialPropertyBlock propertyBlock;

    private float age;
    private float scale;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Assert(meshRenderer != null, "Explosion without renderer!");
    }

    public void Initialize(Vector3 position, float blastRadius, float damage)
    {
        TargetPoint.FillBuffer(position, blastRadius);
        for (int i = 0; i < TargetPoint.BufferedCount; i++)
        {
            TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
        }
        transform.localPosition = position;
        scale = 2f * blastRadius;
    }

    public override bool GameUpdate()
    {
        age += Time.deltaTime;
        if (age >= duration)
        {
            OriginFactory.Reclaim(this);
            return false;
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        float t = age / duration;
        Color c = Color.clear;

        c.a = opacityCurve.Evaluate(t);
        propertyBlock.SetColor("_Color", c);
        meshRenderer.SetPropertyBlock(propertyBlock);
        transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));

        return true;
    }
}