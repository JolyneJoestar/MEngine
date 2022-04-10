Shader "MyPipeline/DeferredRender"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            HLSLPROGRAM
			#pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
			#pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
			#pragma multi_compile _ _PCF _VSM _ESM _PCSS _CSM
			#pragma multi_compile _ LIGHTMAP_ON
            #pragma vertex vert
            #pragma fragment deferredLightingFragPass

			#include "MyDeferredLightPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment SSAOFragment
            #include "SSAOPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment SSAOFragment
            #include "SSAOBlurPass.hlsl"

            ENDHLSL
        }
    }
}
