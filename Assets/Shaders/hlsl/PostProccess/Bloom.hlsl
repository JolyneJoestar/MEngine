#ifndef BLOOM_PASS_INCLUDE
#define BLOOM_PASS_INCLUDE

#include "../Common.hlsl"					
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_BloomInput);
SAMPLER(sampler_BloomInput);

#ifndef BLOOM_RADIUS
#define BLOOM_RADIUS 4
#endif

float4 SSRGenPass(v2f vert) :SV_TARGET
{
	float3 color = 0.0;
	for (int i = -BLOOM_RADIUS; i <= BLOOM_RADIUS; i++)
	{
		for (int j = -BLOOM_RADIUS; j <= BLOOM_RADIUS; j++)
		{
			color += SAMPLE_TEXTURE2D(_BloomInput, sampler_BloomInput, vert.uv);
		}
	}
	color /= (BLOOM_RADIUS * BLOOM_RADIUS * 4);
	return float4(color, 1.0);
}

#endif //BLOOM_PASS_INCLUDE