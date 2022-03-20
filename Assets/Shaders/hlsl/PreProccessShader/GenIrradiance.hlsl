#ifndef CUSTOM_GEN_IRRADIANCE_INCLUDED
#define CUSTOM_GEN_IRRADIANCE_INCLUDED

#include "../Common.hlsl"

TEXTURECUBE(_MainTex);
SAMPLER(sampler_ManiTex);

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

v2f vert(appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);
    o.uv = v.uv;
    return o;
}

float3 uv2normal(float2 uv)
{
    float3 result;
    uv.x = uv.x * PI * 2 - PI;
    uv.y = (1 - uv.y) * PI;
    result.y = cos(uv.y);
    result.x = sin(uv.y) * cos(uv.x);
    result.z = sin(uv.y) * sin(uv.x);
    result = normalize(result);
    return result;
}

float2 normal2uv(float3 normal)
{
    float2 result;
    result.y = 1 - acos(normal.y) / PI;
    result.x = (atan2(normal.z, normal.x)) / PI * 0.5 + 0.5;
    result.x = result.x;
    return result;
}

float4 frag(v2f i) : SV_Target
{
    float4 col = texCube(_MainTex, uv2normal(i.uv));
                // just invert the colors
    return col;
}

float4 frag_tex2tex(v2f i) : SV_Target
{
    float4 col = tex2D(_MainTex, i.uv);
    float3 n = uv2normal(i.uv);
    float3 t;
    if (n.y > 0.99)
        t = float3(1, 0, 0);
    else
        t = float3(0, 1, 0);
    float3 b = normalize(cross(t, n));
    t = normalize(cross(b, n));
    _RandomVector.xyz = normalize(_RandomVector.xyz);
    float3 offsetN = t * _RandomVector.x + b * _RandomVector.z + n * _RandomVector.y;
    offsetN = normalize(offsetN);
    float4 offsetCol = texCUBE(_CubeTex, offsetN);
    col.rgb = (1 - _RandomVector.w) * col.rgb + _RandomVector.w * offsetCol.rgb;
    return col;
}

#endif //CUSTOM_GEN_IRRADIANCE_INCLUDED