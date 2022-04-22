using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRender
{
    partial void initGBuffer();
    partial void deferredRenderGBufferPass(bool useDynamicBatching, bool useGPUInstancing);
    partial void deferredRenderLightingPass();
    partial void deferredRenderAOGenPass();
    partial void deferredRenderAOBlurPass();
    partial void deferredLightVolumeGenPass();
    partial void deferredLightVolumeBlurPass();
    partial void deferredSSRPass();
    partial void BloomGetInput();
    partial void BloomPass();
    partial void CopyColorBuffer();
    partial void TAAPass();
    partial void DrawDeferred(bool useDynamicBatching, bool useGPUInstancings);
    partial void Cleanupdr();

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
    static int bluredAoTextureId = Shader.PropertyToID("_BluredAoTexture"),
        aoTextureId = Shader.PropertyToID("_AoTexture"),
        samplesId = Shader.PropertyToID("samples"),
        noiseId = Shader.PropertyToID("_Noise"),
        lightVolumeId = Shader.PropertyToID("_LightVolume"),
        bluredLightVolumeId = Shader.PropertyToID("_BluredLightVolume"),
        ditherId = Shader.PropertyToID("_Dither"),
        DFColorBufferId = Shader.PropertyToID("_DFColorBuffer"),
        highlightColorBufferId = Shader.PropertyToID("_HighlightColorBufferId"),
        bloomInput = Shader.PropertyToID("_BloomInput"),
        baseColorBuffer = Shader.PropertyToID("_BaseColorBuffer"),
        preColorBuffer = Shader.PropertyToID("_PreColorBuffer"),
        currentColorBuffer = Shader.PropertyToID("_CurrentColorBuffer"),
        preV = Shader.PropertyToID("_PreV"),
        preP = Shader.PropertyToID("_PreP"),
        jitterId = Shader.PropertyToID("_Jitter");

    static int defaultRenderBufferId = Shader.PropertyToID("defaultRenderBufferId");
    RenderTargetIdentifier[] m_renderTarget = new RenderTargetIdentifier[geometricTextureId.Length];
    RenderTargetIdentifier[] m_postProcessSrcTex = new RenderTargetIdentifier[2];
    static Vector4[] m_aosample;
    const string m_gbufferName = "GBufferPass";
    RenderBuffer defaultColorBuffer;
    RenderBuffer defaultDepthBuffer;
    RenderTexture defaultTexture;
    Texture2D m_noiseTexture = Resources.Load<Texture2D>("Blue_Noise");

    //taa properties
    Matrix4x4[] m_preV = new Matrix4x4[2];
    Matrix4x4[] m_preP = new Matrix4x4[2];
    static RenderTexture[] m_preTexture = new RenderTexture[2];

    private Vector2[] HaltonSequence = new Vector2[]
    {
            new Vector2(0.5f, 1.0f / 3),
            new Vector2(0.25f, 2.0f / 3),
            new Vector2(0.75f, 1.0f / 9),
            new Vector2(0.125f, 4.0f / 9),
            new Vector2(0.625f, 7.0f / 9),
            new Vector2(0.375f, 2.0f / 9),
            new Vector2(0.875f, 5.0f / 9),
            new Vector2(0.0625f, 8.0f / 9),
    };
    private Vector2 jitter;
    int frameCount = 0;
    int m_aaPingpongFlag = 0;
        
 //   RenderTexture 
    int width = 2048;
    int height = 2048;

    partial void initGBuffer()
    {
        if(m_aosample == null)
        {
            m_aosample = new Vector4[64];
            for(int i = 0; i < m_aosample.Length; i++)
            {
                m_aosample[i] = new Vector4(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f,1.0f), Random.Range(0.0f,1.0f),1.0f);
            }
        }

        if (width != m_camera.scaledPixelWidth || height != m_camera.scaledPixelHeight)
        {
            width = m_camera.scaledPixelWidth;
            height = m_camera.scaledPixelHeight;
            m_preTexture[0] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            m_preTexture[1] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        }
        frameCount++;
        int index = frameCount % 8;
        jitter = new Vector2((HaltonSequence[index].x - 0.5f) / width, (HaltonSequence[index].y - 0.5f) / height);

        m_preV[m_aaPingpongFlag] = m_camera.worldToCameraMatrix;
        m_preP[m_aaPingpongFlag] = m_camera.projectionMatrix;

        if (m_preP[1 - m_aaPingpongFlag] == m_camera.projectionMatrix)
        {
            int preIndex = (frameCount - 1) % 8;
            Vector2 preJitter = new Vector2((HaltonSequence[preIndex].x - 0.5f) / width, (HaltonSequence[preIndex].y - 0.5f) / height);
            m_preP[m_aaPingpongFlag].m02 -= preJitter.x * 2.0f;
            m_preP[m_aaPingpongFlag].m12 -= preJitter.y * 2.0f;
        }
        
        m_preP[m_aaPingpongFlag].m02 += jitter.x * 2.0f;
        m_preP[m_aaPingpongFlag].m12 += jitter.y * 2.0f;

        m_camera.projectionMatrix = m_preP[m_aaPingpongFlag];

        defaultColorBuffer = Graphics.activeColorBuffer;
        defaultDepthBuffer = Graphics.activeDepthBuffer;

        m_buffer.GetTemporaryRT(defaultRenderBufferId, width, height, 32, FilterMode.Point, RenderTextureFormat.ARGB32);

        m_buffer.GetTemporaryRT(geometricTextureId[0], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(geometricTextureId[1], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
        m_buffer.GetTemporaryRT(geometricTextureId[2], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(geometricTextureId[3], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
        ExecuteBuffer();

        for (int i = 0; i < geometricTextureId.Length; i++)
        {
            m_renderTarget[i] = geometricTextureId[i];
        }
        m_buffer.GetTemporaryRT(DFColorBufferId, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(baseColorBuffer, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(bloomInput, width / 4, height / 4, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);

        ExecuteBuffer();
    }

    partial void deferredRenderGBufferPass(bool useDynamicBatching, bool useGPUInstancing)
    {
        m_buffer.SetRenderTarget(m_renderTarget, defaultDepthBuffer);
        m_buffer.ClearRenderTarget(true, true, Color.black);
        ExecuteBuffer();
        var sortingSettings = new SortingSettings(m_camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(m_gBufferPassId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
        };
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        drawingSettings.SetShaderPassName(1, m_gBufferPassId);
        m_context.DrawRenderers(m_cullResult, ref drawingSettings, ref filteringSettings);
        ExecuteBuffer();
    }

    partial void deferredRenderAOGenPass()
    {
        m_buffer.BeginSample("ao gen");
        m_buffer.GetTemporaryRT(aoTextureId, width / 2, height / 2, 0, FilterMode.Trilinear, RenderTextureFormat.R16);
        m_buffer.SetRenderTarget(aoTextureId);
        for (int i = 0; i < 2; i++)
        {
            m_buffer.SetGlobalTexture(geometricTextureId[i], m_renderTarget[i]);
        }
        m_buffer.SetGlobalTexture(noiseId, m_noiseTexture);
        m_buffer.ClearRenderTarget(false, true, Color.white);
        m_buffer.SetGlobalVectorArray(samplesId, m_aosample);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 1, MeshTopology.Triangles, 3);
        m_buffer.EndSample("ao gen");
        ExecuteBuffer();
    }

    partial void Cleanupdr()
    {
        for (int i = 0; i < geometricTextureId.Length; i++)
        {
            m_buffer.ReleaseTemporaryRT(geometricTextureId[i]);
        }
        m_buffer.ReleaseTemporaryRT(bluredAoTextureId);
        m_buffer.ReleaseTemporaryRT(lightVolumeId);
        m_buffer.ReleaseTemporaryRT(aoTextureId);
        m_buffer.ReleaseTemporaryRT(bluredLightVolumeId);
        m_buffer.ReleaseTemporaryRT(DFColorBufferId);
        m_buffer.ReleaseTemporaryRT(bloomInput);
        m_buffer.ReleaseTemporaryRT(baseColorBuffer);
        m_buffer.ReleaseTemporaryRT(defaultRenderBufferId);
        //m_buffer.ReleaseTemporaryRT(defaultDepthBufferId);
        RenderTexture.ReleaseTemporary(defaultTexture);
        //RenderTexture.ReleaseTemporary(defaultDepthBuffer);
        ExecuteBuffer();
    }
    partial void deferredRenderLightingPass()
    {
        m_buffer.BeginSample("deferred lighting pass");
        m_buffer.SetRenderTarget(baseColorBuffer, defaultDepthBuffer);
        for (int i = 0; i < m_renderTarget.Length; i++)
        {
            m_buffer.SetGlobalTexture(geometricTextureId[i], m_renderTarget[i]);
        }
        m_buffer.SetGlobalTexture(bluredLightVolumeId, bluredLightVolumeId);
        m_buffer.SetGlobalTexture(bluredAoTextureId, bluredAoTextureId);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 0, MeshTopology.Triangles, 3);
        m_buffer.EndSample("deferred lighting pass");
        ExecuteBuffer();
    }

    partial void deferredRenderAOBlurPass()
    {
        m_buffer.BeginSample("ao blur");
        m_buffer.GetTemporaryRT(bluredAoTextureId, width / 2, height / 2, 0, FilterMode.Trilinear, RenderTextureFormat.R16);
        m_buffer.SetRenderTarget(bluredAoTextureId);
        m_buffer.SetGlobalTexture(aoTextureId, aoTextureId);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 2, MeshTopology.Triangles, 3);
        m_buffer.EndSample("ao blur");
        ExecuteBuffer();
    }

    partial void deferredLightVolumeGenPass()
    {
        m_buffer.BeginSample("lightVolume gen");
        m_buffer.GetTemporaryRT(lightVolumeId, width / 4, height / 4, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);
        m_buffer.SetRenderTarget(lightVolumeId);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 3, MeshTopology.Triangles, 3);
        m_buffer.EndSample("lightVolume gen");
        ExecuteBuffer();
    }

    partial void deferredLightVolumeBlurPass()
    {
        m_buffer.BeginSample("lightVolume blur");
        m_buffer.GetTemporaryRT(bluredLightVolumeId, width / 4, height / 4, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);
        m_buffer.SetGlobalMatrix(ditherId, dither);
        m_buffer.SetRenderTarget(bluredLightVolumeId);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 4, MeshTopology.Triangles, 3);
        m_buffer.EndSample("lightVolume blur");
        ExecuteBuffer();
    }

    partial void deferredSSRPass()
    {
        m_buffer.BeginSample("ssr");
        m_buffer.SetRenderTarget(defaultColorBuffer,defaultDepthBuffer);
        m_buffer.SetGlobalTexture(DFColorBufferId, DFColorBufferId);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 5, MeshTopology.Triangles, 3);
        m_buffer.EndSample("ssr");
        ExecuteBuffer();
    }

    partial void BloomGetInput()
    {
        m_buffer.BeginSample("bloom input");
        m_buffer.SetRenderTarget(bloomInput);
        m_buffer.SetGlobalTexture(baseColorBuffer, baseColorBuffer);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 6, MeshTopology.Triangles, 3);
        m_buffer.EndSample("bloom input");
        ExecuteBuffer();
    }

    partial void BloomPass()
    {
        m_buffer.BeginSample("bloom");
        m_buffer.SetRenderTarget(DFColorBufferId);
        m_buffer.SetGlobalTexture(bloomInput, bloomInput);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 7, MeshTopology.Triangles, 3);
        m_buffer.EndSample("bloom");
        ExecuteBuffer();
    }
    partial void CopyColorBuffer()
    {
        m_buffer.BeginSample("copy");
        m_buffer.Blit(baseColorBuffer, m_preTexture[m_aaPingpongFlag]);
        m_aaPingpongFlag = 1 - m_aaPingpongFlag;
        m_buffer.EndSample("copy");
        ExecuteBuffer();
    }
    partial void TAAPass()
    {
        m_buffer.BeginSample("taa pass");
        m_buffer.SetRenderTarget(baseColorBuffer);
        m_buffer.SetGlobalTexture(currentColorBuffer, m_preTexture[1 - m_aaPingpongFlag]);
        m_buffer.SetGlobalTexture(preColorBuffer, m_preTexture[m_aaPingpongFlag]);
        m_buffer.SetGlobalVector(jitterId, jitter);
        m_buffer.SetGlobalMatrix(preV, m_preV[m_aaPingpongFlag]);
        m_buffer.SetGlobalMatrix(preP, m_preP[m_aaPingpongFlag]);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, 8, MeshTopology.Triangles, 3);
        m_buffer.EndSample("taa pass");
        ExecuteBuffer();
    }

    partial void DrawDeferred(bool useDynamicBatching, bool useGPUInstancing)
    {
        initGBuffer();
        deferredRenderGBufferPass(useDynamicBatching, useGPUInstancing);
        deferredLightVolumeGenPass();
        deferredLightVolumeBlurPass();
        deferredRenderAOGenPass();
        deferredRenderAOBlurPass();
        deferredRenderLightingPass();
        CopyColorBuffer();
        TAAPass();
        BloomGetInput();
        BloomPass();
        deferredSSRPass();
    }
}
