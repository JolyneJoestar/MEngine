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
        baseColorBuffer = Shader.PropertyToID("_BaseColorBuffer");
        

    RenderTargetIdentifier[] m_renderTarget = new RenderTargetIdentifier[geometricTextureId.Length];
    RenderTargetIdentifier[] m_postProcessSrcTex = new RenderTargetIdentifier[2];
    static Vector4[] m_aosample;
    const string m_gbufferName = "GBufferPass";
    RenderTargetIdentifier defaultColorBuffer;
    RenderTargetIdentifier defaultDepthBuffer;
    Texture2D m_noiseTexture = Resources.Load<Texture2D>("Blue_Noise");

    //taa properties
    Matrix4x4 m_preVP;
    RenderTargetIdentifier[] m_preTexture = new RenderTargetIdentifier[2];
    int m_aaPingpongFlag;
        
 //   RenderTexture 
    int width = 2018;
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
        //if(width != m_camera.pixelWidth || height != m_camera.pixelHeight)
        //{
        //if(m_camera.cameraType == CameraType.Game)
        //{
        //    defaultColorBuffer = m_camera.targetTexture.colorBuffer;
        //    defaultDepthBuffer = m_camera.targetTexture.depthBuffer;

        //    width = m_camera.targetTexture.width;
        //    height = m_camera.targetTexture.height;
        //    Debug.Log("3");
        //}
        //else
        //if (m_camera.activeTexture != null)
        //{
        //    defaultColorBuffer = m_camera.activeTexture.colorBuffer;
        //    defaultDepthBuffer = m_camera.activeTexture.depthBuffer;

        //    width = m_camera.scaledPixelWidth;
        //    height = m_camera.scaledPixelHeight;
        //    //width = m_camera.activeTexture.width;
        //    //height = m_camera.activeTexture.height;
        //}
        //else
        //{
        //    defaultColorBuffer = Graphics.activeColorBuffer;
        //    defaultDepthBuffer = Graphics.activeDepthBuffer;
        //    width = m_camera.scaledPixelWidth;
        //    height = m_camera.scaledPixelHeight;
        //    //width = m_camera.pixelWidth;
        //    //height = m_camera.pixelHeight;
        //}

        defaultColorBuffer = new RenderTargetIdentifier("_CameraColorTexture");
        defaultDepthBuffer = new RenderTargetIdentifier("_CameraDepthTexture");

        width = m_camera.scaledPixelWidth;
        height = m_camera.scaledPixelHeight;
        //Debug.Log(width);
        //Debug.Log(height);
        //for(int i = 0; i < geometricTextureId.Length; i++)
        //{
        //    m_buffer.ReleaseTemporaryRT(geometricTextureId[i]);
        //}

        m_buffer.GetTemporaryRT(geometricTextureId[0], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(geometricTextureId[1], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        m_buffer.GetTemporaryRT(geometricTextureId[2], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(geometricTextureId[3], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
        ExecuteBuffer();
      
        for (int i = 0; i < geometricTextureId.Length; i++)
        {
            //m_renderTarget[i] = geometricTextureId[i];
            m_renderTarget[i] = geometricTextureId[i];
        }
        m_buffer.GetTemporaryRT(DFColorBufferId, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(baseColorBuffer, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_buffer.GetTemporaryRT(bloomInput, width / 4, height / 4, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);

        m_preVP = m_camera.previousViewProjectionMatrix;

 //       m_buffer.Blit(defaultColorBuffer, m_preTexture[m_aaPingpongFlag]);
        m_aaPingpongFlag = 1 - m_aaPingpongFlag;

        //m_buffer.GetTemporaryRT(highlightColorBufferId, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

        //m_postProcessSrcTex[0] = DFColorBufferId;
        //m_postProcessSrcTex[1] = highlightColorBufferId;
        //}
        //Debug.Log(m_camera.pixelWidth);
        //Debug.Log(m_camera.pixelHeight);
        //Debug.Log(m_camera.activeTexture.width);
        //Debug.Log(m_camera.activeTexture.height);
        //Debug.Log(m_camera.targetTexture.width);
        //Debug.Log(m_camera.targetTexture.height);
        ExecuteBuffer();
    }

    partial void deferredRenderGBufferPass(bool useDynamicBatching, bool useGPUInstancing)
    {
        m_buffer.SetRenderTarget(m_renderTarget, defaultDepthBuffer);
        m_buffer.ClearRenderTarget(true, true, Color.white);
        ExecuteBuffer();
        var sortingSettings = new SortingSettings(m_camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(m_gBufferPassId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
 //           perObjectData = PerObjectData.Lightmaps | PerObjectData.LightProbe | PerObjectData.LightProbeProxyVolume
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
        m_buffer.SetRenderTarget(defaultColorBuffer);
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



    partial void DrawDeferred(bool useDynamicBatching, bool useGPUInstancing)
    {
        initGBuffer();
        deferredRenderGBufferPass(useDynamicBatching, useGPUInstancing);
        deferredLightVolumeGenPass();
        deferredLightVolumeBlurPass();
        deferredRenderAOGenPass();
        deferredRenderAOBlurPass();
        deferredRenderLightingPass();
        BloomGetInput();
        BloomPass();
        deferredSSRPass();
    }
}
