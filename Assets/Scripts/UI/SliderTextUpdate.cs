using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	[RequireComponent(typeof(Slider))]
	public class SliderTextUpdate : MonoBehaviour
	{
		private Slider Slider;

		[SerializeField]
		private Text Text;
		[SerializeField]
		private int Decimals = 0;

		void OnEnable()
		{
			Slider = GetComponent<Slider>();
            Slider.onValueChanged.AddListener(UpdateText);
        }

	    void OnDisable()
	    {
            Slider.onValueChanged.RemoveListener(UpdateText);
        }

        private void UpdateText(float value)
		{
			Text.text = value.ToString("N" + Decimals);
		}
	}
}
