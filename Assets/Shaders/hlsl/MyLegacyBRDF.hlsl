#ifndef MY_LEGACY_BRDF_INCLUDE
#define MY_LEGACY_BRDF_INCLUDE

#define MIN_REFLECTIVITY 0.04

#include "GI.hlsl"

#ifndef SAMPLE_COUNT
#define SAMPLE_COUNT 64
#endif

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
};

float Square(float v)
{
    return v * v;
}

float OneMinusReflectivity(float metallic)
{
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    float nh2 = Square(saturate(dot(surface.normal, h)));
    float lh2 = Square(saturate(dot(light.direction, h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1, lh2) * normalization);
}

float3 CalculateLightVolume(int index, float3 posWS, ShadowData shadowData)
{
    float3 viewDir = _WorldSpaceCameraPos - posWS;
    float step = length(viewDir) / SAMPLE_COUNT;
    viewDir = normalize(viewDir);
    float3 color = 0.0;
    SimpleLight slight;
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        float3 tempPos = posWS + i * step * viewDir;
        slight = GetSimpleLight(index, tempPos, shadowData);
        color += slight.attenuation * slight.color;
    }
    color /= (SAMPLE_COUNT * 2.0);
    return color;

}
//float PerceptualSmoothnessToPerceptualRoughness(float perceptualSmoothness)
//{
//    return 1 - perceptualSmoothness;
//}

//float PerceptualRoughnessToRoughness(float perceptualRoughness)
//{
//    return perceptualRoughness * perceptualRoughness;
//}

BRDF GetBRDF(Surface surface)
{
    BRDF brdf;
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.diffuse = surface.color * OneMinusReflectivity(surface.metallic);
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return brdf;
}

float3 DirectBRDF(Surface surface,BRDF brdf,Light light)
{
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

float3 GetLighting(Surface surface,BRDF brdf, Light light)
{
    return IncomingLight(surface, light) * DirectBRDF(surface,brdf,light);
}

float3 GetLighting(Surface surface,BRDF brdf, GI gi)
{
    ShadowData shadowData = GetShadowData(surface);
    float3 color = 0.5 * brdf.diffuse;
    for (int i = 0; i < GetDirectionLightCount(); i++)
    {
        color += GetLighting(surface, brdf, GetDirectionLight(i, surface, shadowData)) + CalculateLightVolume(i, surface.position, shadowData);
    }
    return color;
}

float3 GetLightVolume(float3 posWS)
{
    ShadowData shadowData = GetShadowData(posWS);
    float3 color =  0.0;
    for (int i = 0; i < GetDirectionLightCount(); i++)
    {
        color += CalculateLightVolume(i, posWS, shadowData);
    }
    return color;
}

float3 GetLighting(Surface surface, BRDF brdf, GI gi, float ao)
{
    ShadowData shadowData = GetShadowData(surface);
    float3 color = 0.5 * brdf.diffuse;// * ao;
    for (int i = 0; i < GetDirectionLightCount(); i++)
    {
        color += GetLighting(surface, brdf, GetDirectionLight(i, surface, shadowData)); // + CalculateLightVolume(i, surface.position, shadowData);
    }
    return color;
}
#endif //MY_LEGACY_LIGHT_INCLUDE