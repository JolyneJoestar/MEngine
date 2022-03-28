#ifndef MY_LEGACY_BRDF_INCLUDE
#define MY_LEGACY_BRDF_INCLUDE

#define MIN_REFLECTIVITY 0.04

#include "GI.hlsl"
#include "PreProccessShader/PreProccessHelper.hlsl"

TEXTURE2D(_IrradianceMap)
SAMPLER(sampler_IrradianceMap)


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
    float3 albedo = surface.color;
    float3 F0 = float3(0.04);
    F0 = mix(F0, albedo, surface.metallic);

    float3 H = normalize(surface.viewDirection + light.direction);
    float distance = 1.0;
    float attenuation = 1.0;
    float3 radiance;

    float NDF = DistributionGGX(surface.normal, H, brdf.roughness);
    float G = GeometrySmith(surface.normal, surface.viewDirection, light.direction, brdf.roughness);
    float3 F = fresnelSchlick(max(dot(H, surface.viewDirection), 0.0), F0);

    float3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(surface.normal, surface.viewDirection), 0.0) * max(dot(surface.normal, light.direction), 0.0) + 0.0001;
    float3 specular = numerator / denominator;

    float3 kS = F;
    float kD = float3(1.0) - kS;
    kD *= 1.0 - surface.metallic;

    float NdotL = max(dot(surface.normal, light.direction), 0.0);
	return (kD * albedo / PI + specular) * radiance + NdotL;
}

float3 GetLighting(Surface surface,BRDF brdf, GI gi)
{
    ShadowData shadowData = GetShadowData(surface);
    float3 Lo = float3(0.0);
    for (int i = 0; i < GetDirectionLightCount(); i++)
    {
		Lo += GetLighting(surface, brdf, GetDirectionLight(i, surface, shadowData));
    }
	float3 kS = fresnelSchlick(max(dot(surface.normal, light.direction), 0.0), F0);
	float3 kD = 1.0 - kS;
	kD *= 1.0 - surface.metallic;
	float3 irradiance = SAMPLE_TEXTURE2D(_IrradianceMap, sampler_IrradianceMap, normal2uv(surface.normal)).rgb;
	float3 diffuse = irradiance * surface.color;
	float3 ambient = (kD * diffuse) * ao;
	float3 color = ambiant + Lo;

	color = color / (color + float3(1.0));
	color = pow(color, float3(1.0 / 2.0));

    return color;
}

#endif //MY_LEGACY_LIGHT_INCLUDE