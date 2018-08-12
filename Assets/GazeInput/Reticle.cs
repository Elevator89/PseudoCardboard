using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GazeInput
{
	[RequireComponent(typeof(Image))]
	public class Reticle : MonoBehaviour
	{
		private Image _image;                     // Reference to the image component that represents the reticle.

		[SerializeField]
		private float _defaultDistance = 10f;

		private Vector3 _originalPosition;
		private Vector3 _originalScale;

		private void Awake()
		{
			_originalPosition = transform.localPosition;
			_originalScale = transform.localScale;
			_image = GetComponent<Image>();
		}

		public void Hide()
		{
			_image.enabled = false;
		}

		public void Show()
		{
			_image.enabled = true;
		}

		public void SetPosition(Vector3 worldPosition, float distance)
		{
			transform.position = worldPosition;
			transform.localScale = _originalScale * distance / _defaultDistance;
		}

		public void SetDefaultPosition()
		{
			transform.localPosition = _originalPosition;
			transform.localScale = _originalScale;
		}
	}
}
