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
SAMPLER(sampler_GPostion);
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
		vertexID == 1 ? 0.0 : 2.0
		);
	return o;
}

float SSAOFragment(v2f vert): SV_TARGET
{
	float3 frgPos = SAMPLE(_GPosition, sampler_GPosition, vert.uv).xyz;
	float3 normal = normalize(SAMPLE(_GNormal, sampler_GNormal, vert.uv).xyz);
	float3 randomVec = normalize(SAMPLE(_Noise, sampler_Noise, vert.uv * noiseScale).xyz);
	float3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
	float3 bitangent = cross(normal, tangent);
	float3x3 TBN = float3x3(tangent, bitangent, normal);
	float occlusion = 0.0;
	for (int i = 0; i < kernelSize; i++)
	{
		float3 samplePos = TBN * samples[i];
		samplePos = fragPos + samplePos * radius;

		float4 offset = float4(samplePos, 1.0);
		offset = projection * offset;
		offset.xyz /= offset.w;
		offset.xyz = offset.xyz * 0.5 + 0.5;

		float smapleDepth = SAMPLE(_GPosition, smapler_GPosition, offset.xy).z;

		float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
		occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
	}
	occlusion = 1.0 - (occlusion / kernelSize);

	return occlusion;
}

#endif //SSAO_PASS_INCLUDE