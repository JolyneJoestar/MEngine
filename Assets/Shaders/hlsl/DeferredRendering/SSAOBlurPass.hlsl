#ifndef SSAO_BLUR_PASS_INCLUDE
#define SSAO_Blur_PASS_INCLUDE

#include "../Common.hlsl"					
#include "../MyLegacySurface.hlsl"
#include "../Shadows.hlsl"
#include "../GI.hlsl"
#include "../MyLegacyLight.hlsl"
#include "../MyLegacyBRDF.hlsl"
#include "../LitInput.hlsl"


TEXTURE2D(_SSAOInput);
SAMPLER(sampler_SSAOInput);

float SSAOBlurFragment(v2f vert) : SV_TARGET
{
	float2 texelSize = 1.0 / float2();
	float result = 0.0;
	for (int i = -2; i < 2; i++)
	{
		for (int j = -2; j < 2; j++)
		{
			float2 offset = float2(float(i), float(j)) * texelSize;
			result += SAMPLE(_SSAOInput, sampler_SSAOInput, vert.uv + offset).r;
		}
	}
	return result / (4.0 * 4.0);
}

#endif //SSAO_Blur_Pass_INCLUDE