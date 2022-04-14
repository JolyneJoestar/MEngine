#ifndef SSR_PASS_INCLUDE
#define SSR_PASS_INCLUDE

#include "../Common.hlsl"					
#include "../MyLegacySurface.hlsl"
#include "../Shadows.hlsl"
#include "../GI.hlsl"
#include "../MyLegacyLight.hlsl"
#include "../MyLegacyBRDF.hlsl"
#include "../LitInput.hlsl"
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);
TEXTURE2D(_GNormal);
SAMPLER(sampler_GNormal);
#ifndef STEP_COUNT
#define STEP_COUNT 32
#endif

float4 SSRGenPass(v2f vert) :SV_TARGET
{
	float2 step = 1.0 / float2(1024.0, 720.0);
	float2 uvPos = vert.uv;
	float3 pos = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, vert.uv).xyz;
	float3 normal = SAMPLE_TEXTURE2D(_GNormal, sampler_GNormal, vert.uv).xyz;
	float3 viewDir = normalize(pos - _WorldSpaceCameraPos);
	float3 viewOut = reflect(viewDir, normal);
	float3 viewOutHCS = TransformWorldToHClipDir(viewOut, true);
	viewOut /= float2(step.x / viewOutHCS.x, step.y / viewOutHCS.y);

	for (int i = 0; i < STEP_COUNT; i++)
	{
		uvPos += actStep;
		if()
	}
}

#endif //SSR_PASS_INCLUDE