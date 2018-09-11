﻿using UnityEngine;

namespace Assets.PseudoCardboard.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class VrCameraBiMesh : MonoBehaviour
    {
        public Material EyeMaterial;

        [SerializeField]
        private Camera _camWorldLeft;
        private FovScaler _camWorldLeftScaler;

        [SerializeField]
        private Camera _camWorldRight;
        private FovScaler _camWorldRightScaler;


        [SerializeField]
        private Camera _camEyeLeft;

        [SerializeField]
        private Camera _camEyeRight;

        private Distortion _distortion;
        private DisplayParameters Display;
        private HmdParameters Hmd;

        void OnEnable()
        {
            Hmd = FindObjectOfType<HmdParamRoot>().HmdParameters;
            Display = new DisplayParameters();

            _camWorldLeft.transform.localRotation = Quaternion.identity;
            _camWorldRight.transform.localRotation = Quaternion.identity;

            _camWorldLeftScaler = _camWorldLeft.GetComponentInChildren<FovScaler>();
            _camWorldRightScaler = _camWorldRight.GetComponentInChildren<FovScaler>();

            _camEyeLeft.transform.localRotation = Quaternion.identity;
            _camEyeRight.transform.localRotation = Quaternion.identity;

            _distortion = new Distortion(Hmd.DistortionK1, Hmd.DistortionK2);
        }

        void Update()
        {
            //Debug.LogFormat("{0}x{1}", screenWidthMeters, screenHeightMeters);

            _distortion.DistortionK1 = Hmd.DistortionK1;
            _distortion.DistortionK2 = Hmd.DistortionK2;

            float zNear = _camWorldLeft.nearClipPlane;
            float zFar = _camWorldLeft.farClipPlane;

            Matrix4x4 projWorldLeft;
            Matrix4x4 projWorldRight;

            // То, как должен видеть левый глаз. Мнимое изображение (после увеличения идеальной линзой без искажений). С широким углом. Именно так надо снять сцену
            Fov fovWorldTanAnglesLeft = Calculator.GetWorldFovTanAnglesLeft(_distortion, Display, Hmd);
            Calculator.ComposeProjectionMatricesFromFovTanAngles(fovWorldTanAnglesLeft, zNear, zFar, out projWorldLeft, out projWorldRight);


            // То, как левый глаз видит свою половину экрана телефона без линз.
            Rect viewportEyeLeft;
            Matrix4x4 projEyeLeft;
            Matrix4x4 projEyeRight;
            Fov fovEyeTanAnglesLeft = Calculator.GetEyeFovTanAnglesAndViewportLeft(Display, Hmd, out viewportEyeLeft);

            //Debug.LogFormat("Viewport: x={0:0.00}; y={1:0.00}; w={2:0.00}; h={3:0.00}", viewportNoLensLeft.x, viewportNoLensLeft.y, viewportNoLensLeft.width, viewportNoLensLeft.height);
            //Debug.LogFormat("FOV: l={0:0.00}; r={1:0.00}; t={2:0.00}; b={3:0.00}", projFov.Left, projFov.Right, projFov.Top, projFov.Bottom);

            Calculator.ComposeProjectionMatricesFromFovTanAngles(fovEyeTanAnglesLeft, zNear, zFar, out projEyeLeft, out projEyeRight);

            _camWorldLeft.transform.localPosition = 0.5f * Vector3.left * Hmd.InterlensDistance;
            _camWorldRight.transform.localPosition = 0.5f * Vector3.right * Hmd.InterlensDistance;

            _camEyeLeft.transform.localPosition = 0.5f * Vector3.left * Hmd.InterlensDistance;
            _camEyeRight.transform.localPosition = 0.5f * Vector3.right * Hmd.InterlensDistance;

            _camWorldLeft.projectionMatrix = projWorldLeft;
            _camWorldRight.projectionMatrix = projWorldRight;

            _camWorldLeftScaler.SetFov(fovWorldTanAnglesLeft);
            _camWorldRightScaler.SetFov(fovWorldTanAnglesLeft.GetFlippedHorizontally());

            _camEyeLeft.projectionMatrix = projEyeLeft;
            _camEyeRight.projectionMatrix = projEyeRight;

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
