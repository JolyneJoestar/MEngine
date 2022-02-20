#ifndef UNITY_INPUT_INCLUDED
#define UNITY_INPUT_INCLUDED


float3 _WorldSpaceCameraPos;

float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
real4 unity_WorldTransformParams;

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;


#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP

#define UNITY_MATRIX_P glstate_matrix_projection

#endif //UNITY_INPUT_INCLUDED