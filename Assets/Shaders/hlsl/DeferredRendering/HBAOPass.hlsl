#ifndef SSAO_PASS_INCLUDE
#define SSAO_PASS_INCLUDE

#include "../Common.hlsl"					


TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);
TEXTURE2D(_GNormal);
SAMPLER(sampler_GNormal);
TEXTURE2D(_Noise);
SAMPLER(sampler_Noise);

float4 samples[64];

float noiseScale;
#define kernelSize  64
#define radius  0.5
#define bias  0.025
float4 HBAOFragment(v2f vert): SV_TARGET
{

}

#endif //SSAO_PASS_INCLUDE