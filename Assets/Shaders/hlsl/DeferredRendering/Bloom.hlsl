#ifndef BLOOM_PASS_INCLUDE
#define BLOOM_PASS_INCLUDE

#include "../Common.hlsl"					
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_BloomInput);
SAMPLER(sampler_BloomInput);
TEXTURE2D(_BaseColorBuffer);
SAMPLER(sampler_BaseColorBuffer);

#ifndef BLOOM_RADIUS
#define BLOOM_RADIUS 6
#endif

float4 BloomGenPass(v2f vert) :SV_TARGET
{
	float3 color = 0.0;
	float2 offset = 1.0 / float2(640.0, 360.0);
	for (int i = -BLOOM_RADIUS; i <= BLOOM_RADIUS; i++)
	{
		for (int j = -BLOOM_RADIUS; j <= BLOOM_RADIUS; j++)
		{
			color += SAMPLE_TEXTURE2D(_BloomInput, sampler_BloomInput, vert.uv + offset * float2(i, j));
		}
	}
	color /= (BLOOM_RADIUS * BLOOM_RADIUS * 4.0);
	return float4(color + SAMPLE_TEXTURE2D(_BaseColorBuffer, sampler_BaseColorBuffer, vert.uv).rgb, 1.0);
}

#endif //BLOOM_PASS_INCLUDE