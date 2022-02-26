#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#if defined(_DIRECTIONAL_PCF3)
	#define DIRECTIONAL_FILTER_SAMPLES 4
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
	#define DIRECTIONAL_FILTER_SAMPLES 9
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
	#define DIRECTIONAL_FILTER_SAMPLES 16
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);
SAMPLER(sampler_DirectionalShadowAtlas);

TEXTURE2D(_BluredDirShadowAtlasId);
SAMPLER(sampler_BluredDirShadowAtlasId);

CBUFFER_START(_CustomShadows)
int _CascadeCount;
float4 _CascadeCullingSphere[MAX_CASCADE_COUNT];
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
float4 _CascadeData[MAX_CASCADE_COUNT];
float4 _ShadowDistanceFade;
float4 _ShadowAtlasSize;
CBUFFER_END

struct DirectionalShadowData {
	float strength;
	int tileIndex;
    float normalBias;
};

struct ShadowData
{
    int cascadeIndex;
    float cascadeBlend;
    float strength;
};

float SamplerDirectionalShadowAtlas(float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float FilterDirectionalShadow(float3 positionSTS)
{
#if defined(DIRECTIONAL_FILTER_SETUP)
		float weights[DIRECTIONAL_FILTER_SAMPLES];
		float2 positions[DIRECTIONAL_FILTER_SAMPLES];
		float4 size = _ShadowAtlasSize.yyxx;
		DIRECTIONAL_FILTER_SETUP(size, positionSTS.xy, weights, positions);
		float shadow = 0;
		float dBlocker = 0;
		for(int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++)
		{
			dBlocker += SAMPLE_TEXTURE2D(_BluredDirShadowAtlasId, sampler_BluredDirShadowAtlasId, positions[i].xy).r;
			shadow += weights[i] * SamplerDirectionalShadowAtlas(float3(positions[i].xy, positionSTS.z));
		}
		dBlocker /= DIRECTIONAL_FILTER_SAMPLES;
		float dReceiver = SAMPLE_TEXTURE2D(_DirectionalShadowAtlas, sampler_DirectionalShadowAtlas,positionSTS.xy).r;
		float weight = (dReceiver - dBlocker) * 100.0 / dBlocker;
		return shadow;
#else
    return SamplerDirectionalShadowAtlas(positionSTS);
#endif
}

float GetDirectionalShadowAttenuation(DirectionalShadowData dirData,ShadowData shadowData, Surface surfaceWS)
{
    if (dirData.strength <= 0.0)
		return 1.0;
    float3 normalBias = surfaceWS.normal * _CascadeData[shadowData.cascadeIndex].y * dirData.normalBias;
    float3 positionSTS = mul(_DirectionalShadowMatrices[dirData.tileIndex], float4(surfaceWS.position + normalBias, 1.0)).xyz;
    float shadow = FilterDirectionalShadow(positionSTS);
    if (shadowData.cascadeBlend < 1.0)
    {
        normalBias = surfaceWS.normal * _CascadeData[shadowData.cascadeIndex + 1].y * dirData.normalBias;
        positionSTS = mul(_DirectionalShadowMatrices[dirData.tileIndex + 1], float4(surfaceWS.position + normalBias, 1.0)).xyz;
        shadow = lerp(FilterDirectionalShadow(positionSTS), shadow, shadowData.cascadeBlend);
    }
    return lerp(1.0, shadow, dirData.strength);
}

#endif //CUSTOM_SHADOWS_INCLUDED