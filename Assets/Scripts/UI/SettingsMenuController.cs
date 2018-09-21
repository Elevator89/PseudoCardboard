using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class SettingsMenuController: MonoBehaviour
    {
        [SerializeField]
        private Button SettingsOpenButton;

        [SerializeField]
        private HmdParamsControlPanel HmdParamsControlPanel;

        public void OpenSettings()
        {
            HmdParamsControlPanel.gameObject.SetActive(true);
            SettingsOpenButton.gameObject.SetActive(false);
        }

        public void CloseSettings()
        {
            HmdParamsControlPanel.gameObject.SetActive(false);
            SettingsOpenButton.gameObject.SetActive(true);
        }
    }
}
