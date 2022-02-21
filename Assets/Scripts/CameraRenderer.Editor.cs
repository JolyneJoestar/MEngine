using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class CameraRender
{
    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();

#if UNITY_EDITOR
    static Material m_errorMaterial;

    static ShaderTagId[] m_legayShaderTagIds ={
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    string SampleName { get; set; }
    partial void DrawUnsupportedShaders()
    {
        if (m_errorMaterial == null)
        {
            m_errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawSettings = new DrawingSettings(m_legayShaderTagIds[0], new SortingSettings(m_camera))
        {
            overrideMaterial = m_errorMaterial
        };
        for (int i = 1; i < m_legayShaderTagIds.Length; i++)
        {
            drawSettings.SetShaderPassName(i, m_legayShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        m_context.DrawRenderers(m_cullResult, ref drawSettings, ref filteringSettings);

    }

    partial void DrawGizmos()
    {

        if (UnityEditor.Handles.ShouldRenderGizmos())
        {
            m_context.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
            m_context.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void PrepareForSceneWindow()
    {
        if (m_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(m_camera);
        }
    }

    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        m_buffer.name = SampleName = m_camera.name;
        Profiler.EndSample();
    }
#else
    const string SampleName = m_bufferName;
#endif
}
