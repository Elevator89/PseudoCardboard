using UnityEngine;

namespace Assets.Scripts.Control.Rotation
{
	public interface IInstantRotator
	{
		void UpdateRotation(Transform transform);
	}
}