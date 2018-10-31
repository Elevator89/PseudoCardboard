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
    public static class Calculator
    {
        public static Rect GetViewportLeft(Fov fovDistances, Vector2 dpm)
        {
            return new Rect(
                0,
                0,
                (fovDistances.Left + fovDistances.Right) * dpm.x,
                (fovDistances.Bottom + fovDistances.Top) * dpm.y);
        }

        public static Fov GetFovDistancesLeft(DisplayParameters display, HmdParameters hmd)
        {
            float outerDist = 0.5f * (display.Size.x - hmd.InterlensDistance);
            float innerDist = 0.5f * hmd.InterlensDistance;
            float bottomDist = hmd.EyeOffsetY;
            float topDist = display.Size.y - bottomDist;

            return new Fov(outerDist, innerDist, bottomDist, topDist);
        }

        public static Fov DistortTanAngles(Fov tanAngles, Distortion distortion)
        {
            return new Fov
            (
                distortion.Distort(tanAngles.Left),
                distortion.Distort(tanAngles.Right),
                distortion.Distort(tanAngles.Bottom),
                distortion.Distort(tanAngles.Top)
            );
        }

        public static void ComposeProjectionMatricesFromFovAngles(Fov leftFovAngles, float near, float far, out Matrix4x4 left, out Matrix4x4 right)
        {
            Fov leftFovTanAngles = (leftFovAngles * Mathf.Deg2Rad).GetTan();

            ComposeProjectionMatricesFromFovTanAngles(leftFovTanAngles, near, far, out left, out right);
        }

        public static void ComposeProjectionMatricesFromFovTanAngles(Fov leftFovTanAngles, float near, float far, out Matrix4x4 left, out Matrix4x4 right)
        {
            Fov nearProjection = leftFovTanAngles * near;

            left = Matrix4x4Ext.CreateFrustum(-nearProjection.Left, nearProjection.Right, -nearProjection.Bottom, nearProjection.Top, near, far);
            right = Matrix4x4Ext.CreateFrustum(-nearProjection.Right, nearProjection.Left, -nearProjection.Bottom, nearProjection.Top, near, far);
        }

    }
}
