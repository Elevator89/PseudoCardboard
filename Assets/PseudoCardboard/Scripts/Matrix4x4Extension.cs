using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.PseudoCardboard
{
	public static class Matrix4x4Ext
	{
		public static Matrix4x4 CreateFrustum(float left, float right, float bottom, float top, float zNear, float zFar)
		{
			float zDelta = (zFar - zNear);
			float dir = (right - left);
			float height = (top - bottom);
			float zNear2 = 2 * zNear;

			return new Matrix4x4()
			{
				m00 = 2.0f * zNear / dir,
				m01 = 0.0f,
				m02 = (right + left) / dir,
				m03 = 0.0f,
				m10 = 0.0f,
				m11 = zNear2 / height,
				m12 = (top + bottom) / height,
				m13 = 0.0f,
				m20 = 0.0f,
				m21 = 0.0f,
				m22 = -(zFar + zNear) / zDelta,
				m23 = -zNear2 * zFar / zDelta,
				m30 = 0.0f,
				m31 = 0.0f,
				m32 = -1.0f,
				m33 = 0.0f,
			};
		}
	}
}
