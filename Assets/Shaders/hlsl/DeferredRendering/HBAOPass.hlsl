#ifndef HBAO_PASS_INCLUDE
#define HBAO_PASS_INCLUDE

#include "../Common.hlsl"	
#include "DeferredRenderHelper.hlsl"


TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);
TEXTURE2D(_GNormal);
SAMPLER(sampler_GNormal);
TEXTURE2D(_Noise);
SAMPLER(sampler_Noise);
TEXTURE2D(_CameraDepthTex);
SAMPLER(sampler_CameraDepthTex);

#ifndef DIRECTION_NUM
#define DIRECTION_NUM 6
#endif
#ifndef STEP_NUM
#define STEP_NUM 6
#endif

float4 _ZBufferParam;
float4 _UV2View;
float _Radius;
float _RadiusPixel;
float _MaxRadiusPixel;
float _AngleBias;
float2 _TexelSize;
float _AOStrength;


float noiseScale;
#define kernelSize  64
#define radius  0.5
#define bias  0.025

inline float3 FetchViewPos(float2 uv)
{
	float3 pos = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, uv).xyz;
	pos = TransformWorldToView(pos);
	return pos;
}

inline float3 FetchViewNormal(float2 uv)
{
	float3 normal = SAMPLE_TEXTURE2D(_GNormal, sampler_GNormal, uv).xyz;
	normal = TransformWorldToViewDir(normal, true);
	return normal;
}

inline float FallOff(float dist)
{
	return (1.0 - dist / _Radius);
}

inline float random(float2 uv) {
	return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
}

inline float SimpleAO(float3 pos, float3 stepPos, float3 normal, inout float top)
{
	float3 h = stepPos - pos;
	float dist = sqrt(dot(h, h));
	float sinBlock = dot(normal, h) / dist;
	float diff = max(sinBlock, top);
	return diff * saturate(FallOff(dist));
}

float4 HBAOGenFragment(v2f vert) : SV_TARGET
{
	float ao = 0;
	float3 viewPos = FetchViewPos(vert.uv);
	float3 normal = FetchViewNormal(vert.uv);
	float stepSize = min((_RadiusPixel / abs(viewPos.z)), _MaxRadiusPixel) / (STEP_NUM + 1.0);
	if (stepSize < 1)
		return float4(1, 1, 1, 1);
	float delta = 2.0 * PI / DIRECTION_NUM;
	float rnd = random(vert.uv * 10);
	float2 xy = float2(1.0, 0.0);

	for (int i = 0; i < DIRECTION_NUM; i++)
	{
		float angle = delta * (float(i) + rnd);
		float cos, sin;
		sincos(angle, sin, cos);
		float2 dir = float2(cos, sin);
		float rayPixel = 1;
		float top = _AngleBias;
		for (int j = 0; j < STEP_NUM; j++)
		{
			float2 stepUV = round(rayPixel * dir) * _TexelSize.xy + vert.uv;
			float3 stepViewPos = FetchViewPos(stepUV);
			ao += SimpleAO(viewPos, stepViewPos, normal, top);
			rayPixel += stepSize;
		}
	}
	ao /= STEP_NUM * DIRECTION_NUM;

	ao = PositivePow(ao * _AOStrength, 0.6);
	float col = saturate(1 - ao);
	return col;
}

#endif //HBAO_PASS_INCLUDE