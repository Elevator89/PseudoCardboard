using UnityEngine;

namespace Assets.Scripts.Control.Rotation
{
	public abstract class BaseInstantRotator : IInstantRotator
	{
		public float RotationStep { get; set; } = 22.5f;
		private bool _canRotate = false;

		public void UpdateRotation(Transform transform)
		{
			Vector3 rotationEuler = transform.rotation.eulerAngles;

			int rotationInput = GetRotationInput();

			//prevent multiple-frame rotation 
			if (_canRotate)
			{
				if (rotationInput != 0)
				{
					float rotationInputAngle = RotationStep * rotationInput;

					rotationEuler.y += rotationInputAngle;
					_canRotate = false;
				}
			}
			else
			{
				_canRotate = rotationInput == 0;
			}

			transform.rotation = Quaternion.Euler(rotationEuler);
		}

		protected abstract int GetRotationInput();
	}
}