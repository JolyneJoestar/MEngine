#ifndef BLUR_PASS_INCLUDED
#define BLUR_PASS_INCLUDED

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 screenUV : VAR_SCREEN_UV;
};

CBUFFER_START(_VSM_BLUR)
int _TexSize;
int _CasecadeSize;
CBUFFER_END


TEXTURE2D(_ShadowBlurSource);
SAMPLER(sampler_ShadowBlurSource);

float GetSource(float2 screenUV)
{
    return SAMPLE_TEXTURE2D(_ShadowBlurSource, sampler_ShadowBlurSource, screenUV).r;
}

Varyings DefaultPassVertex(uint vertexID : SV_VertexID)
{
    Varyings output;
    output.positionCS = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? -3.0 : 1.0,
		0.0, 1.0
	);
    output.screenUV = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);
    return output;
}
int GetBlurRadian()
{
    
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
    float ex = 0.0;
    float e_x2 = 0.0;
//    float2 uv[9];
 //   GetUV(input.screenUV, 1024.0, uv);
//    float size = _CasecadeSize > 1 ? 2.0 : 1.0;
    
    int m = input.screenUV.x < 0.5 ? 3 : 0;
    int n = input.screenUV.y < 0.5 ? 3 : 0;
    float4 texcol = 0;
    int c = m + n;
    float allP = 0;
                      
    for (int x = -c; x <= c; x++)
    {

        for (int y = -c; y <= c; y++)
        {
            float p = 1.0 / max(0.5, pow(length(float2(x, y)), 2));
            float d = GetSource((input.screenUV + float2(x, y) / 2048));
            ex += d * p;
            e_x2 += d * d * p;
            allP += p;
        }
    }
    ex /= allP;
    e_x2 /= allP;
    return float4(ex, e_x2, 0.0, 0.0);
}
#endif //BLUR_PASS_INCLUDED