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

namespace Assets.PseudoCardboard
{
	public class Distortion
	{
		public float DistortionK1;
		public float DistortionK2;

		public Distortion(float distortionK1, float distortionK2)
		{
			DistortionK1 = distortionK1;
			DistortionK2 = distortionK2;
		}

		public float UndistortOld(float radius)
		{
			float r0 = radius * 0.9f;
			float r1 = radius / 0.9f;
			float r2;
			float dr1 = radius - Distort(r1);
			float dr0;
			while (Mathf.Abs(r0 - r1) > 0.0001f)
			{
				dr0 = radius - Distort(r0);
				r2 = r0 - dr0 * ((r0 - r1) / (dr0 - dr1));
				r1 = r0;
				r0 = r2;
				dr1 = dr0;
			}
			return r0;
		}

		public float Undistort(float r)
		{
			float rk = r;
			float drk = Distort(rk) - r; // решаем уравнение f(x) = DistortInner(rk) - r = 0

			int i = 0;
			while (Mathf.Abs(drk) > 0.0001f && i < 100)
			{
				rk = rk - drk / DistortDerivative(rk);
				drk = Distort(rk) - r;
				++i;
			}

			return rk;
		}

		public float Distort(float r)
		{
			float r2 = r * r;
			float r3 = r2 * r;
			float r5 = r2 * r3;
			return DistortionK2 * r5 + DistortionK1 * r3 + r;
		}

		public float DistortDerivative(float r)
		{
			float r2 = r * r;
			float r4 = r2 * r2;
			return 5f * DistortionK2 * r4 + 3f * DistortionK1 * r2 + 1;
		}
	}
}
