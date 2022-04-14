Shader "Hidden/Custom RP/Shadow Post Process" {

	SubShader{
		Cull Off
		ZTest Always
		ZWrite Off

		HLSLINCLUDE
		#include "Common.hlsl"
		ENDHLSL

		Pass {
			Name "VSM"

			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment CopyPassFragment

				#include "VSMBlurPass.hlsl"
			ENDHLSL
		}

		Pass {
			Name "CSM"

			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment FourierGenPassFragment

				#include "ConvolutionPrePass.hlsl"
			ENDHLSL
		}

		Pass {
			Name "CSM_BLUR"

			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment FourierBlurPassFragment

				#include "ConvolutionBlurPass.hlsl"
			ENDHLSL
		}
	}
}
