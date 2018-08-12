using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		void Awake()
		{
			Slider = GetComponent<Slider>();
		}

		public void UpdateText()
		{
			Text.text = Slider.value.ToString("N" + Decimals);
		}
	}
}
