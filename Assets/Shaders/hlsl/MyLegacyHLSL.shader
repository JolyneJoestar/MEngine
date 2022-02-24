// Upgrade NOTE: replaced 'glstate_matrix_projection' with 'UNITY_MATRIX_P'

// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyPipeline/LegacyHLSL"
{
	Properties
	{
		MTint("Tint",Color) = (1,1,1,1)
		MTexture("Texture",2D) = "white"{}
		MMetallic("Metallic",Range(0,1)) = 0.5
		MSmoothness("Smoothness",Range(0,1)) = 0.5
		MCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Toggle(_CLIPPING)] MClipping("Alpha Clipping", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]MSrcBlend("Src Blend",float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]MDstBlend("Dst Blend", float) = 0
		[Enum(Off,0,On,1)]MZWrite("ZWrite",float) = 1

	}
		SubShader
		{

			Pass
			{
				Blend[MSrcBlend][MDstBlend]
				ZWrite[MZWrite]

				Tags { "LightMode" = "SPRDefaultLegay" }

				HLSLPROGRAM

				#pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
				#pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
				#pragma multi_compile _ _PCF _VSM _PCSS
				#pragma multi_compile _ LIGHTMAP_ON
				#pragma multi_compile_instancing
				#pragma vertex LegacyVertex
				#pragma fragment LegacyFragment

				#include "MyLegacyLightPass.hlsl"

				ENDHLSL
			}

			Pass{
				Tags{
					"LightMode" = "ShadowCaster"
				}

				//ColorMask 0

				HLSLPROGRAM
				#pragma target 3.5
				#pragma shader_feature _CLIPPING
				#pragma multi_compile_instancing
				#pragma multi_compile_VSM
				#pragma vertex ShadowCasterPassVertex
				#pragma fragment ShadowCasterPassFragment

				#include "ShadowCasterPass.hlsl"
					
				ENDHLSL
			}

			Pass {
				Tags {
					"LightMode" = "Meta"
				}

				Cull Off

				HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex MetaPassVertex
				#pragma fragment MetaPassFragment
				#include "MetaPass.hlsl"
				ENDHLSL
			}

			Pass{
				Tags{
					"LightMode" = "ShadowBlur"
				}

				//ColorMask 0

				HLSLPROGRAM
				#pragma target 3.5
				#pragma shader_feature _CLIPPING
				#pragma multi_compile_instancing
				#pragma multi_compile_VSM
				#pragma vertex ShadowBlurPassVertex
				#pragma fragment ShadowBlurPassFragment

				#include "ShadowBlurPass.hlsl"
					
				ENDHLSL
			}
		}
}
