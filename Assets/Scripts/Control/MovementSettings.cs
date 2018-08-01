using System;
using UnityEngine;

namespace Assets.Scripts.Control
{
	[Serializable]
	public class MovementSettings
	{
		public bool AllowMovement = true;

		public float ForwardSpeed = 4.0f;   // Speed when walking forward
		public float BackwardSpeed = 4.0f;  // Speed when walking backwards
		public float StrafeSpeed = 4.0f;    // Speed when walking sideways
		public float RunMultiplier = 2.0f;   // Speed when sprinting
		public KeyCode RunKey = KeyCode.LeftShift;
		public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
		[HideInInspector]
		public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
		private bool _running;
#endif

		public void UpdateDesiredTargetSpeed(Vector2 input)
		{
			if (!AllowMovement)
				return;

			if (input == Vector2.zero)
				return;
			if (input.x > 0 || input.x < 0)
			{
				//strafe
				CurrentTargetSpeed = StrafeSpeed;
			}
			if (input.y < 0)
			{
				//backwards
				CurrentTargetSpeed = BackwardSpeed;
			}
			if (input.y > 0)
			{
				//forwards
				//handled last as if strafing and moving forward at the same time forwards speed should take precedence
				CurrentTargetSpeed = ForwardSpeed;
			}
#if !MOBILE_INPUT
			if (Input.GetKey(RunKey))
			{
				CurrentTargetSpeed *= RunMultiplier;
				_running = true;
			}
			else
			{
				_running = false;
			}
#endif
		}

#if !MOBILE_INPUT
		public bool Running
		{
			get { return _running; }
		}
#endif
	}
}