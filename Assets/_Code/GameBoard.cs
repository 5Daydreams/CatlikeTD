using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{

    [SerializeField] Transform ground = default;
    [SerializeField] GameTile tilePrefab = default;
    [SerializeField] Texture2D gridTexture = default;

    Vector2Int size;
    bool showGrid, showPaths;

    GameTile[] tiles;
    Queue<GameTile> searchFrontier = new Queue<GameTile>();

    GameTileContentFactory contentFactory;

    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        this.size = size;
        this.contentFactory = contentFactory;
        ground.localScale = new Vector3(size.x, size.y, 1f);

        Vector2 offset = (size - Vector2.one) * 0.5f;
        tiles = new GameTile[size.x * size.y];

        int i = 0;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                GameTile t = Instantiate(tilePrefab);
                tiles[i] = t;
                GameTile tile = t;

                if (x > 0)
                {
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                }
                if (y > 0)
                {
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
                }

                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition =
                    new Vector3(x - offset.x, 0f, y - offset.y);

                tile.IsAlternative = (i % 2) == 0;
                // tile.IsAlternative = (i & 1) == 0; // funny - this one also works!

                i++;
            }
            // remember - the i++ happens one more time than it should, since it's AFTER the for(x) logic, therefore there is no need to i++ at the start or end of for(y)
        }

        ToggleDestination(tiles[tiles.Length / 2]);
    }

    public bool ShowPaths
    {
        get => showPaths;
        set
        {
            showPaths = value;
            if (showPaths)
            {
                foreach (GameTile tile in tiles)
                {
                    tile.ShowPathArrow();
                }
            }
            else
            {
                foreach (GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    public bool ShowGrid
    {
        get => showGrid;
        set
        {
            showGrid = value;
            Material m = ground.GetComponent<MeshRenderer>().material;
            if (showGrid)
            {
                m.mainTexture = gridTexture;
                m.SetTextureScale("_MainTex", size);
            }
            else
            {
                m.mainTexture = null;
            }
        }
    }

    bool FindPaths()
    {
        foreach (GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                searchFrontier.Enqueue(tile);
            }
            else // anything which isn't a destination isn't relevant right now
            {
                tile.ClearPath();
            }
        }

        if (searchFrontier.Count == 0) // failed to queue any tiles as a destination
        {
            return false;
        }

        // old way of doing it - I'm pretty sure it's no longer necessary to do this in FindPaths(), as it's done by the end of Initialize()

        //tiles[tiles.Length / 2].BecomeDestination();
        //searchFrontier.Enqueue(tiles[tiles.Length / 2]);

        //tiles[0].BecomeDestination();
        //searchFrontier.Enqueue(tiles[0]);

        while (searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier.Dequeue();

            if (tile == null)
            {
                continue;
            }

            if (tile.IsAlternative)
            {
                searchFrontier.Enqueue(tile.GrowPathNorth());
                searchFrontier.Enqueue(tile.GrowPathSouth());
                searchFrontier.Enqueue(tile.GrowPathEast());
                searchFrontier.Enqueue(tile.GrowPathWest());
            }
            else
            {
                searchFrontier.Enqueue(tile.GrowPathWest());
                searchFrontier.Enqueue(tile.GrowPathEast());
                searchFrontier.Enqueue(tile.GrowPathSouth());
                searchFrontier.Enqueue(tile.GrowPathNorth());
            }
        }

        foreach (GameTile tile in tiles)
        {
            if (!tile.HasPath)
            {
                return false;
            }
        }

        if (showPaths)
        {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPathArrow();
            }
        }

        return true; // successfully generated a flow for the board
    }

    public GameTile GetTile(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // this only really works because of centering the board and having tile sizes be (1,1,1)
            int x = (int)(hit.point.x + size.x * 0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);

            bool withinBoardBoundaries = (0 <= x && x < size.x) && (0 <= y && y < size.y);

            if (!withinBoardBoundaries)
            {
                return null;
            }

            return tiles[x + y * size.x];
        }

        return null;
    }

    public void ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths())
            {
                Debug.LogWarning("Board needs at least one destination tile.");
                tile.Content = contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }
}