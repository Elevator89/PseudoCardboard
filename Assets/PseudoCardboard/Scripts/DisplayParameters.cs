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

        public float Dpi
        {
            get { return Screen.dpi; }
        }

        public float Dpm
        {
            get { return Screen.dpi / MetersPerInch; }
        }

        public Vector2 Size
        {
            get
            {
                return new Vector2(
                    Display.main.renderingWidth / Dpm,
                    Display.main.renderingHeight / Dpm);
            }
        }
    }
}
