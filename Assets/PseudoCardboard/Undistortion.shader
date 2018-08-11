// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Undistortion"
{
	Properties
	{
		// we have removed support for texture tiling/offset,
		// so make them not be displayed in material inspector
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_DistortionParams("DistortionParams", Vector) = (-0.032, 0.0, 0.51, 0.16)
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

			// Convert point from world space to undistorted camera space.
			float3 undistort(float3 viewPos, float2 _center, float k1, float k2)
			{
				float2 rad = viewPos.xy - _center;
				float r2 = dot(rad, rad);

				viewPos.xy += rad * (k1 + k2 * r2) * r2;
				return viewPos;
			}
			

			float distortRadius(float radius, float k1, float k2)
			{
				float r2 = radius * radius;
				return radius * (1 + (k1 + k2 * r2) * r2);
			}

			float distortRadiusInverse(float radius, float k1, float k2)
			{
				float r0 = 0;
				float r1 = 1;
				float dr0 = radius - distortRadius(r0, k1, k2);
				while (abs(r1 - r0) > 0.001) {
					float dr1 = radius - distortRadius(r1, k1, k2);
					float r2 = r1 - dr1 * ((r1 - r0) / (dr1 - dr0));
					r0 = r1;
					r1 = r2;
					dr0 = dr1;
				}
				return r1;
			}

			float3 undistort2(float3 viewPos, float2 _center, float k1, float k2)
			{
				float2 radiusV = viewPos.xy - _center;
				float radius = length(radiusV);
				float radiusUndist = distortRadius(radius, k1, k2);

				viewPos.xy += _center + radiusV * radiusUndist / radius;
				return viewPos;
			}

			float4 _DistortionParams;

			// vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				// transform position to clip space
				// (multiply with model*view*projection matrix)

				o.vertex = UnityViewToClipPos(undistort2(UnityObjectToViewPos(v.vertex), _DistortionParams.xy, -_DistortionParams.z, -_DistortionParams.w));

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