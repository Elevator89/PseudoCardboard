/* 
 * Copyright 2018 Andrey Lemin
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

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
