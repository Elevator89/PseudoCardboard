using UnityEngine;
using UnityEngine.UI;
using Assets.PseudoCardboard;

namespace Assets.Scripts.UI
{
	public class HmdParamsControlPanel : MonoBehaviour
	{
		[SerializeField]
		private VrCamera VrCamera;

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
			InterlensDistanceSlider.value = 1000f * VrCamera.HmdParameters.InterlensDistance;
			ScreenToLensDistSlider.value = 1000f * VrCamera.HmdParameters.ScreenToLensDist;
			EyeOffsetYSlider.value = 1000f * VrCamera.HmdParameters.EyeOffsetY;
			DistortionK1Slider.value = VrCamera.HmdParameters.DistortionK1;
			DistortionK2Slider.value = VrCamera.HmdParameters.DistortionK2;
			FovLeftSlider.value = VrCamera.HmdParameters.MaxFovAngles.Left;
			FovRightSlider.value = VrCamera.HmdParameters.MaxFovAngles.Right;
			FovTopSlider.value = VrCamera.HmdParameters.MaxFovAngles.Top;
			FovBottomSlider.value = VrCamera.HmdParameters.MaxFovAngles.Bottom;

			InterlensDistanceSlider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.InterlensDistance = 0.001f * val);
			ScreenToLensDistSlider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.ScreenToLensDist = 0.001f * val);
			EyeOffsetYSlider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.EyeOffsetY = 0.001f * val);
			DistortionK1Slider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.DistortionK1 = val);
			DistortionK2Slider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.DistortionK2= val);
			FovLeftSlider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.MaxFovAngles.Left = val);
			FovRightSlider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.MaxFovAngles.Right = val);
			FovTopSlider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.MaxFovAngles.Top = val);
			FovBottomSlider.onValueChanged.AddListener((val) => VrCamera.HmdParameters.MaxFovAngles.Bottom = val);
		}

		//public void InterlensDistance(float value)
		//{
		//	VrCamera.HmdParameters.InterlensDistance = value;
		//}

		//public void ScreenToLensDist(float value)
		//{
		//	VrCamera.HmdParameters.ScreenToLensDist = value;
		//}

		//public void EyeOffsetY(float value)
		//{
		//	VrCamera.HmdParameters.EyeOffsetY = value;
		//}

		//public void DistortionK1(float value)
		//{
		//	VrCamera.HmdParameters.DistortionK1 = value;
		//}

		//public void DistortionK2(float value)
		//{
		//	VrCamera.HmdParameters.DistortionK2 = value;
		//}

		//public void SetFovLeft(float value)
		//{
		//	VrCamera.HmdParameters.MaxFovAngles.Left = value;
		//}

		//public void SetFovRight(float value)
		//{
		//	VrCamera.HmdParameters.MaxFovAngles.Right = value;
		//}

		//public void SetFovTop(float value)
		//{
		//	VrCamera.HmdParameters.MaxFovAngles.Top = value;
		//}

		//public void SetFovBottom(float value)
		//{
		//	VrCamera.HmdParameters.MaxFovAngles.Bottom = value;
		//}
	}
}
