using System;
using UnityEngine;

namespace Assets.PseudoCardboard
{
    [Serializable]
    public class DisplayParameters
    {
        const float MetersPerInch = 0.0254f;

        public Vector2 Resolution
        {
            get { return new Vector2(Display.main.renderingWidth, Display.main.renderingHeight); }
        }

        public Vector2 Dpi
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics");
                activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);

                return new Vector2(metrics.Get<float>("xdpi"), metrics.Get<float>("ydpi"));
#else
                return new Vector2(Screen.dpi, Screen.dpi);
#endif
            }
        }

        public Vector2 Dpm
        {
            get { return Dpi / MetersPerInch; }
        }

        public Vector2 Size
        {
            get
            {
                return new Vector2(
                    Display.main.renderingWidth / Dpm.x,
                    Display.main.renderingHeight / Dpm.y);
            }
        }
    }
}
