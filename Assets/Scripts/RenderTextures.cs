using UnityEngine;
using System.Collections.Generic;

public class RenderTextures
{
    // Start is called before the first frame update
    static RenderTextures m_instance;
    public static RenderTextures Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new RenderTextures();
            return m_instance;
        }
    }
    Dictionary<string, RenderTexture> m_textures = new Dictionary<string, RenderTexture>();

    public RenderTexture GetTemperory(string name, int width, int height,int depth, RenderTextureFormat format)
    {
        if(m_textures.ContainsKey(name))
            return m_textures[name];
        m_textures[name] = new RenderTexture(width, height, depth, format);
        return m_textures[name];
    }

    public void ClearAll()
    {
        m_textures.Clear();
    }
}
