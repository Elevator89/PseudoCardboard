using UnityEngine;

namespace Assets.Scripts.Control.Rotation
{
	public class AxisInstantRotator : BaseInstantRotator
	{
		private readonly string _axisName = null;

		public AxisInstantRotator(string axisName)
		{
			_axisName = axisName;
		}

		protected override int GetRotationInput()
		{
			return Mathf.RoundToInt(Input.GetAxisRaw(_axisName));
		}
	}
}