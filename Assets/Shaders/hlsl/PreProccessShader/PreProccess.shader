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
            #pragma fragment frag_cube2tex

            #include "GenIrradiance1.hlsl"
            
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM


            #pragma vertex vert
            #pragma fragment frag_tex2tex

            #include "GenIrradiance2.hlsl"

            ENDHLSL
        }
    }
}
