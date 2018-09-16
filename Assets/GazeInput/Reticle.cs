using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GazeInput
{
	[RequireComponent(typeof(Renderer))]
	public class Reticle : MonoBehaviour
	{
		private Renderer _renderer;                     // Reference to the image component that represents the reticle.

		[SerializeField]
		private float _defaultDistance = 10f;

		private Vector3 _originalPosition;
		private Vector3 _originalScale;

		private void Awake()
		{
			_originalPosition = transform.localPosition;
			_originalScale = transform.localScale;
			_renderer = GetComponent<Renderer>();
		}

		public void Hide()
		{
			_renderer.enabled = false;
		}

		public void Show()
		{
			_renderer.enabled = true;
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
