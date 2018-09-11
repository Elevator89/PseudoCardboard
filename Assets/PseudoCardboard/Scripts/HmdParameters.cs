using System;
using UnityEngine;

namespace Assets.PseudoCardboard
{
	[Serializable]
	public class HmdParameters
	{
        [Range(0f, 2f)]
		public float DistortionK1 = 0.51f;

		[Range(0f, 2f)]
		public float DistortionK2 = 0.16f;

		[Range(0f, 0.2f)]
		public float InterlensDistance = 0.065f;

		[Range(0.01f, 0.2f)]
		public float ScreenToLensDist = 0.045f;

		[Range(0f, 0.1f)]
		public float EyeOffsetY = 0.035f;

		public Fov MaxFovAngles = new Fov { Left = 50, Right = 50, Top = 50, Bottom = 50 };
	}
}
