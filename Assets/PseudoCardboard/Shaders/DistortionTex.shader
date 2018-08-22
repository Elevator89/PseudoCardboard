// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/DistortionTex"
{
	Properties
	{
		// we have removed support for texture tiling/offset,
		// so make them not be displayed in material inspector
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_ScreenToLensDist("ScreenToLensDist", float) = 0.045
		_DistortionK1("DistortionK1", float) = 0.51
		_DistortionK2("DistortionK2", float) = 0.16
		_ProjectionLeft("ProjectionLeft", Vector) = (0.5, 0.5, 0.5, 0.5)
		_UnprojectionLeft("UnprojectionLeft", Vector) = (0.5, 0.5, 0.5, 0.5)
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

			float _ScreenToLensDist;
			float _DistortionK1;
			float _DistortionK2;
			float4 _ProjectionLeft;
			float4 _UnprojectionLeft;
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

			float2 barrel(float2 v, float4 projection, float4 unprojection) {
				float2 w = (v + unprojection.zw) / unprojection.xy;
				return projection.xy * (poly(dot(w, w)) * w) - projection.zw;
			}

			float2 direct(float2 v, float4 projection, float4 unprojection) {
				float2 w = (v + unprojection.zw) / unprojection.xy;
				return projection.xy * w - projection.zw;
			}

			// texture we will sample
			sampler2D _MainTex;

			// pixel shader; returns low precision ("fixed4" type)
			// color ("SV_Target" semantic)
			fixed4 frag(v2f_img i) : SV_Target
			{
				// right projections are shifted and vertically mirrored relative to left
				float4 projectionRight = (_ProjectionLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);
				float4 unprojectionRight = (_UnprojectionLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);

				float2 a = (i.uv.x < 0.5) ?
					barrel(float2(2 * i.uv.x, i.uv.y), _ProjectionLeft, _UnprojectionLeft) :
					barrel(float2(2 * (i.uv.x - 0.5), i.uv.y), projectionRight, unprojectionRight);

				// sompare
				//float2 a = (i.uv.x < 0.5) ?
				//	direct(float2(2 * i.uv.x, i.uv.y), _ProjectionLeft, _UnprojectionLeft) :
				//	float2(2 * (i.uv.x - 0.5), i.uv.y), _ProjectionLeft, _UnprojectionLeft;

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