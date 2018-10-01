Shader "Unlit/DistortionMesh"
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
			float4 _ProjectionWorldLeft;
			float4 _ProjectionEyeLeft;

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
				float z : TEXCOORD1; // texture coordinate
			};

			float Undistort(float r);
			float Distort(float r);
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

			float Distort(float r)
			{
				return r * GetDistortionFactor(r*r);
			}

			float GetDistortionFactor(float r2)
			{
				return 1.0 + (_DistortionK1 + _DistortionK2 * r2) * r2;
			}

			float3 SplitClip(float3 clip) {
				return (clip.x < 0) ?
					float3(clip.x + 1, 0.5f * (1 - clip.y), clip.z) :
					float3(clip.x, 0.5f * (1 - clip.y), clip.z);
			}

			float2 ClipToView(float2 clipPos, float4 projectionLine) {
				return (clipPos + projectionLine.zw) / projectionLine.xy;
			}

			float2 ViewToClip(float2 viewPos, float4 projectionLine) {
				return projectionLine.xy * viewPos - projectionLine.zw;
			}

			float2 MergeClip(float2 splitted, bool left) {
				return float2(left ? splitted.x - 1 : splitted.x, 1 - 2 * splitted.y);
			}

			// vertex shader
			v2f vert(appdata v)
			{
				float4 projectionWorldRight = (_ProjectionWorldLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);
				float4 projectionEyeRight = (_ProjectionEyeLeft + float4(0.0, 0.0, 1.0, 0.0)) * float4(1.0, 1.0, -1.0, 1.0);

				// clipPos : -1..+1
				float4 clipPos = UnityObjectToClipPos(v.vertex);
				
				float3 splittedClipPos = SplitClip(clipPos.xyz);

				bool left = clipPos.x < 0;

				float4 projectionWorld = left ? _ProjectionWorldLeft : projectionWorldRight;
				float4 projectionEye = left ? _ProjectionEyeLeft : projectionEyeRight;

				float2 viewPos = ClipToView(splittedClipPos, projectionWorld);
				float radius = length(viewPos);
				float radiusUndistorted = Undistort(radius);
				float undistortionZFactor = radius / radiusUndistorted;
				float2 eyeClipPos = ViewToClip(viewPos / undistortionZFactor, projectionEye);

				float2 mergedClipPos = MergeClip(eyeClipPos, left);

				float z = clipPos.z * undistortionZFactor;

				v2f o;
				o.vertex = float4(mergedClipPos, clipPos.z, clipPos.w);

				o.uv = v.uv / z;
				o.z = 1/z;

				return o;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv / i.z);
			}
			ENDCG
		}
	}
}