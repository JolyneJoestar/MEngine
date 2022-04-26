#ifndef LIGHTING_INCLUDE
#define LIGHTING_INCLUDE

#include "GI.hlsl"
#include "MyLegacyBRDF.hlsl"
#include "NPR.hlsl"


float3 GetLighting(Surface surface,BRDF brdf, GI gi)
{
    ShadowData shadowData = GetShadowData(surface);
    float3 color = gi.diffuse * brdf.diffuse;
    for (int i = 0; i < GetDirectionLightCount(); i++)
    {
#ifdef _NPRLIGHTING
        color += GetNPRLighting(surface, brdf, GetDirectionLight(i, surface, shadowData)) + CalculateLightVolume(i, surface.position, shadowData);
#else
		color += GetLighting(surface, brdf, GetDirectionLight(i, surface, shadowData)) + CalculateLightVolume(i, surface.position, shadowData);
#endif
    }
    return color;
}

float3 GetLighting(Surface surface, BRDF brdf, float ao)
{
    ShadowData shadowData = GetShadowData(surface);
    float3 color = 0.25 * brdf.diffuse * ao;
    for (int i = 0; i < GetDirectionLightCount(); i++)
    {
#ifdef _NPRLIGHTING
        color += GetNPRLighting(surface, brdf, GetDirectionLight(i, surface, shadowData));
#else
		color += GetLighting(surface, brdf, GetDirectionLight(i, surface, shadowData));
#endif
    }
    return color;
}
#endif //LIGHTING_INCLUDE