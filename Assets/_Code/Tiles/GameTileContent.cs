using UnityEngine;


public enum GameTileContentType
{
    Empty, Destination, Wall, SpawnPoint, Tower
}

[SelectionBase]
public class GameTileContent : MonoBehaviour
{
    [SerializeField] private GameTileContentType type = default;
    public GameTileContentType Type => type;
    public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

    private GameTileContentFactory originFactory;

    public GameTileContentFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public virtual void GameUpdate() { }

    public void Recycle()
    {
        originFactory.Reclaim(this);
    }
}