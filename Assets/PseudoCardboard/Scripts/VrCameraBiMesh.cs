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

using UnityEngine;

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

        private DisplayParameters _display;

        void OnEnable()
        {
            _display = new DisplayParameters();

            _camWorldLeft.transform.localRotation = Quaternion.identity;
            _camWorldRight.transform.localRotation = Quaternion.identity;

            _camWorldLeftScaler = _camWorldLeft.GetComponentInChildren<FovScaler>();
            _camWorldRightScaler = _camWorldRight.GetComponentInChildren<FovScaler>();

            _camEyeLeft.transform.localRotation = Quaternion.identity;
            _camEyeRight.transform.localRotation = Quaternion.identity;

            OnHmdParamsChanged(HmdParameters.Instance);

            HmdParameters.Instance.ParamsChanged.AddListener(OnHmdParamsChanged);
        }

        void OnDisable()
        {
            HmdParameters.Instance.ParamsChanged.RemoveListener(OnHmdParamsChanged);
        }

        void OnHmdParamsChanged(HmdParameters hmd)
        {
            Distortion distortion = new Distortion(hmd.DistortionK1, hmd.DistortionK2);

            distortion.DistortionK1 = hmd.DistortionK1;
            distortion.DistortionK2 = hmd.DistortionK2;

            float zNear = _camWorldLeft.nearClipPlane;
            float zFar = _camWorldLeft.farClipPlane;

			Fov displayDistancesLeft = Calculator.GetFovDistancesLeft(_display, hmd);

			// То, как должен видеть левый глаз свой кусок экрана. Без линзы. C учётом только размеров дисплея
			Fov fovDisplayTanAngles = displayDistancesLeft / hmd.ScreenToLensDist;

			// FoV шлема
			Fov hmdMaxFovTanAngles = Fov.AnglesToTanAngles(hmd.MaxFovAngles);

			// То, как должен видеть левый глаз свой кусок экрана. Без линзы. C учётом размеров дисплея и FoV шлема
			Fov fovEyeTanAglesLeft = Fov.Min(fovDisplayTanAngles, hmdMaxFovTanAngles);

			// То, как должен видеть левый глаз. Мнимое изображение (после увеличения идеальной линзой без искажений). С широким углом. Именно так надо снять сцену
			Fov fovWorldTanAnglesLeft = Calculator.DistortTanAngles(fovEyeTanAglesLeft, distortion);

			Matrix4x4 projWorldLeft;
			Matrix4x4 projWorldRight;
			Calculator.ComposeProjectionMatricesFromFovTanAngles(fovWorldTanAnglesLeft, zNear, zFar, out projWorldLeft, out projWorldRight);

			Matrix4x4 projEyeLeft;
			Matrix4x4 projEyeRight;
			Calculator.ComposeProjectionMatricesFromFovTanAngles(fovDisplayTanAngles, zNear, zFar, out projEyeLeft, out projEyeRight);

			_camWorldLeft.transform.localPosition = 0.5f * Vector3.left * hmd.InterlensDistance;
            _camWorldRight.transform.localPosition = 0.5f * Vector3.right * hmd.InterlensDistance;

            _camEyeLeft.transform.localPosition = 0.5f * Vector3.left * hmd.InterlensDistance;
            _camEyeRight.transform.localPosition = 0.5f * Vector3.right * hmd.InterlensDistance;

            _camWorldLeft.projectionMatrix = projWorldLeft;
            _camWorldRight.projectionMatrix = projWorldRight;

            _camWorldLeftScaler.SetFov(fovWorldTanAnglesLeft);
            _camWorldRightScaler.SetFov(fovWorldTanAnglesLeft.GetFlippedHorizontally());

            _camEyeLeft.projectionMatrix = projEyeLeft;
            _camEyeRight.projectionMatrix = projEyeRight;

            EyeMaterialLeft.SetFloat("_DistortionK1", hmd.DistortionK1);
            EyeMaterialRight.SetFloat("_DistortionK1", hmd.DistortionK1);

            EyeMaterialLeft.SetFloat("_DistortionK2", hmd.DistortionK2);
            EyeMaterialRight.SetFloat("_DistortionK2", hmd.DistortionK2);
        }
    }
}
