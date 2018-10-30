using System.Linq;
using UnityEngine;

namespace Assets.PseudoCardboard
{
    /// Для VR-представления применяется фрагментный шейдер. Так работает генератора профиля Google Cardboard. Наиболее точная и медленная реализация
    [ExecuteInEditMode]
    public class VrCameraTex : MonoBehaviour
    {
        public Material EyeMaterial;
        public RenderTexture RenderTexture;

        private Camera _leftWorldCam;
        private Camera _rightWorldCam;

        private Distortion _distortion;
        private DisplayParameters Display;
        private HmdParameters Hmd;

        void OnEnable()
        {
            Hmd = HmdParameters.Instance;
            Display = new DisplayParameters();

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

			Fov displayDistancesLeft = Calculator.GetFovDistancesLeft(Display, Hmd);
			Rect displayViewportLeft = Calculator.GetViewportLeft(displayDistancesLeft, Display.Dpm);

			// То, как должен видеть левый глаз свой кусок экрана. Без линзы. C учётом только размеров дисплея
			Fov fovDisplayTanAngles = displayDistancesLeft / Hmd.ScreenToLensDist;

			// FoV шлема
			Fov hmdMaxFovTanAngles = Fov.AnglesToTanAngles(Hmd.MaxFovAngles);

			// То, как должен видеть левый глаз свой кусок экрана. Без линзы. C учётом размеров дисплея и FoV шлема
			Fov fovEyeTanAglesLeft = Fov.Min(fovDisplayTanAngles, hmdMaxFovTanAngles);

			// То, как должен видеть левый глаз. Мнимое изображение (после увеличения идеальной линзой без искажений). С широким углом. Именно так надо снять сцену
			Fov fovWorldTanAnglesLeft = Calculator.DistortTanAngles(fovEyeTanAglesLeft, _distortion);

			Matrix4x4 projWorldLeft;
			Matrix4x4 projWorldRight;
			Calculator.ComposeProjectionMatricesFromFovTanAngles(fovWorldTanAnglesLeft, zNear, zFar, out projWorldLeft, out projWorldRight);

			Matrix4x4 projEyeLeft;
			Matrix4x4 projEyeRight;
			Calculator.ComposeProjectionMatricesFromFovTanAngles(fovDisplayTanAngles, zNear, zFar, out projEyeLeft, out projEyeRight);

			_leftWorldCam.transform.localPosition = 0.5f * Vector3.left * Hmd.InterlensDistance;
            _leftWorldCam.transform.localPosition = 0.5f * Vector3.left * Hmd.InterlensDistance;

            _rightWorldCam.transform.localPosition = 0.5f * Vector3.right * Hmd.InterlensDistance;
            _rightWorldCam.transform.localPosition = 0.5f * Vector3.right * Hmd.InterlensDistance;

            _leftWorldCam.projectionMatrix = projWorldLeft;
            _rightWorldCam.projectionMatrix = projWorldRight;

            UpdateBarrelDistortion(EyeMaterial, displayViewportLeft, projWorldLeft, projEyeLeft);
            Graphics.Blit(RenderTexture, destination, EyeMaterial);
        }

        private void UpdateBarrelDistortion(Material distortionShader, Rect viewportEyeLeft, Matrix4x4 projWorldLeft, Matrix4x4 projEyeLeft)
        {
            // Код заимствует некоторые детали реализации генератора профиля Google Cardboard https://vr.google.com/intl/ru_ru/cardboard/viewerprofilegenerator/
            // Оригинальный комментарий:
            // Shader params include parts of the projection matrices needed to
            // convert texture coordinates between distorted and undistorted
            // frustums.  The projections are adjusted to include transform between
            // texture space [0..1] and NDC [-1..1] as well as accounting for the
            // viewport on the screen.

            // Даны две проекционные матрицы, соответствующие полю зрения левого глаза в виртуальном (то что в игровой сцене) и реальном (внутри HMD, как если бы не было линз) мирах
            // Из компонент масштаба и переноса этих матриц составляются векторы, 
            // при этом в них вносятся поправки на пересчёт координат между диапазонами [0..1] и [-1..1], с учётом разбиения экрана.
            // Эти "матрицы" упрощённого вида позволяют производить преобразования в обе стороны.
            // Их использование в шейдере даёт возможность получать нужные "видовые" координаты без использования дополнительных камер и вызовов отрисовки

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
