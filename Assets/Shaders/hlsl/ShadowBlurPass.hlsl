#ifndef CUSTOM_SHADOW_BLUR_INCLUDED
#define CUSTOM_SHADOW_BLUR_INCLUDED

#include "Common.hlsl"	
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

TEXTURE2D_SHADOW(_ShadowVSM);
SAMPLER(sampler_ShadowVSM);

struct Attributes
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


Varyings ShadowBlurPassVertex(Attributes input)
{
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

    return output;
}

float4 ShadowBlurPassFragment(Varyings input) : SV_TARGET
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
#endif //CUSTOM_SHADOW_BLUR_INCLUDED