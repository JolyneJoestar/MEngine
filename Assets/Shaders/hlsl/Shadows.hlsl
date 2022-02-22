#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED
	
#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
int _CascadeCount;
float4 _CascadeCullingSphere[MAX_CASCADE_COUNT];
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
float4 _CascadeData[MAX_CASCADE_COUNT];
float4 _ShadowDistanceFade;
CBUFFER_END

struct DirectionalShadowData {
	float strength;
	int tileIndex;
    float normalBias;
};

struct ShadowData
{
    int cascadeIndex;
    float strength;
};

float SamplerDirectionalShadowAtlas(float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(DirectionalShadowData dirData,ShadowData shadowData, Surface surfaceWS)
{
    if (dirData.strength <= 0.0)
		return 1.0;
    float3 normalBias = surfaceWS.normal * _CascadeData[shadowData.cascadeIndex].y * dirData.normalBias;
    float3 positionSTS = mul(_DirectionalShadowMatrices[dirData.tileIndex], float4(surfaceWS.position + normalBias, 1.0)).xyz;
	float shadow = SamplerDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, dirData.strength);
}

#endif //CUSTOM_SHADOWS_INCLUDED