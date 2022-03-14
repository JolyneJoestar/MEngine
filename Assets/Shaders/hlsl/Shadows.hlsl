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

#if defined(_VSM)
TEXTURE2D(_BluredDirShadowAtlasId);
SAMPLER(sampler_BluredDirShadowAtlasId);
#endif

#if defined(_CSM)
TEXTURE2D(_FourierBufferOneId);
SAMPLER(sampler_FourierBufferOneId);
TEXTURE2D(_FourierBufferTwoId);
SAMPLER(sampler_FourierBufferTwoId);
TEXTURE2D(_FourierBufferThreeId);
SAMPLER(sampler_FourierBufferThreeId);
TEXTURE2D(_FourierBufferFourId);
SAMPLER(sampler_FourierBufferFourId);
#endif

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);
SAMPLER(sampler_DirectionalShadowAtlas);

float SamplerDirectionalShadowAtlas(float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

#if defined(_PCSS)
#include "PCSS.hlsl"
#endif

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


float FilterDirectionalShadow(float3 positionSTS)
{
#if defined(DIRECTIONAL_FILTER_SETUP)
    #if defined(_VSM)
		    float2 depth = SAMPLE_TEXTURE2D(_BluredDirShadowAtlasId, sampler_BluredDirShadowAtlasId, positionSTS.xy).rg;
		    float convi = max(0.00000001,depth.y - depth.x * depth.x);
		    float delta = positionSTS.z - depth.x;
		    float result = convi / (convi + delta * delta);
		    return positionSTS.z >= depth.x ? 1.0 : result;
    #elif defined(_PCF)

                float weights[DIRECTIONAL_FILTER_SAMPLES];
		        float2 positions[DIRECTIONAL_FILTER_SAMPLES];
		        float4 size = _ShadowAtlasSize.yyxx;
		        DIRECTIONAL_FILTER_SETUP(size, positionSTS.xy, weights, positions);
		        float shadow = 0;
		        for (int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++) {
			        shadow += weights[i] * SamplerDirectionalShadowAtlas(
				        float3(positions[i].xy, positionSTS.z)
			        );
		        }
		        return shadow;
    #elif defined(_PCSS)
				float shadow = PCSS_Shadow_Calculate(positionSTS, 0.0, 1.0, 1.0);
				return shadow;
    #elif defined(_CSM)
                float shadow = 0.5;
                float z = SAMPLE_TEXTURE2D(_DirectionalShadowAtlas, sampler_DirectionalShadowAtlas, positionSTS.xy).r;
                float d = positionSTS.z;
                float a = 0.0, b = 0.0;
                float ck = PI * (2 * 1 - 1);
                float4 preFourier =  SAMPLE_TEXTURE2D(_FourierBufferOneId, sampler_FourierBufferOneId, positionSTS.xy);
                a += 2 * cos(ck * d) * preFourier.x / ck * exp(1.0 / 8.0 /8.0);
                b += 2 * sin(ck * d) * preFourier.y / ck * exp(1.0 / 8.0 /8.0);
                ck = PI * (2 * 2 - 1);
                a += 2 * cos(ck * d) * preFourier.z / ck * exp(2.0 * 2.0 / 8.0 /8.0);
                b += 2 * sin(ck * d) * preFourier.w / ck * exp(2.0 * 2.0 / 8.0 /8.0);
    
                preFourier = SAMPLE_TEXTURE2D(_FourierBufferTwoId, sampler_FourierBufferTwoId, positionSTS.xy);
                ck = PI * (2 * 3 - 1);
                a += 2 * cos(ck * d) * preFourier.x / ck * exp(3.0 * 3.0 / 8.0 /8.0);
                b += 2 * sin(ck * d) * preFourier.y / ck * exp(3.0 * 3.0 / 8.0 /8.0);
                ck = PI * (2 * 4 - 1);
                a += 2 * cos(ck * d) * preFourier.z / ck * exp(4.0 * 4.0 / 8.0 /8.0);
                b += 2 * sin(ck * d) * preFourier.w / ck * exp(4.0 * 4.0 / 8.0 /8.0);
    
                preFourier = SAMPLE_TEXTURE2D(_FourierBufferThreeId, sampler_FourierBufferThreeId, positionSTS.xy);
                ck = PI * (2 * 5 - 1);
                a += 2 * cos(ck * d) * preFourier.x / ck * exp(5.0 * 5.0 / 8.0 /8.0);
                b += 2 * sin(ck * d) * preFourier.y / ck * exp(5.0 * 5.0 / 8.0 /8.0);
                ck = PI * (2 * 6 - 1);
                a += 2 * cos(ck * d) * preFourier.z / ck * exp(6.0 * 6.0 / 8.0 /8.0);
                b += 2 * sin(ck * d) * preFourier.w / ck * exp(6.0 * 6.0 / 8.0 /8.0);
    
                preFourier = SAMPLE_TEXTURE2D(_FourierBufferFourId, sampler_FourierBufferFourId, positionSTS.xy);
                ck = PI * (2 * 7 - 1);
                a += 2 * cos(ck * d) * preFourier.x / ck * exp(7.0 * 7.0 / 8.0 /8.0);
                b += 2 * sin(ck * d) * preFourier.y / ck * exp(7.0 * 7.0 / 8.0 /8.0);
                ck = PI * (2 * 8 - 1);
                a += 2 * cos(ck * d) * preFourier.z / ck * exp(8.0 * 8.0 / 8.0 /8.0);
                b += 2 * sin(ck * d) * preFourier.w / ck * exp(8.0 * 8.0 / 8.0 /8.0);
                
                shadow += (a - b);
                return (d - z) < 0.03 ? 0 : saturate(2 * shadow);
    #else
                 return SamplerDirectionalShadowAtlas(positionSTS);
    #endif
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