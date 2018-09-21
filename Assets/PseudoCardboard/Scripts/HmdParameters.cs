using System;
using UnityEngine;

namespace Assets.PseudoCardboard
{
    public class HmdParameters
    {
        public static HmdParameters Instance { get { return new HmdParameters(); } }

        public float DistortionK1
        {
            get { return SafeGetFloat("DistortionK1", 0.51f); }
            set { PlayerPrefs.SetFloat("DistortionK1", value); }
        }

        public float DistortionK2
        {
            get { return SafeGetFloat("DistortionK2", 0.16f); }
            set { PlayerPrefs.SetFloat("DistortionK2", value); }
        }

        public float InterlensDistance
        {
            get { return SafeGetFloat("InterlensDistance", 0.065f); }
            set { PlayerPrefs.SetFloat("InterlensDistance", value); }
        }

        public float ScreenToLensDist
        {
            get { return SafeGetFloat("ScreenToLensDist", 0.045f); }
            set { PlayerPrefs.SetFloat("ScreenToLensDist", value); }
        }

        public float EyeOffsetY
        {
            get { return SafeGetFloat("EyeOffsetY", 0.035f); }
            set { PlayerPrefs.SetFloat("EyeOffsetY", value); }
        }

        public Fov MaxFovAngles
        {
            get
            {
                return new Fov
                {
                    Left = SafeGetFloat("FovLeft", 50),
                    Right = SafeGetFloat("FovRight", 50),
                    Top = SafeGetFloat("FovTop", 50),
                    Bottom = SafeGetFloat("FovBottom", 50)
                };
            }

            set
            {
                PlayerPrefs.SetFloat("FovLeft", value.Left);
                PlayerPrefs.SetFloat("FovRight", value.Right);
                PlayerPrefs.SetFloat("FovTop", value.Top);
                PlayerPrefs.SetFloat("FovBottom", value.Bottom);
            }
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
