#ifndef TAA_INCLUDE
#define TAA_INCLUDE

#include "../Common.hlsl"					
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_PreColorBuffer);
SAMPLER(sampler_PreColorBuffer);
TEXTURE2D(_CurrentColorBuffer);
SAMPLER(sampler_CurrentColorBuffer);
TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);

float4x4 _PreV;
float4x4 _PreP;
float2 _Jitter;

#ifndef STEP_COUNT
#define STEP_COUNT 32
#endif

#ifndef TIME_RATIO
#define TIME_RATIO 0.5
#endif

float3 clip_aabb(float3 aabb_min, float3 aabb_max, float3 input_texel)
{
	float3 p_clip = 0.5 * (aabb_max + aabb_min);
	float3 e_clip = 0.5 * (aabb_max - aabb_min);
	float3 v_clip = input_texel - p_clip;
	float3 v_unit = v_clip / e_clip;
	float3 a_unit = abs(v_unit);
	float ma_unit = max(a_unit.x, max(a_unit.y, a_unit.z));

	if (ma_unit > 1.0)
		return p_clip + v_clip / ma_unit;
	else
		return input_texel;
}

float4 TAA(v2f vert) : SV_TARGET
{
	float2 uv = vert.uv - _Jitter;
	float3 posWS = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, vert.uv);
	float4 prePosHCS = mul(_PreP,mul(_PreV, float4(posWS, 1.0)));
	float2 texelSizeU = float2(1.0 / 2560.0, 0.0);
	float2 texelSizeV = float2(0.0, 1.0 / 1440.0);
	prePosHCS /= prePosHCS.w;
	float2 preUV = prePosHCS.xy * 0.5 + 0.5;
	float3 color00 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv - texelSizeU - texelSizeV).rgb;
	float3 color01 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv - texelSizeV).rgb;
	float3 color02 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv + texelSizeU - texelSizeV).rgb;
	float3 color10 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv - texelSizeU).rgb;
	float3 color11 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv).rgb;
	float3 color12 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv + texelSizeU).rgb;
	float3 color20 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv - texelSizeU + texelSizeV).rgb;
	float3 color21 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv + texelSizeV).rgb;
	float3 color22 = SAMPLE_TEXTURE2D(_CurrentColorBuffer, sampler_CurrentColorBuffer, uv - texelSizeU + texelSizeV).rgb;

	float3 colorMin = min(color00, min(color01, min(color02, min(color10, min(color11, min(color12, min(color20, min(color21, color22))))))));
	float3 colorMax = max(color00, max(color01, max(color02, max(color10, max(color11, max(color12, max(color20, max(color21, color22))))))));

	float3 preColor = SAMPLE_TEXTURE2D(_PreColorBuffer, sampler_PreColorBuffer, preUV).rgb;
	preColor = clip_aabb(colorMin, colorMax, preColor);
	float3 resultColor = lerp(color11, preColor, TIME_RATIO);
	return float4(resultColor, 1.0);
}

#endif //TAA_INCLUDE