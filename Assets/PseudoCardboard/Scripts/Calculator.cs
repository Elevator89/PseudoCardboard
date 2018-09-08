using UnityEngine;

namespace Assets.PseudoCardboard
{
    public static class Calculator
    {
        /// То, как должен видеть левый глаз. Мнимое изображение (после преломления идеальной линзой без искажений). С широким углом
        public static FovAngles GetWorldFovLeft(Distortion distortion, DisplayParameters display, HmdParameters hmd)
        {
            // The screen-to-lens distance can be used as a rough approximation
            // of the virtual-eye-to-screen distance.
            float eyeToScreenDist = hmd.ScreenToLensDist;

            float outerDist = 0.5f * (display.Size.x - hmd.InterlensDistance);
            float innerDist = 0.5f * hmd.InterlensDistance;
            float bottomDist = hmd.EyeOffsetY;
            float topDist = display.Size.y - bottomDist;

            float outerAngle = Mathf.Rad2Deg * Mathf.Atan(distortion.DistortTanAngle(outerDist / eyeToScreenDist));
            float innerAngle = Mathf.Rad2Deg * Mathf.Atan(distortion.DistortTanAngle(innerDist / eyeToScreenDist));
            float bottomAngle = Mathf.Rad2Deg * Mathf.Atan(distortion.DistortTanAngle(bottomDist / eyeToScreenDist));
            float topAngle = Mathf.Rad2Deg * Mathf.Atan(distortion.DistortTanAngle(topDist / eyeToScreenDist));

            return new FovAngles
            {
                Left = outerAngle,
                Right = innerAngle,
                Bottom = bottomAngle,
                Top = topAngle,
            };
        }

        /// То, как левый глаз видит свою половину экрана телефона без линз.
        public static FovAngles GetEyeFovAndViewportLeft(DisplayParameters display, HmdParameters hmd, out Rect viewport)
        {
            // The screen-to-lens distance can be used as a rough approximation
            // of the virtual-eye-to-screen distance.
            float eyeToScreenDist = hmd.ScreenToLensDist;
            float halfLensDistance = 0.5f * hmd.InterlensDistance;

            float eyePosX = 0.5f * display.Size.x - halfLensDistance;
            float eyePosY = hmd.EyeOffsetY;

            float outerDist = eyePosX;
            float innerDist = halfLensDistance;
            float bottomDist = eyePosY;
            float topDist = display.Size.y - eyePosY;

            float outerDistTan = outerDist / eyeToScreenDist;
            float innerDistTan = innerDist / eyeToScreenDist;
            float bottomDistTan = bottomDist / eyeToScreenDist;
            float topDistTan = topDist / eyeToScreenDist;

            float x = 0;
            float y = 0;
            float w = (eyePosX + innerDist) * display.Dpm;
            float h = (eyePosY + topDist) * display.Dpm;

            viewport = new Rect(x, y, w, h);

            return new FovAngles
            {
                Left = Mathf.Rad2Deg * Mathf.Atan(outerDistTan),
                Right = Mathf.Rad2Deg * Mathf.Atan(innerDistTan),
                Bottom = Mathf.Rad2Deg * Mathf.Atan(bottomDistTan),
                Top = Mathf.Rad2Deg * Mathf.Atan(topDistTan),
            };
        }

        public static void ComposeProjectionMatrices(FovAngles leftFovAngles, float near, float far, out Matrix4x4 left, out Matrix4x4 right)
        {
            float outer = Mathf.Tan(leftFovAngles.Left * Mathf.Deg2Rad) * near;
            float inner = Mathf.Tan(leftFovAngles.Right * Mathf.Deg2Rad) * near;
            float bottom = Mathf.Tan(leftFovAngles.Bottom * Mathf.Deg2Rad) * near;
            float top = Mathf.Tan(leftFovAngles.Top * Mathf.Deg2Rad) * near;

            left = Matrix4x4Ext.CreateFrustum(-outer, inner, -bottom, top, near, far);
            right = Matrix4x4Ext.CreateFrustum(-inner, outer, -bottom, top, near, far);
        }

    }
}
