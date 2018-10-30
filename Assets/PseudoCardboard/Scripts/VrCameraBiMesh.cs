﻿using UnityEngine;

namespace Assets.PseudoCardboard.Scripts
{
    /// Для VR-представления применяется вершинные шейдеры с заданными процедурными мешами. 
    /// Меши представляют собой равномерные полигональные сетки и располагаются на сцене, частично перекрывая друг друга.
    /// Изображения с камер сохраняются в текстуры, которых натягиваются на меши.
    /// Если пользователь без линз посмотрит левым глазом на левый меш, а правым на правый, то получится в точности та картинка, которую пользователь должен увидеть в шлеме с линзами (после всех коррекций)
    /// Меш обрабатывается вершинным шейдером, который производит коррекцию дисторсии, меняя координату z у вершин мешей, меш становится объёмным
    /// На обработанные меши "смотрит" другая пара камер, поле зрения которых соответсвует параметрам глаз пользоватля в HMD
    /// Метод использует пять камеры и две поверхности, но даёт максимально наглядную иллюстрацию работающего принципа
    [ExecuteInEditMode]
    public class VrCameraBiMesh : MonoBehaviour
    {
        public Material EyeMaterialLeft;
        public Material EyeMaterialRight;

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
            Hmd = HmdParameters.Instance;
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
            _distortion.DistortionK1 = Hmd.DistortionK1;
            _distortion.DistortionK2 = Hmd.DistortionK2;

            float zNear = _camWorldLeft.nearClipPlane;
            float zFar = _camWorldLeft.farClipPlane;

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

            EyeMaterialLeft.SetFloat("_DistortionK1", Hmd.DistortionK1);
            EyeMaterialRight.SetFloat("_DistortionK1", Hmd.DistortionK1);

            EyeMaterialLeft.SetFloat("_DistortionK2", Hmd.DistortionK2);
            EyeMaterialRight.SetFloat("_DistortionK2", Hmd.DistortionK2);
        }
    }
}
