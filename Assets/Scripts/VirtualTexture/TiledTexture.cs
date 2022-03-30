using System;
using UnityEngine;
using System.Collections.Generic;

public class TileTextureStat
{
    private Dictionary<int, int> m_map = new Dictionary<int, int>();

    public int CurrentActive { get; private set; }
    public int MaxActive { get; private set; }

    public void Reset()
    {
        MaxActive = Mathf.Max(MaxActive, CurrentActive);
        CurrentActive = 0;
        m_map.Clear();
    }

    public void AddActive(int id)
    {
        if (!m_map.ContainsKey(id))
        {
            m_map[id] = 1;
            CurrentActive++;
        }
    }
}


public class TiledTexture : MonoBehaviour, ITiledTexture
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public event Action<Vector2Int> OnTileUpdateComplete;

    private Vector2Int m_regionSize;
    private int m_tileSize;
    private int m_paddingSize;
    private int m_layerCount = 1;
    private Shader m_drawTextureShader;
    private Material m_drawTextureMaterial;
    private LRUCache m_tilePool;


    public RenderTexture[] Textures { get; private set; }
    public TileTextureStat Stat { get; } = new TileTextureStat();
    public Vector2Int RegionSize { get { return m_regionSize; } }
    public int TileSize { get { return m_tileSize; } }
    public int PaddingSize { get { return m_paddingSize; } }
    public int LayerCount { get { return m_layerCount; } }
    public int TileSizeWithPadding { get { return m_tileSize + m_tileSize + m_paddingSize * 2; } }

    public Vector2Int RequestTile()
    {
        return IdToPos(m_tilePool.First);
    }

    public bool SetActive(Vector2Int tile)
    {
        bool succes = m_tilePool.SetActive(PosToId(tile));
        if (succes)
            Stat.AddActive(PosToId(tile));
        return succes;
    }

    public void UpdateTile(Vector2Int tile, Texture2D[] textures)
    {
        if (!SetActive(tile))
            return;
        if (textures == null)
            return;
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] != null)
            {
                DrawTexture(textures[i], Textures[i], new RectInt(tile.x * TileSizeWithPadding, tile.y * TileSizeWithPadding, TileSizeWithPadding, TileSizeWithPadding));
            }
        }
    }

    private Vector2Int IdToPos(int id)
    {
        return new Vector2Int(id % RegionSize.x, id / RegionSize.x);
    }

    private int PosToId(Vector2Int pos)
    {
        return pos.y * pos.x + pos.x;
    }

    private void DrawTexture(Texture source, RenderTexture target, RectInt position)
    {
        if (source == null || target == null || m_drawTextureShader == null)
            return;
        if (m_drawTextureMaterial == null)
            m_drawTextureMaterial = new Material(m_drawTextureShader);
        float l = position.x * 2.0f / target.width - 1;
        float r = (position.x + position.width) * 2.0f / target.width - 1;
        float b = position.y * 2.0f / target.height - 1;
        float t = (position.y + position.height) * 2.0f / target.height - 1;
        var mat = new Matrix4x4();
        mat.m00 = r - l;
        mat.m03 = l;
        mat.m11 = t - b;
        mat.m23 = -l;
        mat.m33 = l;

        m_drawTextureMaterial.SetMatrix();
        target.DiscardContents();
        Graphics.Blit(source, target, m_drawTextureMaterial);
    }
}
