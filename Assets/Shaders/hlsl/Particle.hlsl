#ifndef PARTICLE_INCLUDE
#define PARTICLE_INCLUDE

struct Particle
{
    bool alive;
    float3 position;
    float3 velocity;
    float2 life; //(age, lifetime)
    float4 color;
    float size;
};
#endif //PARTICLE_INCLUDE