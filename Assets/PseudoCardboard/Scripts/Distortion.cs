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

		public float UndistortTanAngle(float tanAngle)
		{
			float r0 = tanAngle * 0.9f;
			float r1 = tanAngle / 0.9f;
			float r2;
			float dr1 = tanAngle - DistortTanAngle(r1);
			float dr0;
			while (Mathf.Abs(r0 - r1) > 0.0001f)
			{
				dr0 = tanAngle - DistortTanAngle(r0);
				r2 = r0 - dr0 * ((r0 - r1) / (dr0 - dr1));
				r1 = r0;
				r0 = r2;
				dr1 = dr0;
			}
			return r0;
		}

		public float DistortTanAngle(float tanAngle)
		{
			return tanAngle * GetDistortionFactor(tanAngle);
		}

		float GetDistortionFactor(float radius)
		{
			float rSquared = radius * radius;
			return 1 + DistortionK1 * rSquared + DistortionK2 * rSquared * rSquared;
		}
	}
}
