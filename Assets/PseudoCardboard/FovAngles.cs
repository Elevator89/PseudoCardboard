using System;
using UnityEngine;

namespace Assets.PseudoCardboard
{
	[Serializable]
	public class FovAngles
	{
		[Range(0f, 90f)]
		public float Left;
		[Range(0f, 90f)]
		public float Right;
		[Range(0f, 90f)]
		public float Top;
		[Range(0f, 90f)]
		public float Bottom;
	}
}
