#ifndef BLOOM_INPUT_INCLUDE
#define BLOOM_INPUT_INCLUDE

#include "../Common.hlsl"					
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_BaseColorBuffer);
SAMPLER(sampler_BaseColorBuffer);

float4 BloomGetSource(v2f vert) :SV_TARGET
{
	float3 color = SAMPLE_TEXTURE2D(_BaseColorBuffer, sampler_BaseColorBuffer, vert.uv).rgb;
	if(max(max(color.r, color.g), color.b) > 1.0)
		return float4(color, 1.0);
	return float4(0.0, 0.0, 0.0, 1.0);
}

#endif //BLOOM_INPUT_INCLUDE