using Assets.Scripts.Control.Rotation;
using Assets.Scripts.Control.RotationLook;
using UnityEngine;

namespace Assets.Scripts.Control
{
	public class GyroscopePlayerController : PlayerController
	{
#if UNITY_ANDROID
		public IRotationLook RotationLook = new GyroscopeLook();
#else
		public IRotationLook RotationLook = new MouseLook();
#endif

		private readonly MainButtonHoldState _holdState = new MainButtonHoldState();

		private IInstantRotator _instantRotator = new AxisInstantRotator("Horizontal");
		protected override IInstantRotator InstantRotator { get { return _instantRotator; } }

		protected override void UpdateCameraRotation(Transform cam)
		{
			//avoids the mouse looking if the game is effectively paused
			if (Mathf.Abs(Time.timeScale) < float.Epsilon)
				return;

			// get the rotation before it's changed
			float oldYRotation = cam.eulerAngles.y;

			RotationLook.LookRotation(cam);

			if (Grounded || AdvancedSettings.AirControl)
			{
				// Rotate the rigidbody velocity to match the new direction that the character is looking
				Quaternion velRotation = Quaternion.AngleAxis(cam.eulerAngles.y - oldYRotation, Vector3.up);
				RigidBody.velocity = velRotation * RigidBody.velocity;
			}
		}

		protected override Vector2 GetMovementInput()
		{
			_holdState.ProcessInput();
			if (_holdState.IsHeld(1.0f))
			{
				return Vector2.up;
			}

			return new Vector2 { x = 0f, y = Input.GetAxis("Vertical") };
		}
	}
}