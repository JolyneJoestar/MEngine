using UnityEngine;

public class PageLoader
{
    private static Vector2Int s_invalidTileIndex = new Vector2Int(-1, -1);

    public Vector2Int TileIndex = s_invalidTileIndex;

    public int ActiveFrame;

    public LoadRequest LoadRequest;

    public bool IsReady { get { return TileIndex != s_invalidTileIndex; } }

    public void ResetTileIndex()
    {
        TileIndex = s_invalidTileIndex;
    }
}
