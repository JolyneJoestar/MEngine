using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRender
{
    enum DeferredRenderPass
    {
        DeferredRenderPass_Lighting = 0,
        DeferredRenderPass_SSAOGen,
        DeferredRenderPass_AOBlur,
        DeferredRenderPass_LightVolumeGen,
        DeferredRenderPass_LightVolumeBlur,
        DeferredRenderPass_SSR,
        DeferredRenderPass_TAA,
        DeferredRenderPass_HBAOGen
    }
    partial void initGBuffer();
    partial void deferredRenderGBufferPass(bool useDynamicBatching, bool useGPUInstancing);
    partial void deferredRenderLightingPass();
    partial void deferredRenderAOGenPass();
    partial void SSAOPass();
    partial void HBAOPass();
    partial void deferredRenderAOBlurPass();
    partial void deferredLightVolumeGenPass();
    partial void deferredLightVolumeBlurPass();
    partial void deferredSSRPass();
    partial void CopyColorBuffer();
    partial void TAAPass();
    partial void DrawDeferred(bool useDynamicBatching, bool useGPUInstancings);
    partial void Cleanupdr();

    static Shader m_shader = Shader.Find("MyPipeline/DeferredRender");
    Material m_deferredRenderingMaterial = new Material(m_shader);
    static ShaderTagId m_gBufferPassId = new ShaderTagId("gBufferPass");
    
    static MyShaderProperties m_shaderProperties = new MyShaderProperties();
    static MyShaderBuffers m_shaderBuffers = new MyShaderBuffers();

    RenderTargetIdentifier[] m_renderTarget = new RenderTargetIdentifier[m_shaderBuffers.gbuffers.MaxGBUfferNum];
    static Vector4[] m_aosample;
    AOSetting m_aoSettings;
    const string m_gbufferName = "GBufferPass";

    Texture2D m_noiseTexture = Resources.Load<Texture2D>("Blue_Noise");

    //taa properties
    Matrix4x4[] m_preV = new Matrix4x4[2];
    Matrix4x4[] m_preP = new Matrix4x4[2];
    static int[] m_preTexture = {
            m_shaderBuffers.preColorTextureID,
            m_shaderBuffers.currentColorTextureID
        };

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
    //HBAO
    Vector4 m_zBufferParam;
    Vector4 m_UV2View;
    Vector4 m_texelSize;
    Vector2 m_downScaleTexelSize;
    float m_radiusPixel;
    float m_ssrStepRatio;

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

        m_buffer.GetTemporaryRT(m_preTexture[0], screenSize.x, screenSize.y, 32, FilterMode.Point, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(m_preTexture[1], screenSize.x, screenSize.y, 32, FilterMode.Point, RenderTextureFormat.ARGB32);

        frameCount++;
        int index = frameCount % 8;
        jitter = new Vector2((HaltonSequence[index].x - 0.5f) / screenSize.x, (HaltonSequence[index].y - 0.5f) / screenSize.y);

        m_preV[m_aaPingpongFlag] = m_camera.worldToCameraMatrix;
        m_preP[m_aaPingpongFlag] = m_camera.projectionMatrix;
        

        int preIndex = (frameCount - 1) % 8;
        Vector2 preJitter = new Vector2((HaltonSequence[preIndex].x - 0.5f) / screenSize.x, (HaltonSequence[preIndex].y - 0.5f) / screenSize.y);
        m_preP[m_aaPingpongFlag].m02 -= preJitter.x * 2.0f;
        m_preP[m_aaPingpongFlag].m12 -= preJitter.y * 2.0f;
        
        m_preP[m_aaPingpongFlag].m02 += jitter.x * 2.0f;
        m_preP[m_aaPingpongFlag].m12 += jitter.y * 2.0f;

        //screen parameters init
        m_zBufferParam.x = 1.0f / (-m_preP[m_aaPingpongFlag].m23 / 2.0f / m_camera.farClipPlane);
        m_zBufferParam.y = 1.0f;
        m_zBufferParam.z = m_zBufferParam.x / m_camera.farClipPlane;
        m_zBufferParam.w = 1.0f / m_camera.farClipPlane;
        var tanHalfFovY = Mathf.Tan(m_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var tanHalfFovX = tanHalfFovY * ((float)screenSize.x / screenSize.y);
        m_UV2View = new Vector4(2 * tanHalfFovX, 2 * tanHalfFovY, -tanHalfFovX, -tanHalfFovY);
        m_texelSize = new Vector4(1f / screenSize.x, 1f / screenSize.y, screenSize.x, screenSize.y);
        m_downScaleTexelSize = new Vector2(4f / screenSize.x, 4f / screenSize.y);
        m_radiusPixel = m_camera.pixelHeight * m_aoSettings.Radius / tanHalfFovY / 2;
        //m_camera.projectionMatrix = m_preP[m_aaPingpongFlag];

 //       m_buffer.GetTemporaryRT(m_shaderBuffers.depthBufferID, screenSize.x, screenSize.y, 0, FilterMode.Point, RenderTextureFormat.Depth);
        m_buffer.GetTemporaryRT(m_shaderBuffers.gbuffers.GPositionID, screenSize.x, screenSize.y, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(m_shaderBuffers.gbuffers.GNormalID, screenSize.x, screenSize.y, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(m_shaderBuffers.gbuffers.GAlbedoID, screenSize.x, screenSize.y, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(m_shaderBuffers.gbuffers.GMaterialID, screenSize.x, screenSize.y, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
        ExecuteBuffer();

        for (int i = 0; i < m_shaderBuffers.gbuffers.MaxGBUfferNum; i++)
        {
            m_renderTarget[i] = m_shaderBuffers.gbuffers[i];
        }
        m_buffer.GetTemporaryRT(m_shaderBuffers.dfColorTextureID, screenSize.x, screenSize.y, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        

        ExecuteBuffer();
        //RenderTargetIdentifier db = defaultRenderBufferId;
        //m_camera.targetTexture  = new RenderTexture(db);
    }

    partial void deferredRenderGBufferPass(bool useDynamicBatching, bool useGPUInstancing)
    {
        m_buffer.SetRenderTarget(m_renderTarget, m_carmeraTarget.depthBuffer);
        m_buffer.ClearRenderTarget(true, true, Color.white);
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
    partial void Cleanupdr()
    {
        for (int i = 0; i < m_shaderBuffers.gbuffers.MaxGBUfferNum; i++)
        {
            m_buffer.ReleaseTemporaryRT(m_shaderBuffers.gbuffers[i]);
        }
        m_buffer.ReleaseTemporaryRT(m_preTexture[0]);
        m_buffer.ReleaseTemporaryRT(m_preTexture[1]);
        m_buffer.ReleaseTemporaryRT(m_shaderBuffers.bluredAoTextureID);
        m_buffer.ReleaseTemporaryRT(m_shaderBuffers.lightVolumeTextureID);
        m_buffer.ReleaseTemporaryRT(m_shaderBuffers.aoTextureID);
        m_buffer.ReleaseTemporaryRT(m_shaderBuffers.bluredLightVolumeTextureID);
        m_buffer.ReleaseTemporaryRT(m_shaderBuffers.dfColorTextureID);
        m_buffer.ReleaseTemporaryRT(m_shaderBuffers.baseColorTextureID);
//        m_buffer.ReleaseTemporaryRT(m_shaderBuffers.depthBufferID);
        ExecuteBuffer();
    }
    partial void deferredRenderLightingPass()
    {
        m_buffer.BeginSample("deferred lighting pass");
        m_buffer.SetRenderTarget(m_carmeraTarget);
        for (int i = 0; i < m_renderTarget.Length; i++)
        {
            m_buffer.SetGlobalTexture(m_shaderBuffers.gbuffers[i], m_renderTarget[i]);
        }
        m_buffer.SetGlobalTexture(m_shaderBuffers.bluredLightVolumeTextureID, m_shaderBuffers.bluredLightVolumeTextureID);
        m_buffer.SetGlobalTexture(m_shaderBuffers.bluredAoTextureID, m_shaderBuffers.bluredAoTextureID);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, (int)DeferredRenderPass.DeferredRenderPass_Lighting, MeshTopology.Triangles, 3);
        m_buffer.EndSample("deferred lighting pass");
        ExecuteBuffer();
    }
    partial void deferredRenderAOGenPass()
    {
        HBAOPass();
    }
    partial void SSAOPass()
    {
        m_buffer.BeginSample("ssao gen");
        m_buffer.GetTemporaryRT(m_shaderBuffers.aoTextureID, screenSize.x / 2, screenSize.y / 2, 0, FilterMode.Trilinear, RenderTextureFormat.R16);
        m_buffer.SetRenderTarget(m_shaderBuffers.aoTextureID);
        for (int i = 0; i < 2; i++)
        {
            m_buffer.SetGlobalTexture(m_shaderBuffers.gbuffers[i], m_renderTarget[i]);
        }
        m_buffer.SetGlobalTexture(m_shaderBuffers.noiseTextureID, m_noiseTexture);
        m_buffer.ClearRenderTarget(false, true, Color.white);
        m_buffer.SetGlobalVectorArray(m_shaderProperties.samplesID, m_aosample);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, (int)DeferredRenderPass.DeferredRenderPass_SSAOGen, MeshTopology.Triangles, 3);
        m_buffer.EndSample("ssao gen");
        ExecuteBuffer();
    }
    partial void HBAOPass()
    {
        m_buffer.BeginSample("hbao gen");
        m_buffer.GetTemporaryRT(m_shaderBuffers.aoTextureID, screenSize.x / 2, screenSize.y / 2, 0, FilterMode.Trilinear, RenderTextureFormat.R16);
        m_buffer.SetRenderTarget(m_shaderBuffers.aoTextureID);
        for (int i = 0; i < 2; i++)
        {
            m_buffer.SetGlobalTexture(m_shaderBuffers.gbuffers[i], m_renderTarget[i]);
        }
        m_buffer.SetGlobalVector(m_shaderProperties.screenProperties.uv2ViewID, m_UV2View);
        m_buffer.SetGlobalVector(m_shaderProperties.screenProperties.zBufferParamID, m_zBufferParam);
        m_buffer.SetGlobalVector(m_shaderProperties.screenProperties.texelSizeID, m_texelSize);
        m_buffer.SetGlobalFloat(m_shaderProperties.hbaoPropreties.radiusID, m_aoSettings.Radius);
        m_buffer.SetGlobalFloat(m_shaderProperties.hbaoPropreties.radiusPixelID, m_radiusPixel);
        m_buffer.SetGlobalFloat(m_shaderProperties.hbaoPropreties.maxRadiusPixelID, m_aoSettings.MaxRadiusPixel);
        m_buffer.SetGlobalFloat(m_shaderProperties.hbaoPropreties.angleBiasID, m_aoSettings.AngleBias);
        m_buffer.SetGlobalFloat(m_shaderProperties.hbaoPropreties.aoStrengthID, m_aoSettings.AOStrength);
//        m_buffer.SetGlobalTexture(m_shaderBuffers.depthBufferID, m_shaderBuffers.depthBufferID);
        m_buffer.SetGlobalTexture(m_shaderBuffers.noiseTextureID, m_noiseTexture);
        m_buffer.ClearRenderTarget(false, true, Color.white);
        m_buffer.SetGlobalVectorArray(m_shaderProperties.samplesID, m_aosample);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, (int)DeferredRenderPass.DeferredRenderPass_HBAOGen, MeshTopology.Triangles, 3);
        m_buffer.EndSample("hbao gen");
        ExecuteBuffer();
    }
    partial void deferredRenderAOBlurPass()
    {
        m_buffer.BeginSample("ao blur");
        m_buffer.GetTemporaryRT(m_shaderBuffers.bluredAoTextureID, screenSize.x / 2, screenSize.y / 2, 0, FilterMode.Trilinear, RenderTextureFormat.R16);
        m_buffer.SetRenderTarget(m_shaderBuffers.bluredAoTextureID);
        m_buffer.SetGlobalTexture(m_shaderBuffers.aoTextureID, m_shaderBuffers.aoTextureID);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, (int)DeferredRenderPass.DeferredRenderPass_AOBlur, MeshTopology.Triangles, 3);
        m_buffer.EndSample("ao blur");
        ExecuteBuffer();
    }
    partial void deferredLightVolumeGenPass()
    {
        m_buffer.BeginSample("lightVolume gen");
        m_buffer.GetTemporaryRT(m_shaderBuffers.lightVolumeTextureID, screenSize.x / 4, screenSize.y / 4, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);
        m_buffer.SetRenderTarget(m_shaderBuffers.lightVolumeTextureID);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, (int)DeferredRenderPass.DeferredRenderPass_LightVolumeGen, MeshTopology.Triangles, 3);
        m_buffer.EndSample("lightVolume gen");
        ExecuteBuffer();
    }

    partial void deferredLightVolumeBlurPass()
    {
        m_buffer.BeginSample("lightVolume blur");
        m_buffer.GetTemporaryRT(m_shaderBuffers.bluredLightVolumeTextureID, screenSize.x / 4, screenSize.y / 4, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);
        m_buffer.SetGlobalMatrix(m_shaderProperties.ditherID, dither);
        m_buffer.SetRenderTarget(m_shaderBuffers.bluredLightVolumeTextureID);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, (int)DeferredRenderPass.DeferredRenderPass_LightVolumeBlur, MeshTopology.Triangles, 3);
        m_buffer.EndSample("lightVolume blur");
        ExecuteBuffer();
    }

    partial void deferredSSRPass()
    {
        m_buffer.BeginSample("ssr");
        m_buffer.SetRenderTarget(m_carmeraTarget);
        m_buffer.SetGlobalTexture(m_shaderBuffers.dfColorTextureID, m_carmeraTarget);
        m_buffer.SetGlobalFloat(m_shaderProperties.ssrStepRatioID, m_ssrStepRatio);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, (int)DeferredRenderPass.DeferredRenderPass_SSR, MeshTopology.Triangles, 3);
        m_buffer.EndSample("ssr");
        ExecuteBuffer();
    }
    partial void CopyColorBuffer()
    {
        m_buffer.BeginSample("copy");
        m_buffer.Blit(m_carmeraTarget, m_preTexture[m_aaPingpongFlag]);
        m_aaPingpongFlag = 1 - m_aaPingpongFlag;
        m_buffer.EndSample("copy");
        ExecuteBuffer();
    }
    partial void TAAPass()
    {
        m_buffer.BeginSample("taa pass");
        m_buffer.SetRenderTarget(m_carmeraTarget);
        m_buffer.SetGlobalTexture(m_shaderBuffers.currentColorTextureID, m_preTexture[1 - m_aaPingpongFlag]);
        m_buffer.SetGlobalTexture(m_shaderBuffers.preColorTextureID, m_preTexture[m_aaPingpongFlag]);
        m_buffer.SetGlobalVector(m_shaderProperties.taaPropreties.jitterID, jitter);
        m_buffer.SetGlobalMatrix(m_shaderProperties.taaPropreties.preVID, m_preV[m_aaPingpongFlag]);
        m_buffer.SetGlobalMatrix(m_shaderProperties.taaPropreties.prePID, m_preP[m_aaPingpongFlag]);
        m_buffer.DrawProcedural(Matrix4x4.identity, m_deferredRenderingMaterial, (int)DeferredRenderPass.DeferredRenderPass_TAA, MeshTopology.Triangles, 3);
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
        //deferredSSRPass();
    }
}
