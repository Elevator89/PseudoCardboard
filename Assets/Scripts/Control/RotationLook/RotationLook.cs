using UnityEngine;

namespace Assets.Scripts.Control.RotationLook
{
	public class GyroscopeLook : IRotationLook
	{
		private readonly Quaternion _rotFix = new Quaternion(0, 0, 0.7071068f, 0);
		private readonly Quaternion _rotRoot = Quaternion.Euler(90, 90, 0);

		// Update is called once per frame
		public void LookRotation(Transform camera)
		{
			if (!SystemInfo.supportsGyroscope)
				return;

			if (!Input.gyro.enabled)
			{
				Input.gyro.enabled = true;
			}

			camera.localRotation = _rotRoot * Input.gyro.attitude * _rotFix;
		}
	}
}