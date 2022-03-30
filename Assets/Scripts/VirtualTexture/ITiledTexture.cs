using System;
using UnityEngine;

public interface ITiledTexture
{
    event Action<Vector2Int> OnTileUpdateComplete;

    Vector2Int RegionSize { get; }

    int TileSize { get; }

    int PaddingSize { get; }

    int LayerCount { get; }

    Vector2Int RequestTile();

    bool SetActive(Vector2Int tile);

    void UpdateTile(Vector2Int tile, Texture2D[] textures);
}
