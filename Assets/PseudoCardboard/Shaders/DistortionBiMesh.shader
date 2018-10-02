// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/DistortionBiMesh"
{
	Properties
	{
		// we have removed support for texture tiling/offset,
		// so make them not be displayed in material inspector
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_DistortionK1("DistortionK1", float) = 0.51
		_DistortionK2("DistortionK2", float) = 0.16
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

			float _DistortionK1;
			float _DistortionK2;

			// vertex shader inputs
			struct appdata
			{
				float4 vertex : POSITION; // vertex position
				float2 uv : TEXCOORD0; // texture coordinate
			};

			// vertex shader outputs ("vertex to fragment")
			struct v2f
			{
				float4 vertex : SV_POSITION; // clip space position
				float2 uv : TEXCOORD0; // texture coordinate
			};

			float Undistort(float r);
			float Distort(float r);
			float GetDistortionFactor(float r2);

			// Нужно найти корни многочлена 5-й степени. Сам многочлен описан в методе Distort
			// Решение находится численным методом.
			float Undistort(float r)
			{
				float r0 = r * 0.9f;
				float r1 = r / 0.9f;
				float r2;
				float dr1 = r - Distort(r1);
				float dr0;
				while (abs(r0 - r1) > 0.0001f)
				{
					dr0 = r - Distort(r0);
					r2 = r0 - dr0 * ((r0 - r1) / (dr0 - dr1));
					r1 = r0;
					r0 = r2;
					dr1 = dr0;
				}
				return r0;
			}

			float Distort(float r)
			{
				return r * GetDistortionFactor(r*r);
			}

			float GetDistortionFactor(float r2)
			{
				return 1.0 + (_DistortionK1 + _DistortionK2 * r2) * r2;
			}

			// vertex shader
			v2f vert(appdata v)
			{
				// clipPos : -1..+1
				float3 viewPos = UnityObjectToViewPos(v.vertex);

				float r = length(viewPos.xy);
				float ru = Undistort(r);

				float3 finalViewPos = float3(viewPos.x, viewPos.y, viewPos.z * r / ru);

				v2f o;
				o.vertex = UnityViewToClipPos(finalViewPos);
				o.uv = v.uv;

				return o;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}