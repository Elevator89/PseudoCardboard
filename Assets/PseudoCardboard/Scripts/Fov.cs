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
    public struct Fov
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public Fov(float left, float right, float bottom, float top)
        {
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }

        public Fov GetFlippedHorizontally()
        {
            return new Fov
            {
                Left = this.Right,
                Right = this.Left,
                Bottom = this.Bottom,
                Top = this.Top,
            };
        }

        public Fov GetAtan()
        {
            return new Fov
            {
                Left = Mathf.Atan(this.Left),
                Right = Mathf.Atan(this.Right),
                Bottom = Mathf.Atan(this.Bottom),
                Top = Mathf.Atan(this.Top),
            };
        }

        public Fov GetTan()
        {
            return new Fov
            {
                Left = Mathf.Tan(this.Left),
                Right = Mathf.Tan(this.Right),
                Bottom = Mathf.Tan(this.Bottom),
                Top = Mathf.Tan(this.Top),
            };
        }

        public static Fov TanAnglesToAngles(Fov angles)
        {
            return angles.GetAtan() * Mathf.Rad2Deg;
        }

        public static Fov AnglesToTanAngles(Fov angles)
        {
            return (angles * Mathf.Deg2Rad).GetTan();
        }

        public static Fov Min(Fov fovA, Fov fovB)
        {
            return new Fov
            {
                Left = Mathf.Min(fovA.Left, fovB.Left),
                Right = Mathf.Min(fovA.Right, fovB.Right),
                Bottom = Mathf.Min(fovA.Bottom, fovB.Bottom),
                Top = Mathf.Min(fovA.Top, fovB.Top),
            };
        }

        public static Fov Max(Fov fovA, Fov fovB)
        {
            return new Fov
            {
                Left = Mathf.Max(fovA.Left, fovB.Left),
                Right = Mathf.Max(fovA.Right, fovB.Right),
                Bottom = Mathf.Max(fovA.Bottom, fovB.Bottom),
                Top = Mathf.Max(fovA.Top, fovB.Top),
            };
        }

        public static Fov operator /(Fov fov, float value)
        {
            return new Fov
            {
                Left = fov.Left / value,
                Right = fov.Right / value,
                Bottom = fov.Bottom / value,
                Top = fov.Top / value,
            };
        }

        public static Fov operator *(Fov fov, float value)
        {
            return new Fov
            {
                Left = fov.Left * value,
                Right = fov.Right * value,
                Bottom = fov.Bottom * value,
                Top = fov.Top * value,
            };
        }
    }
}
