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
TEXTURE2D(_GColor);
SAMPLER(sampler_GColor);


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

float SSRFragment(v2f vert): SV_TARGET
{
	float3 pos = SAMPLE(_GPosition, sampler_GPosition, vert.uv).xyz;
	float3 normal = normalize(SAMPLE(_GNormal, sampler_GNormal, vert.uv).xyz);
	float3 ref = ref(viewDir, normal);
	float2 uv = vert.uv;
	float3 a = MVP * ref;
	float texel = 1.0 / textureSize;
	float2 tp = float2(texel, texel) / a.xy;
	float2 next = tp;
	for (int i = 0; i < 64; i++)
	{
		float3 c_pos = pos + ref * next;
		float3 spos = SAMPLE(_GPosition, sampler_GPosition, c_pos.xy).xyz;
		if (c_pos.z < spos.z)
		{
			delt += 2 * z;
			next += delt;
		}
		else if(c_pos.z > spos.z)
		{
			if (delt == z)
				return SAMPLE(_GColor, sampler_GColor, c_pos.xy).rgb;
			delt = delta / 2.0;
			next -= delt;
		}
		else
			return SAMPLE(_GColor, sampler_GColor, c_pos.xy).rgb;
	}

}

#endif //SSAO_PASS_INCLUDE