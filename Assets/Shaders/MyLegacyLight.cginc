#ifndef MY_LEGACY_LIGHT_INCLUDE
#define MY_LEGACY_LIGHT_INCLUDE

#define MAX_VISIBLE_LIGHTS 4

CBUFFER_START(MLightBuffer)
	int MVisibleLightCount;
	float4 MVisibleLightColors[MAX_VISIBLE_LIGHTS];
	float4 MVisibleLightDirecitons[MAX_VISIBLE_LIGHTS];
CBUFFER_END

struct Light
{
    float3 direction;
    float3 color;
};

int GetDirectionLightCount()
{
    return MVisibleLightCount;
}

Light GetDirectionLight(int index)
{
    Light light;
    light.color = MVisibleLightColors[index];
    light.direction = MVisibleLightDirecitons[index];
    return light;
}

float3 IncomingLight(Surface surface,Light light)
{
    return saturate(dot(surface.normal,light.direction)) * light.color;
}

float3 GetLighting(Surface surface,Light light)
{
    return IncomingLight(surface,light)*surface.color;
}

float3 GetLighting(Surface surface)
{
    float3 color = 0.0;
	for (int i = 0; i < GetDirectionLightCount(); i++)
	{
        color += GetLighting(surface, GetDirectionLight(i));
    }
    return color;
}
#endif //MY_LEGACY_LIGHT_INCLUDE