using UnityEngine;

namespace Assets.PseudoCardboard
{
    public static class Calculator
    {
        /// То, как должен видеть левый глаз. Мнимое изображение (после преломления идеальной линзой без искажений). С широким углом
        public static Fov GetWorldFovLeft(Distortion distortion, DisplayParameters display, HmdParameters hmd)
        {
            return Fov.TanAnglesToAngles(GetWorldFovTanAnglesLeft(distortion, display, hmd));
        }

        public static Fov GetWorldFovTanAnglesLeft(Distortion distortion, DisplayParameters display, HmdParameters hmd)
        {
            return DistortTanAngles(GetEyeFovTanAnglesLeft(display, hmd), distortion);
        }

        public static Fov GetEyeFovAndViewportLeft(DisplayParameters display, HmdParameters hmd, out Rect leftViewport)
        {
            // The screen-to-lens distance can be used as a rough approximation
            // of the virtual-eye-to-screen distance.
            float eyeToScreenDist = hmd.ScreenToLensDist;

            Fov fovDistances = GetFovDistancesLeft(display, hmd);
            leftViewport = GetViewportLeft(fovDistances, display.Dpm);

            return Fov.TanAnglesToAngles(fovDistances / eyeToScreenDist);
        }

        public static Fov GetEyeFovTanAnglesAndViewportLeft(DisplayParameters display, HmdParameters hmd, out Rect leftViewport)
        {
            // The screen-to-lens distance can be used as a rough approximation
            // of the virtual-eye-to-screen distance.
            float eyeToScreenDist = hmd.ScreenToLensDist;

            Fov fovDistances = GetFovDistancesLeft(display, hmd);
            leftViewport = GetViewportLeft(fovDistances, display.Dpm);

            return fovDistances / eyeToScreenDist;
        }

        /// То, как левый глаз видит свою половину экрана телефона без линз.
        public static Fov GetEyeFovLeft(DisplayParameters display, HmdParameters hmd)
        {
            return Fov.TanAnglesToAngles(GetEyeFovTanAnglesLeft(display, hmd));
        }

        public static Fov GetEyeFovTanAnglesLeft(DisplayParameters display, HmdParameters hmd)
        {
            // The screen-to-lens distance can be used as a rough approximation
            // of the virtual-eye-to-screen distance.
            float eyeToScreenDist = hmd.ScreenToLensDist;

            Fov fovDistances = GetFovDistancesLeft(display, hmd);
            return fovDistances / eyeToScreenDist;
        }

        public static Rect GetViewportLeft(Fov fovDistances, float dpm)
        {
            return new Rect(
                0,
                0,
                (fovDistances.Left + fovDistances.Right) * dpm,
                (fovDistances.Bottom + fovDistances.Top) * dpm);
        }

        private static Fov GetFovDistancesLeft(DisplayParameters display, HmdParameters hmd)
        {
            float outerDist = 0.5f * (display.Size.x - hmd.InterlensDistance);
            float innerDist = 0.5f * hmd.InterlensDistance;
            float bottomDist = hmd.EyeOffsetY;
            float topDist = display.Size.y - bottomDist;

            return new Fov(outerDist, innerDist, bottomDist, topDist);
        }

        private static Fov DistortTanAngles(Fov tanAngles, Distortion distortion)
        {
            return new Fov
            (
                distortion.DistortTanAngle(tanAngles.Left),
                distortion.DistortTanAngle(tanAngles.Right),
                distortion.DistortTanAngle(tanAngles.Bottom),
                distortion.DistortTanAngle(tanAngles.Top)
            );
        }

        public static void ComposeProjectionMatricesFromFovAngles(Fov leftFovAngles, float near, float far, out Matrix4x4 left, out Matrix4x4 right)
        {
            Fov leftFovTanAngles = (leftFovAngles * Mathf.Deg2Rad).GetTan();

            ComposeProjectionMatricesFromFovTanAngles(leftFovTanAngles, near, far, out left, out right);
        }

        public static void ComposeProjectionMatricesFromFovTanAngles(Fov leftFovTanAngles, float near, float far, out Matrix4x4 left, out Matrix4x4 right)
        {
            Fov nearProjection = leftFovTanAngles * near;

            left = Matrix4x4Ext.CreateFrustum(-nearProjection.Left, nearProjection.Right, -nearProjection.Bottom, nearProjection.Top, near, far);
            right = Matrix4x4Ext.CreateFrustum(-nearProjection.Right, nearProjection.Left, -nearProjection.Bottom, nearProjection.Top, near, far);
        }

    }
}
