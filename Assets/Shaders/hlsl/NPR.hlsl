#ifndef MY_LEGACY_BRDF_INCLUDE
#define MY_LEGACY_BRDF_INCLUDE

#include "Common.hlsl"

float OutlineWidth;
float4 OutlineColor;

struct MVertexIn {
	float3 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct MOutlineVertexOut {
	float4 position : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

MOutlineVertexOut OutlineVert(MVertexIn inVert)
{
	MOutlineVertexOut outVert;
	UNITY_SETUP_INSTANCE_ID(inVert);
	UNITY_TRANSFORM_INSTANCE_ID(inVert, outVert);
	outVert.position.xyz = TransformObjectToView(inVert.positionOS);
	float3 normal = TransformObjectToViewDir(inVert.normalOS, true);
	normal.z = -0.5;
	outVert.position.xyz += normal * OutlineWidth;
	outVert.position = TransformViewToHClip(outVert.position);
}

#endif //MY_LEGACY_LIGHT_INCLUDE