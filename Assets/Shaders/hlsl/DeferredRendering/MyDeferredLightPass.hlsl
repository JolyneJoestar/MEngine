#ifndef MY_DEFERRED_LIGHT_PASS_INCLUDE
#define MY_DEFERRED_LIGHT_PASS_INCLUDE

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
TEXTURE2D(_GAlbedo);
SAMPLER(sampler_GAlbedo);
TEXTURE2D(_GMaterial);
SAMPLER(sampler_GMaterial);
TEXTURE2D(_BluredAoTexture);
SAMPLER(sampler_BluredAoTexture);
TEXTURE2D(_LightVolume);
SAMPLER(sampler_LightVolume);

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

float4 deferredLightingFragPass(v2f vert) : SV_TARGET
{   
	Surface surface;
	surface.position = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, vert.uv).rgb;
	surface.normal = SAMPLE_TEXTURE2D(_GNormal, sampler_GNormal, vert.uv).rgb;
    surface.viewDirection = normalize(_WorldSpaceCameraPos - surface.position);
    surface.depth = -TransformWorldToView(surface.position).z;
	surface.color = SAMPLE_TEXTURE2D(_GAlbedo, sampler_GAlbedo, vert.uv).rgb;
	surface.alpha = 1.0;
	float4 material = SAMPLE_TEXTURE2D(_GMaterial, sampler_GMaterial, vert.uv).rgba;
    surface.metallic = material.x;
    surface.smoothness = material.y;
    surface.dither = 1.0;
	float2 lightuv = material.zw;//float2(material.z, material.w);
	BRDF brdf = GetBRDF(surface);

    GI gi = GetGI(lightuv, surface);
    float ao = SAMPLE_TEXTURE2D(_BluredAoTexture, sampler_BluredAoTexture, vert.uv).r;
    return float4(GetLighting(surface, brdf, gi, ao) + SAMPLE_TEXTURE2D(_LightVolume, sampler_LightVolume, vert.uv).rgb, surface.alpha);
}

#endif //MY_DEFERRED_LIGHT_PASS_INCLUDE