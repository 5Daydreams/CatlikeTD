using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{

    [SerializeField] Transform ground = default;
    [SerializeField] GameTile tilePrefab = default;

    Vector2Int size;

    GameTile[] tiles;

    public void Initialize(Vector2Int size)
    {
        this.size = size;
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

                tile.transform.SetParent(transform, false);
                tile.transform.localPosition =
                    new Vector3(x - offset.x, 0f, y - offset.y);



                i++;
            }

            i++;
        }
    }
}