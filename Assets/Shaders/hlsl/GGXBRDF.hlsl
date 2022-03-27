#ifndef MY_LEGACY_BRDF_INCLUDE
#define MY_LEGACY_BRDF_INCLUDE

#define MIN_REFLECTIVITY 0.04

#include "GI.hlsl"

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

float DistributionGGX(float3 N, float3 H, float3 roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float Ndota2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;
    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}


float3 GetLighting(Surface surface,BRDF brdf, Light light)
{
    float3 N = surface.normal;
    float3 V = surface.viewDirection;
    float3 albedo = surface.color;

    float3 F0 = float3(0.04);
    F0 = mix(F0, albedo, surface.metallic);

    float3 Lo = float3(0.0);

    float3 L = normalize(light.direction);
    float3 H = normalize(V + L);
    float distance = 1.0;
    float attenuation = 1.0;
    float3 radiance;

    float NDF = DistributionGGX(N, H, brdf.roughness);
    float G = GeometrySmith(N, V, L, brdf.roughness);
    float3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

    float3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    float3 specular = numerator / denominator;

    float3 kS = F;
    float kD = float3(1.0) - kS;
    kD *= 1.0 - surface.metallic;

    float NdotL = max(dot(N, L), 0.0);
}

float3 GetLighting(Surface surface,BRDF brdf, GI gi)
{
    ShadowData shadowData = GetShadowData(surface);
    float3 color = gi.diffuse * brdf.diffuse;
    for (int i = 0; i < GetDirectionLightCount(); i++)
    {
        color += GetLighting(surface, brdf, GetDirectionLight(i, surface, shadowData));
    }
    return color;
}

#endif //MY_LEGACY_LIGHT_INCLUDE