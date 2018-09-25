using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Slider))]
	public class SliderTextUpdate : MonoBehaviour
	{
		private Slider Slider;

		[SerializeField]
		private Text Text;
		[SerializeField]
		private int Decimals = 0;

	    void Awake()
	    {
            Slider = GetComponent<Slider>();
        }

        void OnEnable()
		{
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
