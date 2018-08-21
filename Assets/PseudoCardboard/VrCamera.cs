using System.Linq;
using UnityEngine;

namespace Assets.PseudoCardboard
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class VrCamera : MonoBehaviour
    {
        const float MetersPerInch = 0.0254f;

        public HmdParameters HmdParameters;
        public Material EyeMaterial;
        public RenderTexture RenderTexture;

        private Camera _centralCam;
        private Camera _leftEyeCam;
        private Camera _rightEyeCam;

        private Distortion _distortion;

        void OnEnable()
        {
            _centralCam = GetComponent<Camera>();

            _leftEyeCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Left);
            _rightEyeCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Right);

            _leftEyeCam.transform.localRotation = Quaternion.identity;
            _rightEyeCam.transform.localRotation = Quaternion.identity;

            _distortion = new Distortion(HmdParameters.DistortionK1, HmdParameters.DistortionK2);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            float screenWidthMeters = Display.main.renderingWidth / Screen.dpi * MetersPerInch;
            float screenHeightMeters = Display.main.renderingHeight / Screen.dpi * MetersPerInch;

            //Debug.LogFormat("{0}x{1}", screenWidthMeters, screenHeightMeters);

            _distortion.DistortionK1 = HmdParameters.DistortionK1;
            _distortion.DistortionK2 = HmdParameters.DistortionK2;

            Matrix4x4 projLeft;
            Matrix4x4 projRight;

            FovAngles projFov = GetLeftEyeFov(screenWidthMeters, screenHeightMeters);
            ComposeProjectionMatrices(projFov, _leftEyeCam.nearClipPlane, _leftEyeCam.farClipPlane, out projLeft, out projRight);

            Rect viewportNoLensLeft;
            Matrix4x4 projNoLensLeft;
            Matrix4x4 projNoLensRight;
            FovAngles noLensFov = GetLeftEyeFovAndViewportNoDistortionCorrection(screenWidthMeters, screenHeightMeters, out viewportNoLensLeft);

            Debug.LogFormat("Viewport: x={0:0.00}; y={1:0.00}; w={2:0.00}; h={3:0.00}", viewportNoLensLeft.x, viewportNoLensLeft.y, viewportNoLensLeft.width, viewportNoLensLeft.height);
            Debug.LogFormat("FOV: l={0:0.00}; r={1:0.00}; t={2:0.00}; b={3:0.00}", projFov.Left, projFov.Right, projFov.Top, projFov.Bottom);

            ComposeProjectionMatrices(noLensFov, _leftEyeCam.nearClipPlane, _leftEyeCam.farClipPlane, out projNoLensLeft, out projNoLensRight);

            _leftEyeCam.transform.localPosition = 0.5f * Vector3.left * HmdParameters.InterlensDistance;
            _rightEyeCam.transform.localPosition = 0.5f * Vector3.right * HmdParameters.InterlensDistance;

            _leftEyeCam.projectionMatrix = projLeft;
            _rightEyeCam.projectionMatrix = projRight;

            UpdateBarrelDistortion(EyeMaterial, viewportNoLensLeft, projLeft, projNoLensLeft);
            Graphics.Blit(RenderTexture, destination, EyeMaterial);
        }

        // Set barrel_distortion parameters given CardboardView.
        private void UpdateBarrelDistortion(Material distortionTex, Rect viewport, Matrix4x4 projLeft, Matrix4x4 projNoLensLeft)
        {
            // Shader params include parts of the projection matrices needed to
            // convert texture coordinates between distorted and undistorted
            // frustums.  The projections are adjusted to include transform between
            // texture space [0..1] and NDC [-1..1] as well as accounting for the
            // viewport on the screen.
            // TODO: have explicit viewport transform in shader for simplicity

            distortionTex.SetFloat("_DistortionK1", HmdParameters.DistortionK1);
            distortionTex.SetFloat("_DistortionK2", HmdParameters.DistortionK2);

            Vector4 leftProjLine =
                new Vector4(
                    projLeft[0, 0],
                    projLeft[1, 1],
                    projLeft[0, 2] - 1,
                    projLeft[1, 2] - 1) / 2.0f;

            var x_scale = viewport.width / (0.5f * Display.main.renderingWidth);
            var y_scale = viewport.height / Display.main.renderingHeight;
            //var x_trans = 2 * (viewport.x + 0.5f * viewport.width) / (0.5f * Display.main.renderingWidth) - 1;
            var x_trans = 0.0f;
            var y_trans = 2 * (viewport.y + 0.5f * viewport.height) / Display.main.renderingHeight - 1;

            Vector4 leftUnprojLine =
                new Vector4(
                    projNoLensLeft[0, 0] * x_scale,
                    projNoLensLeft[1, 1] * y_scale,
                    projNoLensLeft[0, 2] - 1 - x_trans,
                    projNoLensLeft[1, 2] - 1 - y_trans) / 2.0f;

            distortionTex.SetVector("_ProjectionLeft", leftProjLine);
            distortionTex.SetVector("_UnprojectionLeft", leftUnprojLine);
        }

        private FovAngles GetLeftEyeFov(float screenWidthMeters, float screenHeightMeters)
        {
            // The screen-to-lens distance can be used as a rough approximation
            // of the virtual-eye-to-screen distance.
            float eyeToScreenDist = HmdParameters.ScreenToLensDist;

            float outerDist = 0.5f * (screenWidthMeters - HmdParameters.InterlensDistance);
            float innerDist = 0.5f * HmdParameters.InterlensDistance;
            float bottomDist = HmdParameters.EyeOffsetY;
            float topDist = screenHeightMeters - bottomDist;

            float outerAngle = Mathf.Rad2Deg * Mathf.Atan(_distortion.DistortTanAngle(outerDist / eyeToScreenDist));
            float innerAngle = Mathf.Rad2Deg * Mathf.Atan(_distortion.DistortTanAngle(innerDist / eyeToScreenDist));
            float bottomAngle = Mathf.Rad2Deg * Mathf.Atan(_distortion.DistortTanAngle(bottomDist / eyeToScreenDist));
            float topAngle = Mathf.Rad2Deg * Mathf.Atan(_distortion.DistortTanAngle(topDist / eyeToScreenDist));

            return new FovAngles
            {
                Left = Mathf.Min(outerAngle, HmdParameters.MaxFovAngles.Left),
                Right = Mathf.Min(innerAngle, HmdParameters.MaxFovAngles.Right),
                Bottom = Mathf.Min(bottomAngle, HmdParameters.MaxFovAngles.Bottom),
                Top = Mathf.Min(topAngle, HmdParameters.MaxFovAngles.Top),
            };
        }

        private FovAngles GetLeftEyeFovAndViewportNoDistortionCorrection(float screenWidthMeters, float screenHeightMeters, out Rect viewport)
        {
            // The screen-to-lens distance can be used as a rough approximation
            // of the virtual-eye-to-screen distance.
            float eyeToScreenDist = HmdParameters.ScreenToLensDist;
            float halfLensDistance = 0.5f * HmdParameters.InterlensDistance;
            float xPxPerMeter = Display.main.renderingWidth / screenWidthMeters;
            float yPxPerMeter = Display.main.renderingHeight / screenHeightMeters;

            float eyePosX = 0.5f * screenWidthMeters - halfLensDistance;
            float eyePosY = HmdParameters.EyeOffsetY;

            float outerDist = eyePosX;
            float innerDist = halfLensDistance;
            float bottomDist = eyePosY;
            float topDist = Display.main.renderingHeight - eyePosY;

            float outerDistTan = outerDist / eyeToScreenDist;
            float innerDistTan = innerDist / eyeToScreenDist;
            float bottomDistTan = bottomDist / eyeToScreenDist;
            float topDistTan = topDist / eyeToScreenDist;

            outerDistTan = Mathf.Min(outerDistTan, _distortion.UndistortTanAngle(Mathf.Tan(Mathf.Deg2Rad * HmdParameters.MaxFovAngles.Left)));
            innerDistTan = Mathf.Min(innerDistTan, _distortion.UndistortTanAngle(Mathf.Tan(Mathf.Deg2Rad * HmdParameters.MaxFovAngles.Right)));
            bottomDistTan = Mathf.Min(bottomDistTan, _distortion.UndistortTanAngle(Mathf.Tan(Mathf.Deg2Rad * HmdParameters.MaxFovAngles.Bottom)));
            topDistTan = Mathf.Min(topDistTan, _distortion.UndistortTanAngle(Mathf.Tan(Mathf.Deg2Rad * HmdParameters.MaxFovAngles.Top)));

            outerDist = outerDistTan * eyeToScreenDist;
            innerDist = innerDistTan * eyeToScreenDist;
            bottomDist = bottomDistTan * eyeToScreenDist;
            topDist = topDistTan * eyeToScreenDist;

            float x = (eyePosX - outerDist) * xPxPerMeter;
            float y = (eyePosY - bottomDist) * yPxPerMeter;
            float w = (eyePosX + innerDist) * xPxPerMeter - x;
            float h = (eyePosY + topDist) * yPxPerMeter - y;

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
