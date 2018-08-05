using System.Linq;
using UnityEngine;

namespace Assets.PseudoCardboard
{
	[RequireComponent(typeof(Camera))]
	public class VrCamera : MonoBehaviour
	{
		private Camera _centralCam;
		private Camera _leftEyeCam;
		private Camera _rightEyeCam;

		void Awake()
		{
			_centralCam = GetComponent<Camera>();

			_leftEyeCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Left);
			_rightEyeCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Right);

			_leftEyeCam.transform.localPosition = - 0.5f * Vector3.right * _centralCam.stereoSeparation;
			_rightEyeCam.transform.localPosition = 0.5f * Vector3.right * _centralCam.stereoSeparation;

			_leftEyeCam.transform.localRotation = Quaternion.identity;
			_rightEyeCam.transform.localRotation = Quaternion.identity;
		}

		void Update()
		{

		}
	}
}
