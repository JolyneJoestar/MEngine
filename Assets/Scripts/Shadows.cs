using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    const string bufferName = "Shadows";
    const int maxShadowedDirectionalLightCount = 4, maxCascades = 4;

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
        cascadeCountId = Shader.PropertyToID("_CascadeCount"),
        cascadeCullingSphereId = Shader.PropertyToID("_CascadeCullingSphere"),
        shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade"),
        cascadeDataId = Shader.PropertyToID("_CascadeData"),
        bluredDirShadowAtlasId = Shader.PropertyToID("_BluredDirShadowAtlasId"),
        shadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");

    static int[] FourierBufferDataId =
    {
        Shader.PropertyToID("_FourierBufferOneId"),
        Shader.PropertyToID("_FourierBufferTwoId"),
        Shader.PropertyToID("_FourierBufferThreeId"),
        Shader.PropertyToID("_FourierBufferFourId")
    };

    static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];
    static Vector4[] cascadeCullingSphere = new Vector4[maxCascades],
                    cascadeData = new Vector4[maxCascades];

    ShadowBlur shadowBlur = new ShadowBlur();
    ConvolutionShadowMap convolutionShadowMap = new ConvolutionShadowMap();
    static string[] directionalFilterKeywords =
    {
        "_DIRECTIONAL_PCF3",
        "_DIRECTIONAL_PCF5",
        "_DIRECTIONAL_PCF7",
    };

    static string[] cascadeBlendKeywords =
    {
        "_CASCADE_BLEND_SOFT",
        "_CASCADE_BLEND_DITHER"
    };
    static string[] shadowTypeKeyWords =
    {
        "_PCF",
        "_VSM",
        "_ESM",
        "_PCSS",
        "_CSM"
    };
    struct ShadowedDirectionalLight
    {
        public int m_visibleLightIndex;
        public float m_slopeScaleBias;
        public float m_nearPlaneOffset;
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

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings, Camera camera, ShadowPostSettings shadowPostSettings)
    {
        this.m_context = context;
        this.m_cullingResults = cullingResults;
        this.m_shadowSettings = shadowSettings;
        shadowBlur.Setup(context, camera, shadowPostSettings);
        convolutionShadowMap.Setup(context, camera, shadowPostSettings);
        m_shadowedDirectionalLightCount = 0;
    }

    void ExecuteBuffer()
    {
        m_context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public Vector3 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (m_shadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            m_cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            m_shadowedDirectionalLights[m_shadowedDirectionalLightCount] = new ShadowedDirectionalLight { m_visibleLightIndex = visibleLightIndex, m_slopeScaleBias = light.shadowBias, m_nearPlaneOffset = light.shadowNearPlane };
            return new Vector3(light.shadowStrength, m_shadowSettings.directional.cascadeCount * m_shadowedDirectionalLightCount++, light.shadowNormalBias);
        }
        return Vector3.zero;
    }
    public void Render()
    {
        if(m_shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            //we do this because of there will be some problems when shadow sampling in webGL 2.0
            buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)m_shadowSettings.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        buffer.GetTemporaryRT(bluredDirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.RG32);
        
        buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();

        int tiles = m_shadowedDirectionalLightCount * m_shadowSettings.directional.cascadeCount;
        int splite = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        int tileSize = atlasSize / splite;
        for(int i = 0; i < m_shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, splite, tileSize);
        }
        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        float f = 1f - m_shadowSettings.directional.cascadeFade;
        buffer.SetGlobalVector(shadowDistanceFadeId, new Vector4(1f / m_shadowSettings.maxDistance, 1f / m_shadowSettings.distanceFade, 1f / (1f - f * f)));
        SetKeywords(directionalFilterKeywords, (int)m_shadowSettings.directional.filter - 1);
        SetKeywords(shadowTypeKeyWords, (int)m_shadowSettings.directional.softshadow);
        SetKeywords(cascadeBlendKeywords, (int)m_shadowSettings.directional.cascadeBlend - 1);

        buffer.SetGlobalVector(shadowAtlasSizeId, new Vector4(atlasSize, 1f / atlasSize));
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        if (shadowBlur.IsActive && m_shadowSettings.directional.softshadow == ShadowSettings.Directional.SoftShodowType.VSM)
        {
            shadowBlur.Render(dirShadowAtlasId, bluredDirShadowAtlasId, (int)m_shadowSettings.directional.atlasSize);
        }
        else if(convolutionShadowMap.IsActive && m_shadowSettings.directional.softshadow == ShadowSettings.Directional.SoftShodowType.CSM)
        {
            for(int i = 0; i < FourierBufferDataId.Length; i++)
            {
                buffer.GetTemporaryRT(FourierBufferDataId[i], atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            }
            convolutionShadowMap.Render(dirShadowAtlasId, FourierBufferDataId, (int)m_shadowSettings.directional.atlasSize);
        }
    }

    Vector2 SetTileViewPort(int index, int split, int tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }

    void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
    {
        float texelSize = 2f * cullingSphere.w / tileSize;
        float filterSize = texelSize * ((float)m_shadowSettings.directional.filter + 1f);
        texelSize *= 1.4142136f;
        cullingSphere.w -= filterSize;
        cullingSphere.w *= cullingSphere.w;
        cascadeData[index] = new Vector4(1f / cullingSphere.w, filterSize * 1.4142136f);
        cascadeCullingSphere[index] = cullingSphere;
    }

    void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = m_shadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(m_cullingResults, light.m_visibleLightIndex);

        int cascadeCount = m_shadowSettings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 cascadeRatios = m_shadowSettings.directional.CascadeRatios;

        float cullingFactor = Mathf.Max(0f, 0.8f - m_shadowSettings.directional.cascadeFade);
        for (int i = 0; i < cascadeCount; i++)
        {

            m_cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.m_visibleLightIndex, i, cascadeCount, cascadeRatios, tileSize, light.m_nearPlaneOffset,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData
                );

            int tileIndex = tileOffset + i;
            splitData.shadowCascadeBlendCullingFactor = cullingFactor;
            shadowSettings.splitData = splitData;
            if(index == 0)
            {
                SetCascadeData(i, splitData.cullingSphere, tileSize);
            }
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewPort(tileIndex, split, tileSize), split);
            buffer.SetGlobalInt(cascadeCountId, m_shadowSettings.directional.cascadeCount);
            buffer.SetGlobalVectorArray(cascadeCullingSphereId, cascadeCullingSphere);
            buffer.SetGlobalVectorArray(cascadeDataId, cascadeData);
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            buffer.SetGlobalDepthBias(0, light.m_slopeScaleBias);
            ExecuteBuffer();
            m_context.DrawShadows(ref shadowSettings);
            buffer.SetGlobalDepthBias(0, 0f);
        }
    }
    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if(SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }

    void SetKeywords(string[] keywords, int enableIndex)
    {
        for (int i = 0; i < keywords.Length; i++)
        {
            if (i == enableIndex)
            {
                buffer.EnableShaderKeyword(keywords[i]);
            }
            else
            {
                buffer.DisableShaderKeyword(keywords[i]);
            }
        }
    }

    public int GetDirShadowAtlasId()
    {
        return dirShadowAtlasId;
    }

    public int GetBluredDirShadowAtlasId()
    {
        return bluredDirShadowAtlasId;
    }

    public int GetAtlasSize()
    {
        return (int)m_shadowSettings.directional.atlasSize;
    }
    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        buffer.ReleaseTemporaryRT(bluredDirShadowAtlasId);
        ExecuteBuffer();
    }
}

