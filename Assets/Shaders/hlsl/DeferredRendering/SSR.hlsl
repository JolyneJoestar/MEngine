#ifndef SSR_PASS_INCLUDE
#define SSR_PASS_INCLUDE

#include "../Common.hlsl"					
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);
TEXTURE2D(_GNormal);
SAMPLER(sampler_GNormal);
TEXTURE2D(_DFColorBuffer);
SAMPLER(sampler_DFColorBuffer);
TEXTURE2D(_GMaterial);
SAMPLER(sampler_GMaterial);

#ifndef STEP_COUNT
#define STEP_COUNT 32
#endif

float4 SSRGenPass(v2f vert) :SV_TARGET
{
	float2 material = SAMPLE_TEXTURE2D(_GMaterial, sampler_GMaterial, vert.uv).xy;
	float2 uvPos = vert.uv;
	float3 color = SAMPLE_TEXTURE2D(_DFColorBuffer, sampler_DFColorBuffer, uvPos).rgb;
	//color = pow(color, 1.0 / 2.2);
	if(material.x > 0.1 || material.y < 0.9)
		return	float4(color, 1.0);
	float2 step = 1.0 / float2(1024.0, 720.0);	
	float3 pos = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, vert.uv).xyz;
	float3 normal = SAMPLE_TEXTURE2D(_GNormal, sampler_GNormal, vert.uv).xyz;
	float3 viewDir = normalize(pos - _WorldSpaceCameraPos);
	float3 viewOut = normalize(reflect(viewDir, normal));
	float4 viewOutHCS = TransformWorldToHClip(viewOut);

	float3 stepPosScr = pos;
	float stepRatio = 0.2;
	for (int i = 0; i < STEP_COUNT; i++)
	{
		float3 stepPos = stepPosScr + viewOut * stepRatio;
		float srcDepth = TransformWorldToView(stepPos).z;
		float4 stepPosHCS = TransformWorldToHClip(stepPos);
		stepPosHCS.xyz /= stepPosHCS.w;
		stepPosHCS.y = -stepPosHCS.y;
		float2 stepUV = stepPosHCS.xy * 0.5 + 0.5;
		
		if (stepUV.x > 1.0 || stepUV.x < 0.0)
		{
			stepRatio /= 2.0;
			continue;
		}
		if (stepUV.y > 1.0 || stepUV.y < 0.0)
		{
			stepRatio /= 2.0;
			continue;
		}
		float sampleDepth = TransformWorldToView(SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, stepUV).xyz).z;
		
#if defined(UNITY_REVERSED_Z)
		if (srcDepth < sampleDepth)			
#else
		if (srcDepth > sampleDepth)
#endif
		{
			if(abs(srcDepth - sampleDepth) < 0.05)
				return float4(SAMPLE_TEXTURE2D(_DFColorBuffer, sampler_DFColorBuffer, stepUV).rgb, 1.0);
			stepRatio /= 4.0;
		}
		else
		{
			if(abs(srcDepth - sampleDepth) < 0.05)
				return float4(SAMPLE_TEXTURE2D(_DFColorBuffer, sampler_DFColorBuffer, stepUV).rgb, 1.0);
			if (stepRatio < 1.0)
				stepRatio *= 2.0;
			stepPosScr = stepPos;			
		}
	}
	return	float4(color, 1.0);
}

#endif //SSR_PASS_INCLUDE