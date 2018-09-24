using UnityEngine;

namespace Assets.PseudoCardboard
{
    public class HmdParameters
    {
        private static HmdParameters _instance = null;

        private const string DistortionK1FieldName = "Hmd.DistortionK1";
        private const string DistortionK2FieldName = "Hmd.DistortionK2";
        private const string InterlensDistanceFieldName = "Hmd.InterlensDistance";
        private const string ScreenToLensDistFieldName = "Hmd.ScreenToLensDist";
        private const string EyeOffsetYFieldName = "Hmd.EyeOffsetY";

        private const string MaxFovAnglesLeftFieldName = "Hmd.MaxFovAngles.FovLeft";
        private const string MaxFovAnglesRightFieldName = "Hmd.MaxFovAngles.FovRight";
        private const string MaxFovAnglesTopFieldName = "Hmd.MaxFovAngles.FovTop";
        private const string MaxFovAnglesBottomFieldName = "Hmd.MaxFovAngles.FovBottom";

        private static readonly HmdParameters DefaultValues = new HmdParameters()
        {
            DistortionK1 = 0.51f,
            DistortionK2 = 0.16f,
            ScreenToLensDist = 0.045f,
            InterlensDistance = 0.065f,
            EyeOffsetY = 0.035f,
            MaxFovAngles = new Fov
            {
                Left = SafeGetFloat("FovLeft", 50),
                Right = SafeGetFloat("FovRight", 50),
                Top = SafeGetFloat("FovTop", 50),
                Bottom = SafeGetFloat("FovBottom", 50)
            }
        };

        public static HmdParameters Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HmdParameters();
                    _instance.LoadFromPrefs();
                }
                return _instance;
            }
        }

        public float DistortionK1;
        public float DistortionK2;
        public float InterlensDistance;
        public float ScreenToLensDist;
        public float EyeOffsetY;
        public Fov MaxFovAngles;

        private HmdParameters()
        { }

        public void SaveToPrefs()
        {
            PlayerPrefs.SetFloat(ScreenToLensDistFieldName, ScreenToLensDist);
            PlayerPrefs.SetFloat(DistortionK1FieldName, DistortionK1);
            PlayerPrefs.SetFloat(DistortionK2FieldName, DistortionK2);
            PlayerPrefs.SetFloat(InterlensDistanceFieldName, InterlensDistance);
            PlayerPrefs.SetFloat(EyeOffsetYFieldName, EyeOffsetY);

            PlayerPrefs.SetFloat(MaxFovAnglesLeftFieldName, MaxFovAngles.Left);
            PlayerPrefs.SetFloat(MaxFovAnglesRightFieldName, MaxFovAngles.Right);
            PlayerPrefs.SetFloat(MaxFovAnglesTopFieldName, MaxFovAngles.Top);
            PlayerPrefs.SetFloat(MaxFovAnglesBottomFieldName, MaxFovAngles.Bottom);
        }

        public void LoadFromPrefs()
        {
            ScreenToLensDist = SafeGetFloat(ScreenToLensDistFieldName, DefaultValues.ScreenToLensDist);
            DistortionK1 = SafeGetFloat(DistortionK1FieldName, DefaultValues.DistortionK1);
            DistortionK2 = SafeGetFloat(DistortionK2FieldName, DefaultValues.DistortionK2);
            InterlensDistance = SafeGetFloat(InterlensDistanceFieldName, DefaultValues.InterlensDistance);
            EyeOffsetY = SafeGetFloat(EyeOffsetYFieldName, DefaultValues.EyeOffsetY);
            MaxFovAngles = new Fov
            {
                Left = SafeGetFloat(MaxFovAnglesLeftFieldName, DefaultValues.MaxFovAngles.Left),
                Right = SafeGetFloat(MaxFovAnglesRightFieldName, DefaultValues.MaxFovAngles.Right),
                Top = SafeGetFloat(MaxFovAnglesTopFieldName, DefaultValues.MaxFovAngles.Top),
                Bottom = SafeGetFloat(MaxFovAnglesBottomFieldName, DefaultValues.MaxFovAngles.Bottom)
            };
        }

        public void LoadDefaults()
        {
            ScreenToLensDist = DefaultValues.ScreenToLensDist;
            DistortionK1 = DefaultValues.DistortionK1;
            DistortionK2 = DefaultValues.DistortionK2;
            InterlensDistance = DefaultValues.InterlensDistance;
            EyeOffsetY = DefaultValues.EyeOffsetY;
            MaxFovAngles = DefaultValues.MaxFovAngles;
        }

        private static float SafeGetFloat(string key, float defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetFloat(key, defaultValue);
            }

            return PlayerPrefs.GetFloat(key);
        }
    }
}
