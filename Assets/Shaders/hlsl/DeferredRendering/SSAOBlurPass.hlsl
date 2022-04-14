#ifndef SSAO_BLUR_PASS_INCLUDE
#define SSAO_Blur_PASS_INCLUDE

#include "../Common.hlsl"					
#include "../MyLegacySurface.hlsl"
#include "../Shadows.hlsl"
#include "../GI.hlsl"
#include "../MyLegacyLight.hlsl"
#include "../MyLegacyBRDF.hlsl"
#include "../LitInput.hlsl"
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_AoTexture);
SAMPLER(sampler_AoTexture);

float SSAOBlurFragment(v2f vert) : SV_TARGET
{
	float2 texelSize = 1.0 / float2(1024.0, 720.0);
	float result = 0.0;
	int radius = 4;
	for (int i = -radius; i < radius; i++)
	{
		for (int j = -radius; j < radius; j++)
		{
			float2 offset = float2(float(i), float(j)) * texelSize;
			result += SAMPLE_TEXTURE2D(_AoTexture, sampler_AoTexture, vert.uv + offset).r;
		}
	}
	return result / float(radius * radius * 4);
}

#endif //SSAO_Blur_Pass_INCLUDE