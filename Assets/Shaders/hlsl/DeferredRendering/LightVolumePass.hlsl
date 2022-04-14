#ifndef LIGHT_VOLUME_INCLUDE
#define LIGHT_VOLUME_INCLUDE

#include "../Common.hlsl"					
#include "../MyLegacySurface.hlsl"
#include "../Shadows.hlsl"
#include "../GI.hlsl"
#include "../MyLegacyLight.hlsl"
#include "../MyLegacyBRDF.hlsl"
#include "../LitInput.hlsl"

TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);

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

float4 deferredLightVolumeFrag(v2f vert) : SV_TARGET
{
    float3 position = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, vert.uv).rgb;
    return float4(GetLightVolume(position), 1.0);
}

#endif //LIGHT_VOLUME_INCLUDE