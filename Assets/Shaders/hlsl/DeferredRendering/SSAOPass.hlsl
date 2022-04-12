#ifndef SSAO_PASS_INCLUDE
#define SSAO_PASS_INCLUDE

#include "../Common.hlsl"					
#include "../MyLegacySurface.hlsl"
#include "../Shadows.hlsl"
#include "../GI.hlsl"
#include "../MyLegacyLight.hlsl"
#include "../MyLegacyBRDF.hlsl"
#include "../LitInput.hlsl"


TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);
TEXTURE2D(_GNormal);
SAMPLER(sampler_GNormal);
TEXTURE2D(_Noise);
SAMPLER(sampler_Noise);


struct v2f
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
};

v2f vert(uint vertexID : SV_VertexID)
{
	v2f o;
	o.vertex = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? -3.0 : 1.0,
		0.0, 1.0
		);
	o.uv = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
		);
	return o;
}
float4 samples[64];

float noiseScale;
#define kernelSize  64
#define radius  0.5
#define bias  0.025
float4 SSAOFragment(v2f vert): SV_TARGET
{
    float3 worldPos = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, vert.uv).xyz;
    float3 viewPos = TransformWorldToView(worldPos);

    float3 normal = TransformWorldToViewDir(SAMPLE_TEXTURE2D(_GNormal, sampler_GNormal, vert.uv).xyz, true);

    float3 randomVec = normalize(float3(SAMPLE_TEXTURE2D(_Noise, sampler_Noise, vert.uv * 10.0 ).x, SAMPLE_TEXTURE2D(_Noise, sampler_Noise, vert.uv * 20.0).y, 0.0));
	float3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
	float3 bitangent = cross(normal, tangent);
	float3x3 TBN = float3x3(tangent, bitangent, normal);
	float occlusion = 0.0;
	float kxk = 0.0;
	for (int i = 0; i < kernelSize; i++)
	{
		float3 samples_i = samples[i].xyz;
//#if defined(UNITY_REVERSED_Z)
//		samples_i.z = -samples_i.z;
//#endif
        float3 samplePos = mul(TBN , samples_i);

        samplePos = viewPos.xyz + samplePos * radius;

        float4 offset = float4(samplePos, 1.0);
        offset = TransformWViewToHClip(offset);
		offset.xyz /= offset.w;
        offset.y = -offset.y;
        offset.xyz = offset.xyz * 0.5 + 0.5;
		//if (samplePos.z > viewPos.z)
		//	kxk += 1.0;
        float3 sampleWPos = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, offset.xy).xyz;
		//return abs(offset.y - vert.uv.y);
        float3 sampleVpos = TransformWorldToView(sampleWPos);
//        return radius / abs(viewPos.z - sampleVpos.z);
        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(viewPos.z - sampleVpos.z));
        kxk += rangeCheck;
#if defined(UNITY_REVERSED_Z)
        occlusion += (sampleVpos.z >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
#else
        occlusion += (sampleVpos.z >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
		//if (samplePos.z > sampleVpos.z)
		//	kxk += 1.0;
#endif
    }

	occlusion = 1.0 - (occlusion / kernelSize);
    return kxk / kernelSize;
	return occlusion;
}

#endif //SSAO_PASS_INCLUDE