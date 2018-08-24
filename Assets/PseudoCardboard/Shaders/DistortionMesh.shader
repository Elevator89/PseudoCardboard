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

			float poly(float val) {
				return 1.0 + (_DistortionK1 + _DistortionK2 * val) * val;
			}

			float2 barrel(float2 v, float4 projection, float4 unprojection) {
				float2 w = (v + unprojection.zw) / unprojection.xy;
				return projection.xy * (poly(dot(w, w)) * w) - projection.zw;
			}

			float2 direct(float2 v, float4 projection, float4 unprojection) {
				float2 w = (v + unprojection.zw) / unprojection.xy;
				return projection.xy * w - projection.zw;
			}

			// vertex shader
			v2f vert(appdata v)
			{
				float4 projectionRight = (_ProjectionLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);
				float4 unprojectionRight = (_UnprojectionLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);

				float3 viewPos = UnityObjectToViewPos(v.vertex);

				float2 a = (viewPos.x < 0) ?
					barrel(float2(viewPos.x + 1, 0.5f * (viewPos.y + 1)), _ProjectionLeft, _UnprojectionLeft) :
					barrel(float2(viewPos.x, 0.5f * (viewPos.y + 1)), projectionRight, unprojectionRight);

				//float2 a = (viewPos.x < 0) ?
				//	float2(viewPos.x + 1, 0.5f * (viewPos.y + 1)) :
				//	float2(viewPos.x, 0.5f * (viewPos.y + 1));

				v2f o;

				o.vertex = UnityViewToClipPos(float3((viewPos.x < 0) ? a.x - 1 : a.x, 2 * a.y - 1, viewPos.z));

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