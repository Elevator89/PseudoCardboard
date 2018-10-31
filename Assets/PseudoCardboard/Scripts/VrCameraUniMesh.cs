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
	/// Для VR-представления применяется вершинный шейдер с заданным процедурным мешем. 
	/// Меш представляет собой две равномерных полигональных сетки, раздёлённых минимальным промежутком.
	/// Изображения с камер сохраняются в текстуру, которая натягивается на этот меш.
	/// Меш обрабатывается вершинным шейдером, который производит коррекцию дисторсии в одной плоскости
	/// Фрагментный шейдер производит перспективную коррекцию текстуры в "ручном" режиме.
	/// Метод использует всего три камеры и одну поверхность, что позволяет экономить на вызовах отрисовки по сравнению с VrCameraBiMesh
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class VrCameraUniMesh : MonoBehaviour
	{
		public Material EyeMaterial;

		private Camera _camWorldLeft;
		private Camera _camWorldRight;

		private const int texWidth = 256;
		private Texture2D _undistortionTex;

		private bool _hmdParamsDirty = true;

		private DisplayParameters _display;

		void OnEnable()
		{
			_display = null;

			_camWorldLeft = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Left);
			_camWorldRight = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Right);

			_camWorldLeft.transform.localRotation = Quaternion.identity;
			_camWorldRight.transform.localRotation = Quaternion.identity;

			_undistortionTex = new Texture2D(texWidth, 1, TextureFormat.RFloat, false, true);
			_undistortionTex.filterMode = FilterMode.Bilinear;
			_undistortionTex.wrapMode = TextureWrapMode.Clamp;
			_undistortionTex.alphaIsTransparency = false;

			HmdParameters.Instance.ParamsChanged.AddListener(OnHmdParamsChanged);
		}

		void OnDisable()
		{
			HmdParameters.Instance.ParamsChanged.RemoveListener(OnHmdParamsChanged);
			DestroyImmediate(_undistortionTex);
		}

		void OnHmdParamsChanged(HmdParameters hmd)
		{
			_hmdParamsDirty = true;
		}

		void Update()
		{
			DisplayParameters newDisplay = DisplayParameters.Collect();

			if (_hmdParamsDirty || !newDisplay.Equals(_display))
			{
				UpdateView(HmdParameters.Instance, newDisplay);
				_hmdParamsDirty = false;
				_display = newDisplay;
			}
		}

		void UpdateView(HmdParameters hmd, DisplayParameters display)
		{
			Distortion distortion = new Distortion(hmd.DistortionK1, hmd.DistortionK2);

			float zNear = _camWorldLeft.nearClipPlane;
			float zFar = _camWorldLeft.farClipPlane;

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

			_camWorldLeft.transform.localPosition = 0.5f * Vector3.left * hmd.InterlensDistance;
			_camWorldRight.transform.localPosition = 0.5f * Vector3.right * hmd.InterlensDistance;

			_camWorldLeft.projectionMatrix = projWorldLeft;
			_camWorldRight.projectionMatrix = projWorldRight;

			float maxWorldFovTanAngle = GetMaxDiagonalValue(fovWorldTanAnglesLeft);

			UpdateUndistortionTex(distortion, maxWorldFovTanAngle);

			EyeMaterial.SetFloat("_MaxWorldFovTanAngle", maxWorldFovTanAngle);
			EyeMaterial.SetTexture("_UndistortionTex", _undistortionTex);
			EyeMaterial.SetVector("_ProjectionWorldLeft", ComposeProjectionVector(projWorldLeft));
			EyeMaterial.SetVector("_ProjectionEyeLeft", ComposeProjectionVector(projEyeLeft));
		}

		private void UpdateUndistortionTex(Distortion distortion, float maxDistortedValue)
		{
			const int n = 16;
			float[] distortedValues = new float[n];

			float distortedValue = 0f;
			float distortedValueStep = maxDistortedValue / (n - 1);

			// Генерируется n точек
			for (int i = 0; i < n; i++)
			{
				distortedValues[i] = distortedValue;
				distortedValue += distortedValueStep;
			}

			// Составляется кубический сплайн по n точкам. Сплайн используется для интерполяции функции, обратной к дисторсии
			CubicHermiteSpline spline = new CubicHermiteSpline(distortion, distortedValues);

			distortedValue = 0f;
			distortedValueStep = maxDistortedValue / (texWidth - 1);

			// Точно известно, что функция дисторсии принимает значения больше своего аргумента
			// Следовательно обратная функция будет всегда меньше аргумента. Это значит, что значение обратной ф-ции можно нормировать.
			// Для каждой точки текстуры вычисляется значение сплайна в точке, после чего делится на значение самой точки. Получается число от [0..1], оно записывается в цвет.
			for (int i = 0; i < _undistortionTex.width; ++i)
			{
				float eyeTanAngleNormalized = 1f;

				if (i > 0)
				{
					float undistortedValue = spline.GetValue(distortedValue);

					// Страховка от отклонений сплайна
					if (undistortedValue < distortedValue)
						eyeTanAngleNormalized = undistortedValue / distortedValue;
				}

				_undistortionTex.SetPixel(i, 0, new Color(eyeTanAngleNormalized, 0f, 0f));

				distortedValue += distortedValueStep;
			}

			_undistortionTex.Apply();
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

		private float GetMaxDiagonalValue(Fov fov)
		{
			float maxHorizontalFov = Mathf.Max(fov.Left, fov.Right);
			float maxVerticalFov = Mathf.Max(fov.Bottom, fov.Top);

			return Mathf.Sqrt(maxHorizontalFov * maxHorizontalFov + maxVerticalFov * maxVerticalFov);
		}
	}
}
