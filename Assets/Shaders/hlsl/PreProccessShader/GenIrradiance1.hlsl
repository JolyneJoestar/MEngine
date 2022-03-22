#ifndef CUSTOM_GEN_IRRADIANCE1_INCLUDED
#define CUSTOM_GEN_IRRADIANCE1_INCLUDED

#include "PreProccessHelper.hlsl"

TEXTURECUBE(_MainTex);
SAMPLER(sampler_MainTex);

float4 frag_cube2tex(v2f i) : SV_Target
{
	float4 col = SAMPLE_TEXTURECUBE(_MainTex,sampler_MainTex, uv2normal(i.uv));
	// just invert the colors
	return col;
}

#endif //CUSTOM_GEN_IRRADIANCE_INCLUDED