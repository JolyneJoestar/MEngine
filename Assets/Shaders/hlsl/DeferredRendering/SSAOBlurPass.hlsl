#ifndef SSAO_BLUR_PASS_INCLUDE
#define SSAO_Blur_PASS_INCLUDE

#include "../Common.hlsl"					
#include "../MyLegacySurface.hlsl"
#include "../Shadows.hlsl"
#include "../GI.hlsl"
#include "../MyLegacyLight.hlsl"
#include "../MyLegacyBRDF.hlsl"
#include "../LitInput.hlsl"


TEXTURE2D(_SPosition);
SAMPLER(sampler_SPosition);
TEXTURE2D(_SNormal);
SAMPLER(sampler_SNormal);
TEXTURE2D(_SNoise);
SAMPLER(sampler_SNoise);

MFragOut DeferredGeometricFragment(MVertexOut vert)
{
	UNITY_SETUP_INSTANCE_ID(vert);
	MFragOut fragOut;

    float4 texColor = GetBase(vert.uv);
	fragOut.position = vert.positionWS;
	fragOut.normal = normalize(vert.normal);
	fragOut.albedo = texColor.rgb;
	float2 uv = GI_FRAGMENT_DATA(vert);
	fragOut.material = float4(GetMetallic(), GetSmoothness(),uv.x, uv.y);

	return fragOut;
}

#endif //SSAO_Blur_Pass_INCLUDE