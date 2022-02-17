#ifndef MYRP_LEGACY_INCLUDE
#define MYRP_LEGACY_INCLUDE

struct VertexInput
{
	float4 pos : POSITION;
};
struct VectexOutput
{
	float4 clipPos : SV_POSITION;
};

VertexOutput LegacyVertex(VertexInput input)
{
	VertexOutput output;
	output.clipPos = input.pos;
	return output;
}

float4 LegacyFragment(VertexOutput input) : SV_TARGET
{
	return input.clipPos;
}



















#endif //MYRP_LEGACY_INCLUDE