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
            //           Debug.Log(dir);
            //            Debug.Log(m_visibleLightColor[i]);
        }
    }
    public void Render(ScriptableRenderContext context, Camera camera,bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
    {
        this.m_context = context;
        this.m_camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.maxDistance))
            return;

        //Shadow Pass
        m_buffer.BeginSample(SampleName);
        ExecuteBuffer();
        m_lighting.SetUp(context,m_cullResult,shadowSettings);
        m_buffer.EndSample(SampleName);

        //Regular Pass
        Setup();
        DrawUnsupportedShaders();
        DrawVisibaleGeometry(useDynamicBatching,useGPUInstancing);
        DrawGizmos();
        m_lighting.Cleanup();
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
        drawingSettings.SetShaderPassName(1, m_customShaderTagId);
        m_context.DrawRenderers(m_cullResult, ref drawingSettings, ref filteringSettings);
        m_context.DrawSkybox(m_camera);
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
