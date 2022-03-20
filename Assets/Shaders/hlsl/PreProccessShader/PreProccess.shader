Shader "MyPipeline/PreProccess"
{
    Properties
    {
        _MainTex ("Texture", Cube) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "GenIrradiance.hlsl"
            
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM


            #pragma vertex vert
            #pragma fragment frag_tex2tex

            sampler2D _MainTex;
            samplerCUBE _CubeTex;
            float4 _RandomVector;
            #include "GenIrradiance.hlsl"

            ENDHLSL
        }
    }
}
