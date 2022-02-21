#ifndef MY_LEGACY_LIGHT_PASS_INCLUDE
#define MY_LEGACY_LIGHT_PASS_INCLUDE

#include "Common.hlsl"					
#include "MyLegacySurface.hlsl"
#include "Shadows.hlsl"
#include "MyLegacyLight.hlsl"
#include "MyLegacyBRDF.hlsl"


struct MVertexIn {
	float3 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct MVertexOut {
	float4 positionCS : SV_POSITION;
	float3 positionWS : VAR_POSITION;
	float3 normal : VAR_NORMAL;
	float2 uv : VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

TEXTURE2D(MTexture);
SAMPLER(samplerMTexture);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
UNITY_DEFINE_INSTANCED_PROP(float4, MTexture_ST)
UNITY_DEFINE_INSTANCED_PROP(float4, MTint)
UNITY_DEFINE_INSTANCED_PROP(float, MMetallic)
UNITY_DEFINE_INSTANCED_PROP(float, MSmoothness)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)


float3 CalculateDiffuseFactor(int index, float3 normal)
{
	float3 diffuseLight = MVisibleLightColors[index].rgb;
	float3 direction = MVisibleLightDirecitons[index].xyz;
	return saturate(dot(normalize(direction), normalize(normal))) * diffuseLight;
}


MVertexOut LegacyVertex(MVertexIn inVert)
{

	MVertexOut vert;
	UNITY_SETUP_INSTANCE_ID(inVert);
	UNITY_TRANSFER_INSTANCE_ID(inVert, vert);

	vert.positionWS = TransformObjectToWorld(inVert.positionOS);
	vert.positionCS = TransformWorldToHClip(vert.positionWS);

	float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, MTexture_ST);
	vert.uv = inVert.uv * baseST.xy + baseST.zw;
	vert.normal = TransformObjectToWorldNormal(inVert.normalOS);
	return vert;
}

float4 LegacyFragment(MVertexOut vert) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(vert);
	float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, MTint);
	float4 texColor = SAMPLE_TEXTURE2D(MTexture, samplerMTexture, vert.uv) * baseColor;
	Surface surface;
	surface.position = vert.positionWS;
	surface.normal = normalize(vert.normal);
	surface.viewDirection = normalize(_WorldSpaceCameraPos - vert.positionWS);
	surface.color = texColor.rgb;
	surface.alpha = texColor.a;
	surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,MMetallic);
	surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, MSmoothness);

	BRDF brdf = GetBRDF(surface);
//    return texColor;
    return float4(GetLighting(surface, brdf), surface.alpha);
}

#endif //MY_LEGACY_LIGHT_PASS_INCLUDE