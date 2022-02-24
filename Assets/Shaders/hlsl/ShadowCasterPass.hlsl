#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#include "Common.hlsl"	

TEXTURE2D(MBaseMap);
SAMPLER(samplerMBaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
UNITY_DEFINE_INSTANCED_PROP(float4, MBaseMap_ST)
UNITY_DEFINE_INSTANCED_PROP(float4, MBaseColor)
UNITY_DEFINE_INSTANCED_PROP(float, MCutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes {
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ShadowCasterPassVertex(Attributes input) {
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	float3 positionWS = TransformObjectToWorld(input.positionOS);
	output.positionCS = TransformWorldToHClip(positionWS);

#if UNITY_REVERSED_Z
	output.positionCS.z =
		min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    output.positionCS.z =
			max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

	float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, MBaseMap_ST);
	output.baseUV = input.baseUV * baseST.xy + baseST.zw;
	return output;
}

float4 ShadowCasterPassFragment(Varyings input) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap = float4(0.0, 0.0, 0.0, 0.0);
#if defined(_VSM)
    baseMap.r = input.positionCS.z;
    baseMap.g = input.positionCS.z * input.positionCS.z;
#else
    baseMap.r = input.positionCS.z;
    baseMap.g = input.positionCS.z * input.positionCS.z;
#endif
    return baseMap;
#if defined(_CLIPPING)
	clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, MCutoff));
#endif
}

#endif