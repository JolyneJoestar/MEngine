#ifndef CUSTOM_GEN_IRRADIANCE2_INCLUDED
#define CUSTOM_GEN_IRRADIANCE2_INCLUDED

#include "PreProccessHelper.hlsl"

float4 _RandomVector;

TEXTURECUBE(_CubeTex);
SAMPLER(sampler_CubeTex);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

float4 frag_tex2tex(v2f i) : SV_Target
{
    float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
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
    float4 offsetCol = SAMPLE_TEXTURECUBE(_CubeTex, sampler_CubeTex, offsetN);
    col.rgb = (1 - _RandomVector.w) * col.rgb + _RandomVector.w * offsetCol.rgb;
    return col;
}

#endif //CUSTOM_GEN_IRRADIANCE_INCLUDED