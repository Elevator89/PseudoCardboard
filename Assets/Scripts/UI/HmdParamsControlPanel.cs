using UnityEngine;
using UnityEngine.UI;
using Assets.PseudoCardboard;

namespace Assets.Scripts.UI
{
	public class HmdParamsControlPanel : MonoBehaviour
	{
		[SerializeField]
		private VrCameraTex VrCamera;

		[SerializeField]
		private Slider InterlensDistanceSlider;
		[SerializeField]
		private Slider ScreenToLensDistSlider;
		[SerializeField]
		private Slider EyeOffsetYSlider;
		[SerializeField]
		private Slider DistortionK1Slider;
		[SerializeField]
		private Slider DistortionK2Slider;
		[SerializeField]
		private Slider FovLeftSlider;
		[SerializeField]
		private Slider FovRightSlider;
		[SerializeField]
		private Slider FovTopSlider;
		[SerializeField]
		private Slider FovBottomSlider;

		void Start()
		{
			InterlensDistanceSlider.value = 1000f * VrCamera.Hmd.InterlensDistance;
			ScreenToLensDistSlider.value = 1000f * VrCamera.Hmd.ScreenToLensDist;
			EyeOffsetYSlider.value = 1000f * VrCamera.Hmd.EyeOffsetY;
			DistortionK1Slider.value = VrCamera.Hmd.DistortionK1;
			DistortionK2Slider.value = VrCamera.Hmd.DistortionK2;
			FovLeftSlider.value = VrCamera.Hmd.MaxFovAngles.Left;
			FovRightSlider.value = VrCamera.Hmd.MaxFovAngles.Right;
			FovTopSlider.value = VrCamera.Hmd.MaxFovAngles.Top;
			FovBottomSlider.value = VrCamera.Hmd.MaxFovAngles.Bottom;

			InterlensDistanceSlider.onValueChanged.AddListener((val) => VrCamera.Hmd.InterlensDistance = 0.001f * val);
			ScreenToLensDistSlider.onValueChanged.AddListener((val) => VrCamera.Hmd.ScreenToLensDist = 0.001f * val);
			EyeOffsetYSlider.onValueChanged.AddListener((val) => VrCamera.Hmd.EyeOffsetY = 0.001f * val);
			DistortionK1Slider.onValueChanged.AddListener((val) => VrCamera.Hmd.DistortionK1 = val);
			DistortionK2Slider.onValueChanged.AddListener((val) => VrCamera.Hmd.DistortionK2= val);
			FovLeftSlider.onValueChanged.AddListener((val) => VrCamera.Hmd.MaxFovAngles.Left = val);
			FovRightSlider.onValueChanged.AddListener((val) => VrCamera.Hmd.MaxFovAngles.Right = val);
			FovTopSlider.onValueChanged.AddListener((val) => VrCamera.Hmd.MaxFovAngles.Top = val);
			FovBottomSlider.onValueChanged.AddListener((val) => VrCamera.Hmd.MaxFovAngles.Bottom = val);
		}

		//public void InterlensDistance(float value)
		//{
		//	VrCamera.Hmd.InterlensDistance = value;
		//}

		//public void ScreenToLensDist(float value)
		//{
		//	VrCamera.Hmd.ScreenToLensDist = value;
		//}

		//public void EyeOffsetY(float value)
		//{
		//	VrCamera.Hmd.EyeOffsetY = value;
		//}

		//public void DistortionK1(float value)
		//{
		//	VrCamera.Hmd.DistortionK1 = value;
		//}

		//public void DistortionK2(float value)
		//{
		//	VrCamera.Hmd.DistortionK2 = value;
		//}

		//public void SetFovLeft(float value)
		//{
		//	VrCamera.Hmd.MaxFovAngles.Left = value;
		//}

		//public void SetFovRight(float value)
		//{
		//	VrCamera.Hmd.MaxFovAngles.Right = value;
		//}

		//public void SetFovTop(float value)
		//{
		//	VrCamera.Hmd.MaxFovAngles.Top = value;
		//}

		//public void SetFovBottom(float value)
		//{
		//	VrCamera.Hmd.MaxFovAngles.Bottom = value;
		//}
	}
}
