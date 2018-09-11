using UnityEngine;

namespace Assets.PseudoCardboard
{
    [ExecuteInEditMode]
    public class FovScaler : MonoBehaviour
    {
        public void SetFov(Fov fovTanAngles)
        {
            float dist = transform.localPosition.z;

            Fov fovDistances = fovTanAngles * dist;

            transform.localPosition = new Vector3(0.5f * (fovDistances.Right - fovDistances.Left), 0.5f * (fovDistances.Top - fovDistances.Bottom), dist);
            transform.localScale = new Vector3(fovDistances.Right + fovDistances.Left, fovDistances.Top + fovDistances.Bottom, 1f);
        }
    }
}
