#ifndef LIGHT_VOLUME_INCLUDE
#define LIGHT_VOLUME_INCLUDE

#include "../Common.hlsl"					
#include "../MyLegacySurface.hlsl"
#include "../Shadows.hlsl"
#include "../GI.hlsl"
#include "../MyLegacyLight.hlsl"
#include "../MyLegacyBRDF.hlsl"
#include "../LitInput.hlsl"
#include "DeferredRenderHelper.hlsl"

TEXTURE2D(_GPosition);
SAMPLER(sampler_GPosition);

float4 deferredLightVolumeFrag(v2f vert) : SV_TARGET
{
    float3 position = SAMPLE_TEXTURE2D(_GPosition, sampler_GPosition, vert.uv).rgb;
    return float4(GetLightVolume(position), 1.0);
}

#endif //LIGHT_VOLUME_INCLUDE