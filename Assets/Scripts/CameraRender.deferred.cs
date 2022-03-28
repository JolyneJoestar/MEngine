using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRender
{
    partial void initGBuffer();
    partial void deferredRenderGBufferPass(bool useDynamicBatching, bool useGPUInstancing);
    partial void deferredRenderLightingPass();
    partial void DrawDeferred(bool useDynamicBatching, bool useGPUInstancings);

    static Shader m_shader = Shader.Find("MyPipeline/DeferredRender");
    Material m_deferredRenderingMaterial = new Material(m_shader);
    static ShaderTagId m_gBufferPassId = new ShaderTagId("gBufferPass");
    static int[] geometricTextureId =
    {
        Shader.PropertyToID("_GPosition"),
        Shader.PropertyToID("_GNormal"),
        Shader.PropertyToID("_GAlbedo"),
        Shader.PropertyToID("_GMaterial")
    };
    RenderTargetIdentifier[] m_renderTarget;
    const string m_gbufferName = "GBufferPass";
    RenderBuffer defaultColorBuffer;
    RenderBuffer defaultDepthBuffer;

    int width = 2048;
    int height = 2048;

    partial void initGBuffer()
    {
        defaultColorBuffer = Graphics.activeColorBuffer;
        defaultDepthBuffer = Graphics.activeDepthBuffer;
        m_buffer.GetTemporaryRT(geometricTextureId[0], width, height, 32, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(geometricTextureId[1], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(geometricTextureId[2], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(geometricTextureId[3], width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        ExecuteBuffer();
        m_renderTarget = new RenderTargetIdentifier[geometricTextureId.Length];
        for(int i = 0; i < m_renderTarget.Length; i++)
        {
            m_renderTarget[i] = geometricTextureId[i];
        }
        m_buffer.SetRenderTarget(m_renderTarget, m_renderTarget[0]);
        ExecuteBuffer();
    }

    partial void deferredRenderGBufferPass(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(m_camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(m_gBufferPassId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
            perObjectData = PerObjectData.Lightmaps | PerObjectData.LightProbe | PerObjectData.LightProbeProxyVolume
        };
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        drawingSettings.SetShaderPassName(1, m_gBufferPassId);
        m_context.DrawRenderers(m_cullResult, ref drawingSettings, ref filteringSettings);
        m_context.DrawSkybox(m_camera);
    }

    partial void deferredRenderLightingPass()
    {
        m_buffer.SetRenderTarget(defaultColorBuffer, defaultDepthBuffer);
        for (int i = 0; i < m_renderTarget.Length; i++)
        {
            m_buffer.SetGlobalTexture(geometricTextureId[i], m_renderTarget[i]);
        }
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 0, MeshTopology.Triangles, 3);
        ExecuteBuffer();
    }

    partial void DrawDeferred(bool useDynamicBatching, bool useGPUInstancing)
    {
        initGBuffer();
        deferredRenderGBufferPass(useDynamicBatching, useGPUInstancing);
        deferredRenderLightingPass();
    }
}
