// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Undistortion"
{
	Properties
	{
		// we have removed support for texture tiling/offset,
		// so make them not be displayed in material inspector
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_EyeCenterOffsetX("EyeCenterOffsetX", float) = -0.032
		_DistortionK1("DistortionK1", float) = 0.51
		_DistortionK2("DistortionK2", float) = 0.16
		_ScreenToLensDist("ScreenToLensDist", float) = 0.045
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			// use "vert" function as the vertex shader
	#pragma vertex vert
			// use "frag" function as the pixel (fragment) shader
	#pragma fragment frag
	#include "UnityCG.cginc"

			float _EyeCenterOffsetX;
			float _DistortionK1;
			float _DistortionK2;
			float _ScreenToLensDist;

			// vertex shader inputs
			struct appdata
			{
				float4 vertex : POSITION; // vertex position
				float2 uv : TEXCOORD0; // texture coordinate
			};

			// vertex shader outputs ("vertex to fragment")
			struct v2f
			{
				float2 uv : TEXCOORD0; // texture coordinate
				float4 vertex : SV_POSITION; // clip space position
			};

			float distortionFactor(float radius)
			{
				float result = 1.0;
				float rFactor = 1.0;
				float rSquared = radius * radius;

				rFactor *= rSquared;
				result += _DistortionK1 * rFactor;

				rFactor *= rSquared;
				result += _DistortionK2 * rFactor;

				return result;
			}

			float distort(float radius)
			{
				return radius * distortionFactor(radius);
			}

			float3 undistort(float3 viewPos)
			{
				float2 center = { _EyeCenterOffsetX, 0.0 };
				float2 radiusV = viewPos.xy - center;
				float radius = length(radiusV) / _ScreenToLensDist;

				float radiusUndist = distort(radius);

				viewPos.xy = center + radiusV * radius / radiusUndist;
				return viewPos;
			}

			// vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				// transform position to clip space
				// (multiply with model*view*projection matrix)

				o.vertex = UnityViewToClipPos(undistort(UnityObjectToViewPos(v.vertex)));

				// just pass the texture coordinate
				o.uv = v.uv;
				return o;
			}

			// texture we will sample
			sampler2D _MainTex;

			// pixel shader; returns low precision ("fixed4" type)
			// color ("SV_Target" semantic)
			fixed4 frag(v2f i) : SV_Target
			{
				// sample texture and return it
				fixed4 col = tex2D(_MainTex, i.uv);
			return col;
			}
			ENDCG
		}
	}
}