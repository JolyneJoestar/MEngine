#ifndef BLUR_PASS_INCLUDED
#define BLUR_PASS_INCLUDED

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 screenUV : VAR_SCREEN_UV;
};

int _TexSize;


TEXTURE2D(_PostFXSource);
SAMPLER(sampler_linear_clamp);

float GetSource(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_PostFXSource, sampler_linear_clamp, screenUV).r;
}

Varyings DefaultPassVertex(uint vertexID : SV_VertexID)
{
    Varyings output;
    output.positionCS = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? 3.0 : -1.0,
		0.0, 1.0
	);
    output.screenUV = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);
    return output;
}
void GetUV(float2 uv,float size, out float2 outUV[9])
{
    outUV[0] = uv;
    outUV[1] = uv + float2(1.0 / size, 1.0 / size);
    outUV[2] = uv + float2(1.0 / size, -1.0 / size);
    outUV[3] = uv + float2(-1.0 / size, 1.0 / size);
    outUV[4] = uv + float2(-1.0 / size, -1.0 / size);
    outUV[5] = uv + float2(0.0, 1.0 / size);
    outUV[6] = uv + float2(0.0, -1.0 / size);
    outUV[7] = uv + float2(1.0 / size, 0.0);
    outUV[8] = uv + float2(-1.0 / size, 0.0);
}

float4 CopyPassFragment(Varyings input) : SV_TARGET
{
    float depth = 0.0;
    float conv = 0.0;
    float2 uv[9];
    GetUV(input.screenUV, 1024.0, uv);
    for (int i = 0; i < 9; i++)
    {
        depth += GetSource(uv[i]);
    }
    depth /= 9.0;
    for (int j = 0; j < 9; j++)
    {
        float a = GetSource(uv[j]) - depth;
        conv += a * a;
    }
    conv /= 9.0;
    return float4(depth, conv, 0.0, 0.0);
}
#endif //BLUR_PASS_INCLUDED