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
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlaneSplitted : MonoBehaviour
    {
        public int Width = 4;
        public int Height = 4;

        MeshParameters _meshParams;

        void OnEnable()
        {
            _meshParams = MeshParameters.Instance;
            Refresh(_meshParams.SegmentWidth, _meshParams.SegmentHeight);
            _meshParams.ParamsChanged.AddListener(Refresh);
        }

        void OnDisable()
        {
            _meshParams.ParamsChanged.RemoveListener(Refresh);
        }

        private Mesh mesh;

        public void Refresh(int segmentWidth, int segmentHeight)
        {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "Procedural Grid";

            const float epsilon = 0.00001f;

            int baseLeft = 0;
            int limit = (segmentWidth + 1) * (segmentHeight + 1);
            int baseRight = limit;

            Vector3[] vertices = new Vector3[2 * limit];
            Vector2[] uv = new Vector2[2 * limit];

            float startXLeft = -0.5f * Width - epsilon;
            float startXRight = 0.0f + epsilon;
            float startY = -0.5f * Height;

            float stepX = 0.5f * Width / (float)segmentWidth;
            float stepY = Height / (float)segmentHeight;

            for (int i = 0, y = 0; y <= segmentHeight; y++)
            {
                for (int x = 0; x <= segmentWidth; x++, i++)
                {
                    vertices[baseLeft + i] = new Vector3(startXLeft + x * stepX, startY + y * stepY);
                    uv[baseLeft + i] = new Vector2(0.5f * x / segmentWidth, y / (float)segmentHeight);
                }
            }

            for (int i = 0, y = 0; y <= segmentHeight; y++)
            {
                for (int x = 0; x <= segmentWidth; x++, i++)
                {
                    vertices[baseRight + i] = new Vector3(startXRight + x * stepX, startY + y * stepY);
                    uv[baseRight + i] = new Vector2(0.5f + 0.5f * x / segmentWidth, y / (float)segmentHeight);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;

            int baseTriLeft = 0;
            int triLimit = segmentWidth * segmentHeight * 6;
            int baseTriRight = triLimit;

            int[] triangles = new int[2 * triLimit];

            for (int ti = 0, vi = baseLeft, y = 0; y < segmentHeight; y++, vi++)
            {
                for (int x = 0; x < segmentWidth; x++, ti += 6, vi++)
                {
                    triangles[baseTriLeft + ti] = vi;
                    triangles[baseTriLeft + ti + 3] = triangles[baseTriLeft + ti + 2] = vi + 1;
                    triangles[baseTriLeft + ti + 4] = triangles[baseTriLeft + ti + 1] = vi + segmentWidth + 1;
                    triangles[baseTriLeft + ti + 5] = vi + segmentWidth + 2;
                }
            }
            for (int ti = 0, vi = baseRight, y = 0; y < segmentHeight; y++, vi++)
            {
                for (int x = 0; x < segmentWidth; x++, ti += 6, vi++)
                {
                    triangles[baseTriRight + ti] = vi;
                    triangles[baseTriRight + ti + 3] = triangles[baseTriRight + ti + 2] = vi + 1;
                    triangles[baseTriRight + ti + 4] = triangles[baseTriRight + ti + 1] = vi + segmentWidth + 1;
                    triangles[baseTriRight + ti + 5] = vi + segmentWidth + 2;
                }
            }
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
    }
}
