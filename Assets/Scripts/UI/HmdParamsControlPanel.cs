using UnityEngine;
using UnityEngine.UI;
using Assets.PseudoCardboard;

namespace Assets.Scripts.UI
{
    public class HmdParamsControlPanel : MonoBehaviour
    {
        private HmdParameters _hmd;
        private MeshParameters _mesh;

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

        [SerializeField]
        private Toggle GridToggle;

        [SerializeField]
        private GameObject GridCm;

        [SerializeField]
        private Slider MeshVerticalSegmentsSlider;
        [SerializeField]
        private Slider MeshHorizontalSegmentsSlider;

        [SerializeField]
        private Button RestoreDefaultsButton;
        [SerializeField]
        private Button LoadButton;
        [SerializeField]
        private Button SaveButton;

        void Start()
        {
            _hmd = HmdParameters.Instance;
            _mesh = MeshParameters.Instance;

            UpdateControls();

            InterlensDistanceSlider.onValueChanged.AddListener((val) => _hmd.InterlensDistance = 0.001f * val);
            ScreenToLensDistSlider.onValueChanged.AddListener((val) => _hmd.ScreenToLensDist = 0.001f * val);
            EyeOffsetYSlider.onValueChanged.AddListener((val) => _hmd.EyeOffsetY = 0.001f * val);
            DistortionK1Slider.onValueChanged.AddListener((val) => _hmd.DistortionK1 = val);
            DistortionK2Slider.onValueChanged.AddListener((val) => _hmd.DistortionK2 = val);
            FovLeftSlider.onValueChanged.AddListener((val) => _hmd.MaxFovAngles = new Fov(val, _hmd.MaxFovAngles.Right, _hmd.MaxFovAngles.Bottom, _hmd.MaxFovAngles.Top));
            FovRightSlider.onValueChanged.AddListener((val) => _hmd.MaxFovAngles = new Fov(_hmd.MaxFovAngles.Left, val, _hmd.MaxFovAngles.Bottom, _hmd.MaxFovAngles.Top));
            FovTopSlider.onValueChanged.AddListener((val) => _hmd.MaxFovAngles = new Fov(_hmd.MaxFovAngles.Left, _hmd.MaxFovAngles.Right, _hmd.MaxFovAngles.Bottom, val));
            FovBottomSlider.onValueChanged.AddListener((val) => _hmd.MaxFovAngles = new Fov(_hmd.MaxFovAngles.Left, _hmd.MaxFovAngles.Right, val, _hmd.MaxFovAngles.Top));

            MeshHorizontalSegmentsSlider.onValueChanged.AddListener((val) => _mesh.SegmentWidth = (int)val);
            MeshVerticalSegmentsSlider.onValueChanged.AddListener((val) => _mesh.SegmentHeight = (int)val);

            GridToggle.onValueChanged.AddListener((val) => GridCm.SetActive(val));

            RestoreDefaultsButton.onClick.AddListener(RestoreDefaultSettings);
            LoadButton.onClick.AddListener(LoadSettings);
            SaveButton.onClick.AddListener(SaveSettings);
        }

        private void UpdateControls()
        {
            InterlensDistanceSlider.value = 1000f * _hmd.InterlensDistance;
            ScreenToLensDistSlider.value = 1000f * _hmd.ScreenToLensDist;
            EyeOffsetYSlider.value = 1000f * _hmd.EyeOffsetY;
            DistortionK1Slider.value = _hmd.DistortionK1;
            DistortionK2Slider.value = _hmd.DistortionK2;
            FovLeftSlider.value = _hmd.MaxFovAngles.Left;
            FovRightSlider.value = _hmd.MaxFovAngles.Right;
            FovTopSlider.value = _hmd.MaxFovAngles.Top;
            FovBottomSlider.value = _hmd.MaxFovAngles.Bottom;

            MeshHorizontalSegmentsSlider.value = _mesh.SegmentWidth;
            MeshVerticalSegmentsSlider.value = _mesh.SegmentHeight;

            GridToggle.isOn = false;
        }

        public void SaveSettings()
        {
            _hmd.SaveToPrefs();
            _mesh.SaveToPrefs();
            UpdateControls();
        }

        public void LoadSettings()
        {
            _hmd.LoadFromPrefs();
            _mesh.LoadFromPrefs();
            UpdateControls();
        }

        public void RestoreDefaultSettings()
        {
            _hmd.LoadDefaults();
            _mesh.LoadDefaults();
            UpdateControls();
        }
    }
}
