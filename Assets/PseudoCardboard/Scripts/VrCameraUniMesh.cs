﻿using System.IO;
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

		private Camera _centralCam;

		private Camera _camWorldLeft;
		private Camera _camWorldRight;

		private Distortion _distortion;
		private DisplayParameters Display;
		private HmdParameters Hmd;

		private const int texWidth = 2048;
		private Texture2D _undistortionTex;

		void OnEnable()
		{
			Hmd = HmdParameters.Instance;
			Display = new DisplayParameters();

			_centralCam = GetComponent<Camera>();

			_camWorldLeft = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Left);
			_camWorldRight = GetComponentsInChildren<Camera>().First(cam => cam.stereoTargetEye == StereoTargetEyeMask.Right);

			_camWorldLeft.transform.localRotation = Quaternion.identity;
			_camWorldRight.transform.localRotation = Quaternion.identity;

			_distortion = new Distortion(Hmd.DistortionK1, Hmd.DistortionK2);
			_undistortionTex = new Texture2D(texWidth, 1, TextureFormat.RGBAFloat, false, true);
			_undistortionTex.alphaIsTransparency = false;
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

			_camWorldLeft.projectionMatrix = projWorldLeft;
			_camWorldRight.projectionMatrix = projWorldRight;

			float maxEyeFovTanAngle = GetMaxValue(fovEyeTanAglesLeft);
			float maxWorldFovTanAngle = GetMaxValue(fovWorldTanAnglesLeft);

			UpdateBarrelDistortion(EyeMaterial, maxEyeFovTanAngle, maxWorldFovTanAngle, displayViewportLeft, projWorldLeft, projEyeLeft);
		}

		// Set barrel_distortion parameters given CardboardView.
		private void UpdateBarrelDistortion(Material distortionShader, float maxEyeFovTanAngle, float maxWorldFovTanAngle, Rect viewportEyeLeft, Matrix4x4 projWorldLeft, Matrix4x4 projEyeLeft)
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


			UpdateUndistortionTex(distortionShader, maxEyeFovTanAngle);

			distortionShader.SetTexture("_UndistortionTex", _undistortionTex);

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

		private void UpdateUndistortionTex(Material distortionShader, float maxArgument)
		{
			const int n = 16;
			float[] distortedValues = new float[n];
			float stepArg = maxArgument / (n - 1);

			for (int i = 0; i < n; i++)
			{
				float argument = i * stepArg;
				distortedValues[i] = _distortion.Distort(argument);
			}

			float maxDistortedValue = distortedValues[n - 1];

			CubicHermiteSpline spline = new CubicHermiteSpline(_distortion, distortedValues);

			for (int i = 0; i < _undistortionTex.width; ++i)
			{
				float eyeTanAngleNormalized = 1f;

				if (i > 0)
				{
					float distortedValue = maxDistortedValue * (float)i / texWidth;
					float undistortedValue = spline.GetValue(distortedValue);

					if (distortedValue < undistortedValue)
						eyeTanAngleNormalized = distortedValue / undistortedValue;
				}

				_undistortionTex.SetPixel(i, 0, new Color(eyeTanAngleNormalized, eyeTanAngleNormalized, eyeTanAngleNormalized));
			}

			_undistortionTex.Apply();
			byte[] bytes = _undistortionTex.EncodeToPNG();
			File.WriteAllBytes("xx.png", bytes);


			distortionShader.SetFloat("_MaxWorldFovTanAngle", maxDistortedValue);
		}

		private float GetMaxValue(Fov fov)
		{
			float maxHorizontalFov = Mathf.Max(fov.Left, fov.Right);
			float maxVerticalFov = Mathf.Max(fov.Bottom, fov.Top);

			return Mathf.Sqrt(maxHorizontalFov * maxHorizontalFov + maxVerticalFov * maxVerticalFov);
		}
	}
}
