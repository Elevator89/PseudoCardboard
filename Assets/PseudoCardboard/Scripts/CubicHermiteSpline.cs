﻿namespace Assets.PseudoCardboard
{
	public class CubicHermiteSpline
	{
		float[] _x;
		float[] _fx;
		float[] _dfx;

		public CubicHermiteSpline(Distortion distortion, float[] x)
		{
			_x = x;
			_fx = new float[x.Length];
			_dfx = new float[x.Length];

			for (int i = 0; i < x.Length; ++i)
			{
				_fx[i] = distortion.Undistort(x[i]);
				_dfx[i] = 1f / distortion.DistortDerivative(_fx[i]);
			}
		}

		public float GetValue(float x)
		{
			int i = GetIndex(x);

			float x0 = _x[i];
			float x1 = _x[i + 1];

			float fx0 = _fx[i];
			float fx1 = _fx[i + 1];

			float dfx0 = _dfx[i];
			float dfx1 = _dfx[i + 1];

			float t = (x - x0) / (x1 - x0);
			float t2 = t * t;
			float t3 = t2 * t;

			return
				(2f * t3 - 3f * t2 + 1) * fx0
				+ (t3 - 2f * t2 + t) * (x1 - x0) * dfx0
				+ (-2f * t3 + 3f * t2) * fx1
				+ (t3 - t2) * (x1 - x0) * dfx1;
		}


		private int GetIndex(float x)
		{
			if (x < _x[0])
				return 0;

			for (int i = 1; i < _x.Length; ++i)
			{
				if (x < _x[i])
					return i - 1;
			}

			return _x.Length - 2;
		}

	}
}