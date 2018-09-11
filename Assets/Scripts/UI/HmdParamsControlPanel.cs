using UnityEngine;
using UnityEngine.UI;
using Assets.PseudoCardboard;

namespace Assets.Scripts.UI
{
	public class HmdParamsControlPanel : MonoBehaviour
	{
		private HmdParameters _hmd;

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
            _hmd = FindObjectOfType<HmdParamRoot>().HmdParameters;

            InterlensDistanceSlider.value = 1000f * _hmd.InterlensDistance;
			ScreenToLensDistSlider.value = 1000f * _hmd.ScreenToLensDist;
			EyeOffsetYSlider.value = 1000f * _hmd.EyeOffsetY;
			DistortionK1Slider.value = _hmd.DistortionK1;
			DistortionK2Slider.value = _hmd.DistortionK2;
			FovLeftSlider.value = _hmd.MaxFovAngles.Left;
			FovRightSlider.value = _hmd.MaxFovAngles.Right;
			FovTopSlider.value = _hmd.MaxFovAngles.Top;
			FovBottomSlider.value = _hmd.MaxFovAngles.Bottom;

			InterlensDistanceSlider.onValueChanged.AddListener((val) => _hmd.InterlensDistance = 0.001f * val);
			ScreenToLensDistSlider.onValueChanged.AddListener((val) => _hmd.ScreenToLensDist = 0.001f * val);
			EyeOffsetYSlider.onValueChanged.AddListener((val) => _hmd.EyeOffsetY = 0.001f * val);
			DistortionK1Slider.onValueChanged.AddListener((val) => _hmd.DistortionK1 = val);
			DistortionK2Slider.onValueChanged.AddListener((val) => _hmd.DistortionK2= val);
			FovLeftSlider.onValueChanged.AddListener((val) => _hmd.MaxFovAngles.Left = val);
			FovRightSlider.onValueChanged.AddListener((val) => _hmd.MaxFovAngles.Right = val);
			FovTopSlider.onValueChanged.AddListener((val) => _hmd.MaxFovAngles.Top = val);
			FovBottomSlider.onValueChanged.AddListener((val) => _hmd.MaxFovAngles.Bottom = val);
		}

		//public void InterlensDistance(float value)
		//{
		//	Hmd.InterlensDistance = value;
		//}

		//public void ScreenToLensDist(float value)
		//{
		//	Hmd.ScreenToLensDist = value;
		//}

		//public void EyeOffsetY(float value)
		//{
		//	Hmd.EyeOffsetY = value;
		//}

		//public void DistortionK1(float value)
		//{
		//	Hmd.DistortionK1 = value;
		//}

		//public void DistortionK2(float value)
		//{
		//	Hmd.DistortionK2 = value;
		//}

		//public void SetFovLeft(float value)
		//{
		//	Hmd.MaxFovAngles.Left = value;
		//}

		//public void SetFovRight(float value)
		//{
		//	Hmd.MaxFovAngles.Right = value;
		//}

		//public void SetFovTop(float value)
		//{
		//	Hmd.MaxFovAngles.Top = value;
		//}

		//public void SetFovBottom(float value)
		//{
		//	Hmd.MaxFovAngles.Bottom = value;
		//}
	}
}
