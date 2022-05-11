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
        if (m_textures.ContainsKey(name))
        {
            if (m_textures[name] == null)
            {
                m_textures[name] = new RenderTexture(width, height, depth, format);
                return m_textures[name];
            }
            if (m_textures[name].width == width && m_textures[name].height == height)
            {
                 return m_textures[name];
            }
        }
        m_textures[name] = new RenderTexture(width, height, depth, format);
        return m_textures[name];
    }

    public void ClearAll()
    {
        Debug.Log("clear");
        m_textures.Clear();
    }
}
