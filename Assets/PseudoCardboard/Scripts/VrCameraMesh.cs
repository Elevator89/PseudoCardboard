using System.Linq;
using UnityEngine;

namespace Assets.PseudoCardboard
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class VrCameraMesh : MonoBehaviour
    {
        public DisplayParameters Display;
        public HmdParameters Hmd;
        public Material EyeMaterial;

        private Camera _centralCam;
        private Camera _leftWorldCam;
        private Camera _rightWorldCam;

        private Distortion _distortion;

        void OnEnable()
        {
            _centralCam = GetComponent<Camera>();

            _leftWorldCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Left);
            _rightWorldCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Right);

            _leftWorldCam.transform.localRotation = Quaternion.identity;
            _rightWorldCam.transform.localRotation = Quaternion.identity;

            _distortion = new Distortion(Hmd.DistortionK1, Hmd.DistortionK2);
        }

        void Update()
        {
            //Debug.LogFormat("{0}x{1}", screenWidthMeters, screenHeightMeters);

            _distortion.DistortionK1 = Hmd.DistortionK1;
            _distortion.DistortionK2 = Hmd.DistortionK2;

            float zNear = _leftWorldCam.nearClipPlane;
            float zFar = _leftWorldCam.farClipPlane;

            Matrix4x4 projWorldLeft;
            Matrix4x4 projWorldRight;

            // То, как должен видеть левый глаз. Мнимое изображение (после увеличения идеальной линзой без искажений). С широким углом. Именно так надо снять сцену
            FovAngles fovWorldLeft = Calculator.GetWorldFovLeft(_distortion, Display, Hmd);
            Calculator.ComposeProjectionMatrices(fovWorldLeft, zNear, zFar, out projWorldLeft, out projWorldRight);


            // То, как левый глаз видит свою половину экрана телефона без линз.
            Rect viewportEyeLeft;
            Matrix4x4 projEyeLeft;
            Matrix4x4 projEyeRight;
            FovAngles fovEyeLeft = Calculator.GetEyeFovAndViewportLeft(Display, Hmd, out viewportEyeLeft);

            //Debug.LogFormat("Viewport: x={0:0.00}; y={1:0.00}; w={2:0.00}; h={3:0.00}", viewportNoLensLeft.x, viewportNoLensLeft.y, viewportNoLensLeft.width, viewportNoLensLeft.height);
            //Debug.LogFormat("FOV: l={0:0.00}; r={1:0.00}; t={2:0.00}; b={3:0.00}", projFov.Left, projFov.Right, projFov.Top, projFov.Bottom);

            Calculator.ComposeProjectionMatrices(fovEyeLeft, zNear, zFar, out projEyeLeft, out projEyeRight);

            _leftWorldCam.transform.localPosition = 0.5f * Vector3.left * Hmd.InterlensDistance;
            _rightWorldCam.transform.localPosition = 0.5f * Vector3.right * Hmd.InterlensDistance;

            _leftWorldCam.projectionMatrix = projWorldLeft;
            _rightWorldCam.projectionMatrix = projWorldRight;

            //_leftEyeCam.transform.localPosition = 0.5f * Vector3.left * Hmd.InterlensDistance;
            //_rightEyeCam.transform.localPosition = 0.5f * Vector3.right * Hmd.InterlensDistance;

            //_leftEyeCam.projectionMatrix = projWorldLeft;
            //_rightEyeCam.projectionMatrix = projWorldRight;

            UpdateBarrelDistortion(EyeMaterial, viewportEyeLeft, projWorldLeft, projEyeLeft);
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
    }
}
