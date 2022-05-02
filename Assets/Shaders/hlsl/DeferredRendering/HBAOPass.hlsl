#ifndef HBAO_PASS_INCLUDE
#define HBAO_PASS_INCLUDE

#include "../Common.hlsl"					


TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);
TEXTURE2D(_GNormal);
SAMPLER(sampler_GNormal);
TEXTURE2D(_Noise);
SAMPLER(sampler_Noise);
TEXTURE2D(_CameraDepthTex);
SAMPLER(sampler_CameraDepthTex);

float4 samples[64];

float noiseScale;
#define kernelSize  64
#define radius  0.5
#define bias  0.025

float PositivePow(float base, float power)
{
	return pow(max(abs(base), FLT), power);
}

inline float FetchDepth(float2 uv)
{
	return SAMPLE_TEXTURE2D(_CameraDepthTex, sampler_CameraDepthTex, uv).r;
}

inline float3 FetchViewPos(float2 uv)
{
	float depth = LinearEyeDepth(FetchDepth(uv));
	return float3((uv * _UV2View.xy + _UV2View.zw) * depth, depth);
}

inline float3 FetchViewNormal(float2 uv)
{
	float3 normal = DecodeViewNormalStereo()
}

inline float FallOff(float dist)
{
	return 1 - dist / _Radius;
}

inline float SimpleAO(float3 pos, float3 stepPos, float3 normal, inout float top)
{
	float3 h = stepPos - pos;
	float dist = sqrt(dot(h, h));
	float sinBlock = dot(normal, h) / dist;
	float diff - max(sinBlock, top);
	return diff * saturate(FallOff(dist));
}

float4 HBAOFragment(v2f vert) : SV_TARGET
{
	float ao = 0;
	float3 viewPos = FetchViewPos(vert.uv);
	float3 normal = FetchNormal(vert.uv);
	float stepSize = min((_RadiusPixel / viewPos.z), _MaxRadiusPixel) / (STEPS + 1.0);
	if (stepSize < 1)
		return float4(1, 1, 1, 1);
	float delta = 2.0 * PI / _DirectionNum;
	float rnd = random(input.uv * 10);
	float2 xy = float(1, 0);

	UNITY_UNROLL
	for (int i = 0; i < _DirectionNum; i++)
	{
		float angle = delta * (float(i) + rnd);
		float cos, sin;
		sincos(angle, sin, cos);
		float2 dir = float2(cos, sin);
		float rayPixel = 1;
		float top = _AngleBias;
		UNITY_UNROLL
		for (int j = 0; j < _StepNum; j++)
		{
			float2 stepUV = roumd(rayPixel * dir) * _TexelSize.xy + input.uv;
			float3 stepViewPos = FetchViewPos(stepUV);
			ao += SimpleAO(viewPos, stepViewPos, normal, top);
			rayPixel += stepSize;
		}
	}
	ao /= _StepNum * _DirectionNum;
	ao = PositivePow(ao * _AOStrength, 0.6);
	float col = saturate(1 - ao);
	return col;
}

#endif //HBAO_PASS_INCLUDE