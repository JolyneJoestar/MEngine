#ifndef MY_LEGACY_LIGHT_PASS_INCLUDE
#define MY_LEGACY_LIGHT_PASS_INCLUDE

#include "../Common.hlsl"					
#include "../LitInput.hlsl"


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

struct MFragOut {
	float3 position : SV_TARGET0;
	float3 normal : SV_TARGET1;
	float3 albedo : SV_TARGET2;
	float4 material: SV_TARGET3;
};

MVertexOut DeferredGeometricVertex(MVertexIn inVert)
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

MFragOut DeferredGeometricFragment(MVertexOut vert)
{
	UNITY_SETUP_INSTANCE_ID(vert);
	MFragOut fragOut;

    float4 texColor = GetBase(vert.uv);
	float3 normal = normalize(vert.normal);
	normal = normal * 0.5 + 0.5;
	fragOut.position = vert.positionWS;
	fragOut.normal = normal;
	fragOut.albedo = texColor.rgb;
	fragOut.material = float4(GetMetallic(), GetSmoothness(), vert.uv.x, vert.uv.y);

	return fragOut;
}

#endif //MY_LEGACY_LIGHT_PASS_INCLUDE