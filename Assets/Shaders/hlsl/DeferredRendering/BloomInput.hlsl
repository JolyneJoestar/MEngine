#ifndef BLOOM_INPUT_INCLUDE
#define BLOOM_INPUT_INCLUDE

#include "../Common.hlsl"					
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_DFColorBuffer);
SAMPLER(sampler_DFColorBuffer);

#ifndef BLOOM_RADIUS
#define BLOOM_RADIUS 4
#endif

float4 BloomGetSource(v2f vert) :SV_TARGET
{
	float3 color = SAMPLE_TEXTURE2D(_DFColorBuffer, sampler_DFColorBuffer, vert.uv).rgb;
	if(max(max(color.r, color,g), color.b) > 0.98)
		return float4(color, 1.0);
	return float4(0.0, 0.0, 0.0, 1.0);
}

#endif //BLOOM_INPUT_INCLUDE