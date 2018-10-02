// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


// Этот шейдер перенесён с сайта генератора профиля Google Cardboard https://vr.google.com/intl/ru_ru/cardboard/viewerprofilegenerator/
// и слегка отрефакторен
Shader "Unlit/DistortionTex"
{
	Properties
	{
		// we have removed support for texture tiling/offset,
		// so make them not be displayed in material inspector
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_DistortionK1("DistortionK1", float) = 0.51
		_DistortionK2("DistortionK2", float) = 0.16
		_ProjectionWorldLeft("ProjectionWorldLeft", Vector) = (0.5, 0.5, 0.5, 0.5)
		_ProjectionEyeLeft("ProjectionEyeLeft", Vector) = (0.5, 0.5, 0.5, 0.5)
		_BackgroundColor("BackgroundColor", Color) = (0.5, 0.5, 0.5, 0.5)
		_DividerColor("DividerColor", Color) = (0.5, 0.5, 0.5, 0.5)
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			// use "vert" function as the vertex shader
	#pragma vertex vert_img
			// use "frag" function as the pixel (fragment) shader
	#pragma fragment frag
	#include "UnityCG.cginc"

			float _DistortionK1;
			float _DistortionK2;
			float4 _ProjectionWorldLeft;
			float4 _ProjectionEyeLeft;
			fixed4 _BackgroundColor;
			fixed4 _DividerColor;
			int _ShowCenter;

			float poly(float val) {
				return 
					(val < 0.0005)
						? 10000.0 
						: 1.0 + (_DistortionK1 + _DistortionK2 * val) * val;
				//return 1.0 + (_DistortionK1 + _DistortionK2 * val) * val;
			}

			float2 barrel(float2 v, float4 projectionWorld, float4 projectionEye) {
				float2 w = (v + projectionEye.zw) / projectionEye.xy;
				return projectionWorld.xy * (poly(dot(w, w)) * w) - projectionWorld.zw;
			}

			float2 direct(float2 v, float4 projectionWorld, float4 projectionEye) {
				float2 w = (v + projectionEye.zw) / projectionEye.xy;
				return projectionWorld.xy * w - projectionWorld.zw;
			}

			// texture we will sample
			sampler2D _MainTex;

			// pixel shader; returns low precision ("fixed4" type)
			// color ("SV_Target" semantic)
			fixed4 frag(v2f_img i) : SV_Target
			{
				// right projections are shifted and vertically mirrored relative to left
				float4 projectionWorldRight = (_ProjectionWorldLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);
				float4 projectionEyeRight = (_ProjectionEyeLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);

				float2 a = (i.uv.x < 0.5) ?
					barrel(float2(2 * i.uv.x, i.uv.y), _ProjectionWorldLeft, _ProjectionEyeLeft) :
					barrel(float2(2 * (i.uv.x - 0.5), i.uv.y), projectionWorldRight, projectionEyeRight);

				// sompare
				//float2 a = (i.uv.x < 0.5) ?
				//	direct(float2(2 * i.uv.x, i.uv.y), _ProjectionWorldLeft, _ProjectionEyeLeft) :
				//	float2(2 * (i.uv.x - 0.5), i.uv.y), _ProjectionWorldLeft, _ProjectionEyeLeft;

				if (_DividerColor.w > 0.0 && abs(i.uv.x - 0.5) < .001) {
					return _DividerColor;
				}
				else if (a.x < 0.0 || a.x > 1.0 || a.y < 0.0 || a.y > 1.0) {
					return _BackgroundColor;
				}
				else {
					return tex2D(_MainTex, float2(a.x * 0.5 + (i.uv.x < 0.5 ? 0.0 : 0.5), a.y));
				}
			}
			ENDCG
		}
	}
}