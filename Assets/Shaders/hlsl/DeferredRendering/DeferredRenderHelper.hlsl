#ifndef DEFERRED_RENDER_HELPER_INCLUDE
#define DEFERRED_RENDER_HELPER_INCLUDE

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


#endif //DEFERRED_RENDER_HELPER_INCLUDE