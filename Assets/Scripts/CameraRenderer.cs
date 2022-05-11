using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public partial class CameraRender {
    const int m_maxVisibleLights = 4;

    Vector4[] m_visibleLightColor = new Vector4[m_maxVisibleLights];
    Vector4[] m_visibleLightDirection = new Vector4[m_maxVisibleLights];

    ScriptableRenderContext m_context;
    Camera m_camera;

    const string m_bufferName = "Render Camera";
    CommandBuffer m_buffer = new CommandBuffer { name = m_bufferName };

    CullingResults m_cullResult;

    Lighting m_lighting = new Lighting();
    static ShaderTagId m_customShaderTagId = new ShaderTagId("SPRDefaultLegay");
    static ShaderTagId m_particlesTagId = new ShaderTagId("ParticlesRender");
    static ShaderTagId m_nprOutlineId = new ShaderTagId("NPROutline");
    static Matrix4x4 dither = new Matrix4x4
    (
       new Vector4(0,       0.5f,    0.125f,  0.625f),
       new Vector4(0.75f,    0.25f,   0.875f,  0.375f),
       new Vector4(0.1875f,  0.6875f, 0.0625f, 0.5625f),
       new Vector4(0.9375f,  0.4375f, 0.8125f, 0.3125f)
    );

    static string m_nprKeywords = "_NPRLIGHTING";
    static int m_colorStreetId = Shader.PropertyToID("_ColorStreet"),
        m_specularSegmentId = Shader.PropertyToID("_SpecularSegment"),
        m_outlineWidthId = Shader.PropertyToID("_OutlineWidth"),
        m_outlineColorId = Shader.PropertyToID("_OutlineColor");
    NPRSetting m_nprSettings;
    //   RenderTexture
    Vector2Int screenSize = new Vector2Int(2048, 2048);
    RenderTexture m_carmeraTarget;

    void ConfigerLights(ref CullingResults cull)
    {
        for (int i = 0; i < cull.visibleLights.Length; i++)
        {
            if (i >= m_maxVisibleLights)
                break;
            VisibleLight light = cull.visibleLights[i];
            m_visibleLightColor[i] = light.finalColor;
            Vector4 dir = light.localToWorldMatrix.GetColumn(2);
            dir.x = -dir.x;
            dir.y = -dir.y;
            dir.z = -dir.z;
            m_visibleLightDirection[i] = dir;
            //       Debug.Log(dir);
            //       Debug.Log(m_visibleLightColor[i]);
        }
    }
    void SetData()
    {
        if (screenSize.x != m_camera.scaledPixelWidth || screenSize.y != m_camera.scaledPixelHeight)
        {
            screenSize.x = m_camera.scaledPixelWidth;
            screenSize.y = m_camera.scaledPixelHeight;
        }
    }
    public void Render(ScriptableRenderContext context, Camera camera,bool useDynamicBatching, bool useGPUInstancing, bool m_useDeferredRendering , ShadowSettings shadowSettings, ShadowPostSettings shadowPostSettings,
        NPRSetting nprSetting, AOSetting aoSetting, float ssrStepRatio)
    {
        this.m_context = context;
        this.m_camera = camera;
        this.m_nprSettings = nprSetting;
        this.m_aoSettings = aoSetting;
        this.m_ssrStepRatio = ssrStepRatio;

        SetData();

        m_carmeraTarget = RenderTextures.Instance.GetTemperory(m_camera.name, screenSize.x, screenSize.y, 32, RenderTextureFormat.Default);
        m_camera.SetTargetBuffers(m_carmeraTarget.colorBuffer, m_carmeraTarget.depthBuffer);

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.maxDistance))
            return;
        
        //Shadow Pass
        m_buffer.BeginSample(SampleName);
        m_lighting.SetUp(context,m_cullResult,shadowSettings,camera, shadowPostSettings);
        m_buffer.EndSample(SampleName);
        ExecuteBuffer();

        //Regular Pass
        Setup();
        if(m_useDeferredRendering)
        {
            DrawDeferred(useDynamicBatching, useGPUInstancing);
        }
        else
        {
            DrawVisibaleGeometry(useDynamicBatching, useGPUInstancing);
        }
        if (m_nprSettings.EnableNPR)
        {
            DrawNPROutline(useDynamicBatching, useGPUInstancing);
        }
        DrawParticles();
        PostProccess();
        m_context.DrawSkybox(m_camera);
        DrawUnsupportedShaders();
        DrawGizmos();
        Cleanup();
        Submit();
    }

    void DrawVisibaleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(m_camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(m_customShaderTagId, sortingSettings) {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
            perObjectData = PerObjectData.Lightmaps | PerObjectData.LightProbe | PerObjectData.LightProbeProxyVolume
        };
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        drawingSettings.SetShaderPassName(0, m_customShaderTagId);
        if (m_nprSettings.EnableNPR)
        {
            m_buffer.SetGlobalFloat(m_outlineWidthId, m_nprSettings.OutlineWidth);
            m_buffer.SetGlobalColor(m_outlineColorId, m_nprSettings.OutlineColor);
            m_buffer.SetGlobalFloat(m_specularSegmentId, m_nprSettings.SpecularSegment);
            m_buffer.SetGlobalVector(m_colorStreetId, m_nprSettings.ColorStreet);
            m_buffer.EnableShaderKeyword(m_nprKeywords);
        }
        else
        {
            m_buffer.DisableShaderKeyword(m_nprKeywords);
        }
        ExecuteBuffer();
        m_context.DrawRenderers(m_cullResult, ref drawingSettings, ref filteringSettings);
       
    }

    void DrawNPROutline(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(m_camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(m_customShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
            perObjectData = PerObjectData.Lightmaps | PerObjectData.LightProbe | PerObjectData.LightProbeProxyVolume
        };
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        drawingSettings.SetShaderPassName(4, m_nprOutlineId);
        m_context.DrawRenderers(m_cullResult, ref drawingSettings, ref filteringSettings);
    }

    void DrawParticles()
    {
        ParticleEmitterManager.Instance.render(m_buffer, m_context, m_camera);
    }
    void Submit()
    {
        m_buffer.EndSample(SampleName);
        ExecuteBuffer();
        m_context.Submit();
    }
    void Setup()
    {
        m_context.SetupCameraProperties(m_camera);
        m_buffer.ClearRenderTarget(true, true, Color.clear);
        m_buffer.BeginSample(SampleName);
        ExecuteBuffer();

    }

    void Cleanup()
    {
        m_lighting.Cleanup();
        Cleanupdr();
    }
    void ExecuteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }
    bool Cull(float maxShadowDistance)
    {
        if (m_camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
        {
            cullingParameters.shadowDistance = Mathf.Min(maxShadowDistance, m_camera.farClipPlane);
            m_cullResult = m_context.Cull(ref cullingParameters);
            return true;
        }
        return false;

    }
}
