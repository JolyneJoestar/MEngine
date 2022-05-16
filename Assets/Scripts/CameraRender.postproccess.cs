using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRender
{
    const int BLOOM_NUM = 10;
    partial void InitPostBuffers();
    partial void BlitColor();
    partial void BloomGetInput();
    partial void BloomFilter();
    partial void BloomPass();
    partial void PostProccess();

    static Shader m_postShader = Shader.Find("MyPipeline/PostProccess");
    Material m_postProccessMaterial = new Material(m_postShader);
    RenderTexture[] m_bloomPingpongTex = new RenderTexture[2];
    int m_bloomPingpongFlag;
    enum PostProccessPass
    {
        PostProccessPass_BloomGetInput = 0,
        PostProccessPass_BloomBlur,
        PostProccessPass_Bloom,
    }

    partial void PostProccess()
    {
        InitPostBuffers();
        BlitColor();
        BloomGetInput();
        BloomFilter();
        BloomPass();
    }
    partial void InitPostBuffers()
    {
        m_bloomPingpongTex[0] = RenderTextures.Instance.GetTemperory(m_camera.name + "m_bloomPingpongTex_0", screenSize.x / 4, screenSize.y / 4, 0, RenderTextureFormat.ARGB32);
        m_bloomPingpongTex[1] = RenderTextures.Instance.GetTemperory(m_camera.name + "m_bloomPingpongTex_1", screenSize.x / 4, screenSize.y / 4, 0, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(m_shaderBuffers.baseColorTextureID, screenSize.x, screenSize.y, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_downScaleTexelSize = new Vector2(4f / screenSize.x, 4f / screenSize.y);
    }
    partial void BlitColor()
    {
        m_buffer.BeginSample("copy");
        m_buffer.Blit(BuiltinRenderTextureType.CameraTarget, m_shaderBuffers.baseColorTextureID);
        m_buffer.EndSample("copy");
        ExecuteBuffer();
    }
    partial void BloomGetInput()
    {
        m_buffer.BeginSample("bloom input");
        m_buffer.SetRenderTarget(m_bloomPingpongTex[0]);
        m_buffer.ClearRenderTarget(false,true,Color.black);
        m_buffer.SetGlobalTexture(m_shaderBuffers.baseColorTextureID, m_shaderBuffers.baseColorTextureID);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_postProccessMaterial, (int)PostProccessPass.PostProccessPass_BloomGetInput, MeshTopology.Triangles, 3);
        m_buffer.EndSample("bloom input");
        ExecuteBuffer();
    }
    partial void BloomFilter()
    {
        m_bloomPingpongFlag = 1;
        for (int i = 0; i < BLOOM_NUM; i++)
        {
            m_buffer.BeginSample("bloom blur");
            m_buffer.SetRenderTarget(m_bloomPingpongTex[m_bloomPingpongFlag]);
            m_buffer.SetGlobalTexture(m_shaderBuffers.bloomInputTextureID, m_bloomPingpongTex[1 - m_bloomPingpongFlag]);
            m_buffer.SetGlobalVector(m_shaderProperties.screenProperties.downScaleTexelSizeID, m_downScaleTexelSize);
            m_buffer.SetGlobalInt(m_shaderProperties.isHorizonID, m_bloomPingpongFlag);
            m_buffer.DrawProcedural(Matrix4x4.identity, m_postProccessMaterial, (int)PostProccessPass.PostProccessPass_BloomBlur, MeshTopology.Triangles, 3);
            m_buffer.EndSample("bloom blur");
            ExecuteBuffer();
            m_bloomPingpongFlag = 1 - m_bloomPingpongFlag;
        }
    }
    partial void BloomPass()
    {
        m_buffer.BeginSample("bloom");
        m_buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        m_buffer.SetGlobalTexture(m_shaderBuffers.bloomInputTextureID, m_bloomPingpongTex[1 - m_bloomPingpongFlag]);
        m_buffer.SetGlobalTexture(m_shaderBuffers.baseColorTextureID, m_shaderBuffers.baseColorTextureID);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_postProccessMaterial, (int)PostProccessPass.PostProccessPass_Bloom, MeshTopology.Triangles, 3);
        m_buffer.EndSample("bloom");
        ExecuteBuffer();
    }

}
