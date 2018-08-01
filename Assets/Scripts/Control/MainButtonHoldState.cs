using UnityEngine;

namespace Assets.Scripts.Control
{
	public class MainButtonHoldState
	{
		private float _timePressed;

		public MainButtonHoldState()
		{
			_timePressed = 0f;
		}

		public void ProcessInput()
		{
			if (!IsPressing())
			{
				_timePressed = 0.0f;
				return;
			}

			_timePressed += Time.deltaTime;
		}

		public bool IsHeld(float holdThreshold)
		{
			return _timePressed >= holdThreshold;
		}

		private static bool IsPressing()
		{
			return Input.GetMouseButton(0) || Input.GetButton("Fire1");
		}
	}
}