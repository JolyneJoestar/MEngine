using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    const string bufferName = "Shadows";
    const int maxShadowedDirectionalLightCount = 1;

    struct ShadowedDirectionalLight
    {
        public int m_visibleLightIndex;
    }
    ShadowedDirectionalLight[] m_shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };
    int m_shadowedDirectionalLightCount;

    ScriptableRenderContext m_context;

    CullingResults m_cullingResults;

    ShadowSettings m_shadowSettings;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.m_context = context;
        this.m_cullingResults = cullingResults;
        this.m_shadowSettings = shadowSettings;
        m_shadowedDirectionalLightCount = 0;
    }

    void ExecuteBuffer()
    {
        m_context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (m_shadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            m_cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            m_shadowedDirectionalLights[m_shadowedDirectionalLightCount++] = new ShadowedDirectionalLight { m_visibleLightIndex = visibleLightIndex };
        }
    }
    public void Render()
    {
        if(m_shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
    }

    void RenderDirectionalShadows()
    {

    }
}

