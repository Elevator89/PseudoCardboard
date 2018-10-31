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
