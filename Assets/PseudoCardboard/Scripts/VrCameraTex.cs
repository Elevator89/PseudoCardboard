using System.Linq;
using UnityEngine;

namespace Assets.PseudoCardboard
{
    [ExecuteInEditMode]
    public class VrCameraTex : MonoBehaviour
    {
        public DisplayParameters Display;
        public HmdParameters Hmd;
        public Material EyeMaterial;
        public RenderTexture RenderTexture;

        private Camera _leftWorldCam;
        private Camera _rightWorldCam;

        private Distortion _distortion;

        void OnEnable()
        {
            _leftWorldCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Left);
            _rightWorldCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Right);

            _leftWorldCam.transform.localRotation = Quaternion.identity;
            _rightWorldCam.transform.localRotation = Quaternion.identity;

            _distortion = new Distortion(Hmd.DistortionK1, Hmd.DistortionK2);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _distortion.DistortionK1 = Hmd.DistortionK1;
            _distortion.DistortionK2 = Hmd.DistortionK2;

            float zNear = _leftWorldCam.nearClipPlane;
            float zFar = _leftWorldCam.farClipPlane;

            Matrix4x4 projWorldLeft;
            Matrix4x4 projWorldRight;

            // То, как должен видеть левый глаз. Мнимое изображение (после преломления идеальной линзой без искажений). С широким углом. Именно так надо снять сцену
            FovAngles fovWorldLeft = GetWorldFovLeft(Display, Hmd);
            ComposeProjectionMatrices(fovWorldLeft, zNear, zFar, out projWorldLeft, out projWorldRight);


            // То, как левый глаз видит свою половину экрана телефона без линз.
            Rect viewportEyeLeft;
            Matrix4x4 projEyeLeft;
            Matrix4x4 projEyeRight;
            FovAngles fovEyeLeft = GetEyeFovAndViewportLeft(Display, Hmd, out viewportEyeLeft);

            Debug.LogFormat("Viewport: x={0:0.00}; y={1:0.00}; w={2:0.00}; h={3:0.00}", viewportEyeLeft.x, viewportEyeLeft.y, viewportEyeLeft.width, viewportEyeLeft.height);
            Debug.LogFormat("FovWorld: l={0:0.00}; r={1:0.00}; t={2:0.00}; b={3:0.00}", fovWorldLeft.Left, fovWorldLeft.Right, fovWorldLeft.Top, fovWorldLeft.Bottom);
            Debug.LogFormat("FovEye: l={0:0.00}; r={1:0.00}; t={2:0.00}; b={3:0.00}", fovEyeLeft.Left, fovEyeLeft.Right, fovEyeLeft.Top, fovEyeLeft.Bottom);

            ComposeProjectionMatrices(fovEyeLeft, zNear, zFar, out projEyeLeft, out projEyeRight);

            _leftWorldCam.transform.localPosition = 0.5f * Vector3.left * Hmd.InterlensDistance;
            _leftWorldCam.transform.localPosition = 0.5f * Vector3.left * Hmd.InterlensDistance;

            _rightWorldCam.transform.localPosition = 0.5f * Vector3.right * Hmd.InterlensDistance;
            _rightWorldCam.transform.localPosition = 0.5f * Vector3.right * Hmd.InterlensDistance;

            _leftWorldCam.projectionMatrix = projWorldLeft;
            _rightWorldCam.projectionMatrix = projWorldRight;

            UpdateBarrelDistortion(EyeMaterial, viewportEyeLeft, projWorldLeft, projEyeLeft);
            Graphics.Blit(RenderTexture, destination, EyeMaterial);
        }

        // Set barrel_distortion parameters given CardboardView.
        private void UpdateBarrelDistortion(Material distortionShader, Rect viewportEyeLeft, Matrix4x4 projWorldLeft, Matrix4x4 projEyeLeft)
        {
            // Shader params include parts of the projection matrices needed to
            // convert texture coordinates between distorted and undistorted
            // frustums.  The projections are adjusted to include transform between
            // texture space [0..1] and NDC [-1..1] as well as accounting for the
            // viewport on the screen.
            // TODO: have explicit viewport transform in shader for simplicity

            distortionShader.SetFloat("_DistortionK1", Hmd.DistortionK1);
            distortionShader.SetFloat("_DistortionK2", Hmd.DistortionK2);

            Vector4 projWorldLine =
                new Vector4(
                    projWorldLeft[0, 0],
                    projWorldLeft[1, 1],
                    projWorldLeft[0, 2] - 1,
                    projWorldLeft[1, 2] - 1) / 2.0f;

            var x_scale = viewportEyeLeft.width / (0.5f * Display.Resolution.x);
            var y_scale = viewportEyeLeft.height / Display.Resolution.y;
            var x_trans = 2 * (viewportEyeLeft.x + 0.5f * viewportEyeLeft.width) / (0.5f * Display.Resolution.x) - 1;
            var y_trans = 2 * (viewportEyeLeft.y + 0.5f * viewportEyeLeft.height) / Display.Resolution.y - 1;

            Vector4 projEyeLine =
                new Vector4(
                    projEyeLeft[0, 0] * x_scale,
                    projEyeLeft[1, 1] * y_scale,
                    projEyeLeft[0, 2] - 1 - x_trans,
                    projEyeLeft[1, 2] - 1 - y_trans) / 2.0f;

            distortionShader.SetVector("_ProjectionWorldLeft", projWorldLine);
            distortionShader.SetVector("_ProjectionEyeLeft", projEyeLine);
        }

        /// То, как должен видеть левый глаз. Мнимое изображение (после преломления идеальной линзой без искажений). С широким углом
        private FovAngles GetWorldFovLeft(DisplayParameters display, HmdParameters hmd)
        {
            // The screen-to-lens distance can be used as a rough approximation
            // of the virtual-eye-to-screen distance.
            float eyeToScreenDist = hmd.ScreenToLensDist;

            float outerDist = 0.5f * (display.Size.x - hmd.InterlensDistance);
            float innerDist = 0.5f * hmd.InterlensDistance;
            float bottomDist = hmd.EyeOffsetY;
            float topDist = display.Size.y - bottomDist;

            float outerAngle = Mathf.Rad2Deg * Mathf.Atan(_distortion.DistortTanAngle(outerDist / eyeToScreenDist));
            float innerAngle = Mathf.Rad2Deg * Mathf.Atan(_distortion.DistortTanAngle(innerDist / eyeToScreenDist));
            float bottomAngle = Mathf.Rad2Deg * Mathf.Atan(_distortion.DistortTanAngle(bottomDist / eyeToScreenDist));
            float topAngle = Mathf.Rad2Deg * Mathf.Atan(_distortion.DistortTanAngle(topDist / eyeToScreenDist));

            return new FovAngles
            {
                Left = outerAngle,
                Right = innerAngle,
                Bottom = bottomAngle,
                Top = topAngle,
            };
        }

        /// То, как левый глаз видит свою половину экрана телефона без линз.
        private FovAngles GetEyeFovAndViewportLeft(DisplayParameters display, HmdParameters hmd, out Rect viewport)
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

        private void ComposeProjectionMatrices(FovAngles leftFovAngles, float near, float far, out Matrix4x4 left, out Matrix4x4 right)
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
