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

		void OnEnable()
        {
            _leftWorldCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Left);
            _rightWorldCam = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Right);

            _leftWorldCam.transform.localRotation = Quaternion.identity;
            _rightWorldCam.transform.localRotation = Quaternion.identity;
        }

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			UpdateView(HmdParameters.Instance, DisplayParameters.Collect());
			Graphics.Blit(RenderTexture, destination, EyeMaterial);
		}

		void UpdateView(HmdParameters hmd, DisplayParameters display)
		{
			Distortion distortion = new Distortion(hmd.DistortionK1, hmd.DistortionK2);

            float zNear = _leftWorldCam.nearClipPlane;
            float zFar = _leftWorldCam.farClipPlane;

			Fov displayDistancesLeft = Calculator.GetFovDistancesLeft(display, hmd);

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

			_leftWorldCam.transform.localPosition = 0.5f * Vector3.left * hmd.InterlensDistance;
            _leftWorldCam.transform.localPosition = 0.5f * Vector3.left * hmd.InterlensDistance;

            _rightWorldCam.transform.localPosition = 0.5f * Vector3.right * hmd.InterlensDistance;
            _rightWorldCam.transform.localPosition = 0.5f * Vector3.right * hmd.InterlensDistance;

            _leftWorldCam.projectionMatrix = projWorldLeft;
            _rightWorldCam.projectionMatrix = projWorldRight;

			EyeMaterial.SetFloat("_DistortionK1", hmd.DistortionK1);
			EyeMaterial.SetFloat("_DistortionK2", hmd.DistortionK2);
			EyeMaterial.SetVector("_ProjectionWorldLeft", ComposeProjectionVector(projWorldLeft));
			EyeMaterial.SetVector("_ProjectionEyeLeft", ComposeProjectionVector(projEyeLeft));
        }

		private Vector4 ComposeProjectionVector(Matrix4x4 projectionMatrix)
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

			return new Vector4(
				projectionMatrix[0, 0],
				projectionMatrix[1, 1],
				projectionMatrix[0, 2] - 1,
				projectionMatrix[1, 2] - 1) / 2.0f;
		}
	}
}
