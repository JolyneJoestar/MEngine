
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    const int m_maxVisibleLightCount = 4;
    const string m_bufferName = "Lighting";
    CullingResults m_cullingResults;
    CommandBuffer m_buffer = new CommandBuffer { name = m_bufferName };
    static int
        m_visibleLightCountId = Shader.PropertyToID("MVisibleLightCount"),
        m_dirLightColorId = Shader.PropertyToID("MVisibleLightColors"),
        m_dirLightDirectionId = Shader.PropertyToID("MVisibleLightDirecitons"),
        m_dirLightShadowDataId = Shader.PropertyToID("MDirectionalLightShadowData");

    Shadows m_shadows = new Shadows();

    static Vector4[]
        m_dirLightColors = new Vector4[m_maxVisibleLightCount],
        m_dirLightDirecitons = new Vector4[m_maxVisibleLightCount],
        m_dirLightShadowData = new Vector4[m_maxVisibleLightCount];
    public void SetUp(ScriptableRenderContext context,CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.m_cullingResults = cullingResults;
        m_buffer.BeginSample(m_bufferName);
        m_shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights();
        m_shadows.Render();
        m_buffer.EndSample(m_bufferName);
        context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();

    }
    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = m_cullingResults.visibleLights;
        int i = 0;
        for (; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            SetDirectionLight(i,ref visibleLight);
            if (i >= m_maxVisibleLightCount - 1)
                break;
        }
        m_buffer.SetGlobalInt(m_visibleLightCountId, i);
        m_buffer.SetGlobalVectorArray(m_dirLightColorId, m_dirLightColors);
        m_buffer.SetGlobalVectorArray(m_dirLightDirectionId, m_dirLightDirecitons);
        m_buffer.SetGlobalVectorArray(m_dirLightShadowDataId, m_dirLightShadowData);
    }

    void SetDirectionLight(int index,ref VisibleLight visibleLight)
    {
        //       UnityEngine.Light light = RenderSettings.sun;
        //m_buffer.SetGlobalVector(m_dirLightColorId, visibleLight.finalColor);
        //m_buffer.SetGlobalVector(m_dirLightDiretion, -visibleLight.localToWorldMatrix.GetColumn(2));
        m_dirLightColors[index] = visibleLight.finalColor;
        m_dirLightDirecitons[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        m_dirLightShadowData[index] = m_shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }

    public void Cleanup()
    {
        m_shadows.Cleanup();
    }
}
