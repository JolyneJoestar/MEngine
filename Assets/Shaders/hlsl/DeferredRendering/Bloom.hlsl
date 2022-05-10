#ifndef BLOOM_PASS_INCLUDE
#define BLOOM_PASS_INCLUDE

#include "../Common.hlsl"					
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_BloomInput);
SAMPLER(sampler_BloomInput);
TEXTURE2D(_BaseColorBuffer);
SAMPLER(sampler_BaseColorBuffer);

float4 BloomFinal(v2f vert) :SV_TARGET
{
	float3 color = SAMPLE_TEXTURE2D(_BloomInput, sampler_BloomInput, vert.uv).rgb + SAMPLE_TEXTURE2D(_BaseColorBuffer, sampler_BaseColorBuffer, vert.uv).rgb;
	return float4(color, 1.0);
}
#endif //BLOOM_PASS_INCLUDE