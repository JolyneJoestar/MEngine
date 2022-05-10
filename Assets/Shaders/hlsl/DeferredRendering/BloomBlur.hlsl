#ifndef BLOOM_BLUR_PASS_INCLUDE
#define BLOOM_BLUR_PASS_INCLUDE

#include "../Common.hlsl"					
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_BloomInput);
SAMPLER(sampler_BloomInput);

#ifndef BLOOM_RADIUS
#define BLOOM_RADIUS 4
#endif

static const float _Weights[5] = { 0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162 };
bool _IsHorizon;
float2 _DownScaleTexelSize;

float4 BloomBlurPass(v2f vert) : SV_TARGET
{
	float3 color = SAMPLE_TEXTURE2D(_BloomInput, sampler_BloomInput, vert.uv).rgb * _Weights[0];
	
	if (_IsHorizon)
	{
		for (int i = 1; i < BLOOM_RADIUS + 1; i++)
		{
			color += SAMPLE_TEXTURE2D(_BloomInput, sampler_BloomInput, vert.uv + float2(0.0, _DownScaleTexelSize.y * i).rgb * _Weights[i];
			color += SAMPLE_TEXTURE2D(_BloomInput, sampler_BloomInput, vert.uv - float2(0.0, _DownScaleTexelSize.y * i).rgb * _Weights[i];
		}
	}
	else
	{
		for (int i = 1; i < BLOOM_RADIUS + 1; i++)
		{
			color += SAMPLE_TEXTURE2D(_BloomInput, sampler_BloomInput, vert.uv + float2(_DownScaleTexelSize.x * i, 0.0).rgb * _Weights[i];
			color += SAMPLE_TEXTURE2D(_BloomInput, sampler_BloomInput, vert.uv - float2(_DownScaleTexelSize.x * i, 0.0).rgb * _Weights[i];
		}
	}
	return float4(color, 1.0);
}

#endif //BLOOM_BLUR_PASS_INCLUDE