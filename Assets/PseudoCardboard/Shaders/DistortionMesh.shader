// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/DistortionMesh"
{
	Properties
	{
		// we have removed support for texture tiling/offset,
		// so make them not be displayed in material inspector
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_DistortionK1("DistortionK1", float) = 0.51
		_DistortionK2("DistortionK2", float) = 0.16
		_ProjectionLeft("ProjectionLeft", Vector) = (0.5, 0.5, 0.5, 0.5)
		_UnprojectionLeft("UnprojectionLeft", Vector) = (0.5, 0.5, 0.5, 0.5)
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
			float4 _ProjectionLeft;
			float4 _UnprojectionLeft;

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

			float Undistort(float r);
			float2 Undistort(float2 v);
			float Distort(float r);
			float2 Distort(float2 v);
			float GetDistortionFactor(float r2);

			float Undistort(float r)
			{
				float r0 = r * 0.9f;
				float r1 = r / 0.9f;
				float r2;
				float _dr1 = Distort(r1);
				float dr1 = r - Distort(r1);
				float _dr0;
				float dr0;
				while (abs(r0 - r1) > 0.0001f)
				{
					_dr0 = Distort(r0);
					dr0 = r - Distort(r0);
					r2 = r0 - dr0 * ((r0 - r1) / (dr0 - dr1));
					r1 = r0;
					r0 = r2;
					dr1 = dr0;
				}
				return r0;
			}

			float2 Undistort(float2 v)
			{
				float r = length(v);
				float ru = Undistort(r);
				return v * (ru / r);
			}

			float Distort(float r)
			{
				return r * GetDistortionFactor(r*r);
			}

			float2 Distort(float2 v)
			{
				return GetDistortionFactor(dot(v,v)) * v;
			}

			float GetDistortionFactor(float r2)
			{
				return 1.0 + (_DistortionK1 + _DistortionK2 * r2) * r2;
			}

			float2 unbarrel(float2 v, float4 projection, float4 unprojection) {
				float2 w = (v + projection.zw) / projection.xy;
				return unprojection.xy * Undistort(w) - unprojection.zw;
			}

			float2 direct(float2 v, float4 projection, float4 unprojection) {
				float2 w = (v + projection.zw) / projection.xy;
				return unprojection.xy * w - unprojection.zw;
			}

			// vertex shader
			v2f vert(appdata v)
			{
				float4 projectionRight = (_ProjectionLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);
				float4 unprojectionRight = (_UnprojectionLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);

				// clipPos : -1..+1
				float4 clipPos = UnityObjectToClipPos(v.vertex);

				float2 a = (clipPos.x < 0) ?
					unbarrel(float2(clipPos.x + 1, 0.5f * (1- clipPos.y)), _ProjectionLeft, _UnprojectionLeft) :
					unbarrel(float2(clipPos.x, 0.5f * (1 - clipPos.y)), projectionRight, unprojectionRight);

				//float2 a = (clipPos.x < 0) ?
				//	float2(clipPos.x + 1, 0.5f * (clipPos.y + 1)) :
				//	float2(clipPos.x, 0.5f * (clipPos.y + 1));

				v2f o;

				o.vertex = float4((clipPos.x < 0) ? a.x - 1 : a.x, 1 - 2 * a.y, clipPos.z, 1);
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