#ifndef MY_LEGACY_SURFACE_INCLUDE
#define MY_LEGACY_SURFACE_INCLUDE

struct Surface
{
    float3 position;
    float3 normal;
    float3 viewDirection;
    float3 color;
    float alpha;
    float metallic;
    float smoothness;
    float depth;
};
#endif //MY_LEGACY_SURFACE_INCLUDE