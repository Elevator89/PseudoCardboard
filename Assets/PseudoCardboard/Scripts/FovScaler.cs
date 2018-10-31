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
    [ExecuteInEditMode]
    public class FovScaler : MonoBehaviour
    {
        public void SetFov(Fov fovTanAngles)
        {
            float dist = transform.localPosition.z;

            Fov fovDistances = fovTanAngles * dist;

            transform.localPosition = new Vector3(0.5f * (fovDistances.Right - fovDistances.Left), 0.5f * (fovDistances.Top - fovDistances.Bottom), dist);
            transform.localScale = new Vector3(fovDistances.Right + fovDistances.Left, fovDistances.Top + fovDistances.Bottom, 1f);
        }
    }
}
