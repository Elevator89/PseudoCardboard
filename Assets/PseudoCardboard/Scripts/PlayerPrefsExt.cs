using UnityEngine;

namespace Assets.PseudoCardboard
{
    public static class PlayerPrefsExt
    {
        public static float SafeGetFloat(string key, float defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetFloat(key, defaultValue);
            }

            return PlayerPrefs.GetFloat(key);
        }

        public static int SafeGetInt(string key, int defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, defaultValue);
            }

            return PlayerPrefs.GetInt(key);
        }
    }
}