using Assets.PseudoCardboard;
using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UiDpiScaler : MonoBehaviour
    {
        public int TargetUnitsInMeter = 100 * 100; // 100 units per cm, 100 cm in 1 m

        void OnEnable()
        {
            DisplayParameters displayParameters = DisplayParameters.Collect();

            GetComponent<RectTransform>().localScale = 
                new Vector3(
                    displayParameters.Dpm.x / TargetUnitsInMeter, 
                    displayParameters.Dpm.y / TargetUnitsInMeter, 
                    1f);
        }
    }
}
