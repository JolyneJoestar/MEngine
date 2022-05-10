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
			Tags { "LightMode" = "DeferredLighting" }
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
			Tags { "LightMode" = "SSAO" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment SSAOFragment
            #include "SSAOPass.hlsl"

            ENDHLSL
        }

        Pass
        {
			Tags { "LightMode" = "SSAOBlur" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment SSAOBlurFragment
            #include "SSAOBlurPass.hlsl"

            ENDHLSL
        }

        Pass
        {
			Tags { "LightMode" = "LightVolume" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment deferredLightVolumeFrag
            #include "LightVolumePass.hlsl"

            ENDHLSL
        }

		Pass
		{
			Tags { "LightMode" = "LightVolumeBlur" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment lightVolumeBlurFragment
			#include "LightVolumeBlurPass.hlsl"

			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "SSR" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment SSRGenPass
			#include "SSR.hlsl"

			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "BloomInput" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment BloomGetSource
			#include "BloomInput.hlsl"

			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "BloomBlur" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment BloomBlurPass
			#include "BloomBlur.hlsl"

			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "TAA" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment TAA
			#include "TAA.hlsl"

			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "HBAOGen" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment HBAOGenFragment
			#include "HBAOPass.hlsl"

			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "Bloom" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment BloomFinal
			#include "Bloom.hlsl"
			ENDHLSL
		}
    }
}
