using UnityEngine;
using UnityEngine.Rendering;

public class DeferredGeometry
{
    static int[] geometricTextureId =
    {
        Shader.PropertyToID("_GPosition"),
        Shader.PropertyToID("_GNormal"),
        Shader.PropertyToID("_GAlbedo"),
        Shader.PropertyToID("_GMaterial")
    };
    const string m_bufferName = "GBufferPass";

    CommandBuffer m_buffer = new CommandBuffer
    {
        name = m_bufferName
    };
    int width = 2048;
    int height = 2048;
    public void initGBuffer()
    {
        m_buffer.BeginSample(m_bufferName);
        m_buffer.GetTemporaryRT(geometricTextureId[0], width, height, 32, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(geometricTextureId[1], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(geometricTextureId[2], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(geometricTextureId[3], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        
    }

}
