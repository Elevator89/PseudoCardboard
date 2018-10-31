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
    public class Plane : MonoBehaviour
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
            DestroyImmediate(mesh);

            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "Procedural Grid";

            const float epsilon = 0.00001f;

            int baseInd = 0;
            int limit = (segmentWidth + 1) * (segmentHeight + 1); 

            Vector3[] vertices = new Vector3[limit];
            Vector2[] uv = new Vector2[limit];

            float startX = -0.5f * Width;
            float startY = -0.5f * Height;

            float stepX = Width / (float)segmentWidth;
            float stepY = Height / (float)segmentHeight;

            for (int i = 0, y = 0; y <= segmentHeight; y++)
            {
                for (int x = 0; x <= segmentWidth; x++, i++)
                {
                    vertices[baseInd + i] = new Vector3(startX + x * stepX, startY + y * stepY);
                    uv[baseInd + i] = new Vector2(x / (float)segmentWidth, y / (float)segmentHeight);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;

            int baseTri = 0;
            int triLimit = segmentWidth * segmentHeight * 6;

            int[] triangles = new int[2 * triLimit];

            for (int ti = 0, vi = baseInd, y = 0; y < segmentHeight; y++, vi++)
            {
                for (int x = 0; x < segmentWidth; x++, ti += 6, vi++)
                {
                    triangles[baseTri + ti] = vi;
                    triangles[baseTri + ti + 3] = triangles[baseTri + ti + 2] = vi + 1;
                    triangles[baseTri + ti + 4] = triangles[baseTri + ti + 1] = vi + segmentWidth + 1;
                    triangles[baseTri + ti + 5] = vi + segmentWidth + 2;
                }
            }
            mesh.triangles = triangles;
        }
    }
}
