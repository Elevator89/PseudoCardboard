using System;

namespace Assets.PseudoCardboard
{
	[Serializable]
	public class HmdParameters
	{
		public float DistortionK1 = 0.51f;
		public float DistortionK2 = 0.16f;
		public float InterlensDistance = 0.065f;
		public float ScreenToLensDist = 0.045f;
		public float EyeOffsetY = 0.035f;
		public FovAngles MaxFovAngles = new FovAngles { Left = 50, Right = 50, Top = 50, Bottom = 50 };
	}
}
