using UnityEngine;

namespace Assets.PseudoCardboard
{
	[RequireComponent(typeof(Camera))]
	public class VrCamera : MonoBehaviour
	{
		void Update()
		{
			Camera camera = GetComponent<Camera>();

			Vector3 leftCamPos = camera.transform.position - camera.transform.right * camera.stereoSeparation;
			Vector3 rightCamPos = camera.transform.position + camera.transform.right * camera.stereoSeparation;

			Camera cam = new Camera();

			Matrix4x4 leftMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			Matrix4x4 rightMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

		}
	}
}
