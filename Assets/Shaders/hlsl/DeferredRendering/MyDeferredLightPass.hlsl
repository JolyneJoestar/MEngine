#ifndef MY_LEGACY_LIGHT_PASS_INCLUDE
#define MY_LEGACY_LIGHT_PASS_INCLUDE

#include "Common.hlsl"					
#include "MyLegacySurface.hlsl"
#include "Shadows.hlsl"
#include "GI.hlsl"
#include "MyLegacyLight.hlsl"
#include "MyLegacyBRDF.hlsl"
#include "LitInput.hlsl"

TEXTURE2D(_GPosition)
SAMPLER(sampler_GPosition)
TEXTURE2D(_GNormal)
SAMPLER(sampler_GNormal)
TEXTURE2D(_GAlbedo)
SAMPLER(sampler_GAlbedo)
TEXTURE2D(_GMaterial)
SAMPLER(sampler_GMaterial)

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
};

v2f vert(appdata v)
{
	v2f o;
	o.vertex = TransformObjectToHClip(v.vertex);
	o.uv = v.uv;
	return o;
}

float4 deferredLightingFragPass(v2f vert) : SV_TARGET
{   
	Surface surface;
	surface.position = SAMPLE_TEXTURE2D(_GPosition, sampler_GPostion, vert.uv).rgb;
	surface.normal = SAMPLER_TEXTURE2D(_GNormal, sampler_GNormal, vert.uv).rgb;
	surface.viewDirection = normalize(_WorldSpaceCameraPos - vert.positionWS);
    surface.depth = -TransformWorldToView(surface.position).z;
	surface.color = SAMPLE_TEXTURE2D(_GAlbedo, sampler_GAlbedo, vert.uv).rgb;
	surface.alpha = 1.0;
	float3 material = SAMPLE_TEXTURE2D(_GMaterial, sampler_Gmaterial, vert.uv).rgb
    surface.metallic = material.x;
    surface.smoothness = material.y;
    surface.dither = material.z;

	BRDF brdf = GetBRDF(surface);
//    return texColor;
    GI gi = GetGI(GI_FRAGMENT_DATA(vert), surface);
    return float4(GetLighting(surface, brdf, gi), surface.alpha);
}

#endif //MY_LEGACY_LIGHT_PASS_INCLUDE