using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPageTable
{
    int TableSize { get; }

    int MaxMipLevel { get; }
}

public class PageTable : MonoBehaviour, IPageTable
{
    private int m_tableSzie;
    private int m_mipLevelLimit;
    private Shader m_debugShader;
    private TableNode m_pageNode;
    private Dictionary<Vector2Int, TableNode> m_activePages = new Dictionary<Vector2Int, TableNode>();
    private Texture2D m_lookupTexture;
    private ILoader m_loader;

    private ITiledTexture m_tileTexture;
    public int TableSize { get { return m_tableSzie; } }
    public int MaxMipLevel { get { return Mathf.Min(m_mipLevelLimit, (int)Mathf.Log(TableSize, 2)); } }

    private void ProcessFeedBack(Texture2D texture)
    {
        foreach(var c in texture.GetRawTextureData<Color32>())
        {
            ActivePage(c.r, c.g, c.b);
        }

        var currentFrame = (byte)Time.frameCount;
        var pixels = m_lookupTexture.GetRawTextureData<Color32>();
        foreach(var kv in m_activePages)
        {
            var page = kv.Value;

            if (page.PageLoad.ActiveFrame != Time.frameCount)
                continue;
            var c = new Color32((byte)page.PageLoad.TileIndex.x, (byte)page.PageLoad.TileIndex.y, (byte)page.MipLevel, currentFrame);
            for(int y = page.Rect.y; y < page.Rect.yMax; y++)
            {
                for(int x = page.Rect.x; x < page.Rect.xMax; x++)
                {
                    var id = y * m_tableSzie + x;
                    if (pixels[id].b > c.b || pixels[id].a != currentFrame)
                        pixels[id] = c;
                }
            }
        }
        m_lookupTexture.Apply(false);
    }

    private TableNode ActivePage(int x, int y, int mip)
    {
        if (mip > m_pageNode.MipLevel)
            return null;
        var page = m_pageNode.GetAvaliable(x, y, mip);
        if(page == null)
        {
            LoadPage(x, y, m_pageNode);
            return null;
        }
        else if(page.MipLevel > mip)
        {
            LoadPage(x, y, page.GetChild(x, y));
        }

        m_tileTexture.SetActive(page.PageLoad.TileIndex);
        page.PageLoad.ActiveFrame = Time.frameCount;

        return page;
    }

    private void LoadPage(int x, int y, TableNode node)
    {
        if (node == null)
            return;
        if (node.PageLoad.LoadRequest != null)
            return;
        node.PageLoad.LoadRequest = m_loader.Request(x, y, node.MipLevel);
    }

    private void OnLoadComplete(LoadRequest request, Texture2D[] textures)
    {
        var node = m_pageNode.Get(request.PageX, request.PageY, request.MipLevel);
        if (node == null || node.PageLoad.LoadRequest != request)
            return;
        node.PageLoad.LoadRequest = null;
        var id = m_tileTexture.RequestTile();
        m_tileTexture.UpdateTile(id, textures);

        node.PageLoad.TileIndex = id;
        m_activePages[id] = node;
    }

    private void InvalidatePage(Vector2Int id)
    {
        TableNode node = null;
        if (!m_activePages.TryGetValue(id, out node))
            return;
        node.PageLoad.ResetTileIndex();
        m_activePages.Remove(id);
    }


}
