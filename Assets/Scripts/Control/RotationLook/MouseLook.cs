using System;
using UnityEngine;

namespace Assets.Scripts.Control.RotationLook
{
	public class MouseLook : IRotationLook
	{
		public float XSensitivity = 2f;
		public float YSensitivity = 2f;
		public float MinimumX = -85f;
		public float MaximumX = 85f;
		public bool LockCursor = true;

		private bool _cursorIsLocked = true;

		public void LookRotation(Transform camera)
		{
			float deltaY = Input.GetAxis("Mouse X") * XSensitivity;
			float deltaX = Input.GetAxis("Mouse Y") * YSensitivity;

			Vector3 eulerAngles = camera.localRotation.eulerAngles;

			if (deltaY != 0)
			{
				eulerAngles = camera.localRotation.eulerAngles;
			}

			eulerAngles.x = Mathf.Clamp(GetIsotropicAngle(eulerAngles.x - deltaX), MinimumX, MaximumX);
			eulerAngles.y += deltaY;
			camera.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0.0f);

			UpdateCursorLock();
		}

		public void UpdateCursorLock()
		{
			//if the user set "lockCursor" we check & properly lock the cursos
			if (LockCursor)
				InternalLockUpdate();
		}

		private void InternalLockUpdate()
		{
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				_cursorIsLocked = false;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				_cursorIsLocked = true;
			}

			if (_cursorIsLocked)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else if (!_cursorIsLocked)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		static float GetIsotropicAngle(float angle)
		{
			return angle > 180.0f ? angle - 360.0f : angle;
		}
	}
}
