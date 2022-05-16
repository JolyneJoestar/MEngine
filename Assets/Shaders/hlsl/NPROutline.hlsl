#ifndef NPR_OUTLINE_INCLUDE
#define NPR_OUTLINE_INCLUDE

#include "Common.hlsl"

float _OutlineWidth;
float4 _OutlineColor;

struct MVertexIn {
	float3 positionOS : POSITION;
	float3 normalOS : NORMAL;
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
	UNITY_TRANSFER_INSTANCE_ID(inVert, outVert);
	outVert.position.xyz = TransformObjectToWorld(inVert.positionOS);
	outVert.position.xyz = TransformWorldToView(outVert.position.xyz);
	float3 normal = TransformObjectToWorldNormal(inVert.normalOS);
	normal = TransformWorldToViewDir(normal);
	normal.z = -0.5;
	outVert.position.xyz += normalize(normal) * _OutlineWidth;
	outVert.position = TransformWViewToHClip(outVert.position.xyz);
	return outVert;
}

float4 OutlineFrag(MOutlineVertexOut vert) : SV_TARGET
{
	return _OutlineColor;
}

#endif //NPR_OUTLINE_INCLUDE