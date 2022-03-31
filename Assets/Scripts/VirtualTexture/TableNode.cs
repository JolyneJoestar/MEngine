using UnityEngine;

public class TableNode
{
    private TableNode[] m_children;

    public int MipLevel { get; }
    public RectInt Rect { get; }

    public PageLoader PageLoad { get; }
    public TableNode(int mip, int x, int y, int width, int height)
    {
        MipLevel = mip;
        Rect = new RectInt(x, y, width, height);
        PageLoad = new PageLoader();
    }
    public TableNode Get(int x, int y, int mip)
    {
        if (!Contains(x, y))
            return null;
        if (MipLevel == mip)
            return this;
        if(m_children != null)
        {
            foreach(var child in m_children)
            {
                var item = child.Get(x, y, mip);
                if (item != null)
                    return item;
            }
        }
        return null;
    }

    public TableNode GetAvaliable(int x, int y, int mip)
    {
        if (!Contains(x, y))
            return null;
        if(MipLevel > mip && m_children != null)
        {
            foreach(var child in m_children)
            {
                var item = child.GetAvaliable(x, y, mip);
                if (item != null)
                    return item;
            }
        }
        return (PageLoad.IsReady ? this : null);
    }

    public TableNode GetChild(int x, int y)
    {
        if (!Contains(x, y))
            return null;
        if (MipLevel == 0)
            return null;
        if(m_children == null)
        {
            m_children = new TableNode[4];
            m_children = new TableNode[4];
            m_children[0] = new TableNode(MipLevel - 1, Rect.x, Rect.y, Rect.width / 2, Rect.height / 2);
            m_children[1] = new TableNode(MipLevel - 1, Rect.x + Rect.width / 2, Rect.y, Rect.width / 2, Rect.height / 2);
            m_children[2] = new TableNode(MipLevel - 1, Rect.x, Rect.y + Rect.height / 2, Rect.width / 2, Rect.height / 2);
            m_children[3] = new TableNode(MipLevel - 1, Rect.x + Rect.width / 2, Rect.y + Rect.height / 2, Rect.width / 2, Rect.height / 2);
        }

        foreach(var child in m_children)
        {
            if (child.Contains(x, y))
                return child;
        }

        return null;
    }

    private bool Contains(int x, int y)
    {
        if (x < Rect.x || x >= Rect.xMax)
            return false;

        if (y < Rect.y || y >= Rect.yMax)
            return false;

        return true;
    }
}
