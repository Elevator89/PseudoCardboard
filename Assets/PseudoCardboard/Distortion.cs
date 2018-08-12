using UnityEngine;

namespace Assets.PseudoCardboard
{
	public class Distortion
	{
		public float DistortionK1;
		public float DistortionK2;

		public Distortion(float distortionK1, float distortionK2)
		{
			DistortionK1 = distortionK1;
			DistortionK2 = distortionK2;
		}

		public float DistortInverse(float radius)
		{
			float r0 = radius / 0.9f;
			float r1 = radius * 0.9f;
			float r2;
			float dr0 = radius - DistortRadius(r0);
			float dr1;
			while (Mathf.Abs(r1 - r0) > 0.0001f)
			{
				dr1 = radius - DistortRadius(r1);
				r2 = r1 - dr1 * ((r1 - r0) / (dr1 - dr0));
				r0 = r1;
				r1 = r2;
				dr0 = dr1;
			}
			return r1;
		}

		public float DistortRadius(float radius)
		{
			return radius * GetDistortionFactor(radius);
		}

		float GetDistortionFactor(float radius)
		{
			float rSquared = radius * radius;
			return 1 + DistortionK1 * rSquared + DistortionK2 * rSquared * rSquared;
		}
	}
}
