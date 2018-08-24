﻿using UnityEngine;

namespace Assets.PseudoCardboard.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GridMesh : MonoBehaviour
    {
        public int SegmentWidth = 8;
        public int SegmentHeight = 5;

        public int Width = 4;
        public int Height = 4;

        void OnEnable()
        {
            Generate();
        }

        private Mesh mesh;

        private void Generate()
        {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "Procedural Grid";

            const float epsilon = 0.00001f;

            int baseLeft = 0;
            int limit = (SegmentWidth + 1) * (SegmentHeight + 1);
            int baseRight = limit;

            Vector3[] vertices = new Vector3[2 * limit];
            Vector2[] uv = new Vector2[2 * limit];

            float startXLeft = -0.5f * Width - epsilon;
            float startXRight = 0.0f + epsilon;
            float startY = -0.5f * Height;

            float stepX = 0.5f * Width / (float)SegmentWidth;
            float stepY = Height / (float)SegmentHeight;

            for (int i = 0, y = 0; y <= SegmentHeight; y++)
            {
                for (int x = 0; x <= SegmentWidth; x++, i++)
                {
                    vertices[baseLeft + i] = new Vector3(startXLeft + x * stepX, startY + y * stepY);
                    uv[baseLeft + i] = new Vector2(0.5f * x / SegmentWidth, y / (float)SegmentHeight);
                }
            }

            for (int i = 0, y = 0; y <= SegmentHeight; y++)
            {
                for (int x = 0; x <= SegmentWidth; x++, i++)
                {
                    vertices[baseRight + i] = new Vector3(startXRight + x * stepX, startY + y * stepY);
                    uv[baseRight + i] = new Vector2(0.5f + 0.5f * x / SegmentWidth, y / (float)SegmentHeight);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;

            int baseTriLeft = 0;
            int triLimit = SegmentWidth * SegmentHeight * 6;
            int baseTriRight = triLimit;

            int[] triangles = new int[2 * triLimit];

            for (int ti = 0, vi = baseLeft, y = 0; y < SegmentHeight; y++, vi++)
            {
                for (int x = 0; x < SegmentWidth; x++, ti += 6, vi++)
                {
                    triangles[baseTriLeft + ti] = vi;
                    triangles[baseTriLeft + ti + 3] = triangles[baseTriLeft + ti + 2] = vi + 1;
                    triangles[baseTriLeft + ti + 4] = triangles[baseTriLeft + ti + 1] = vi + SegmentWidth + 1;
                    triangles[baseTriLeft + ti + 5] = vi + SegmentWidth + 2;
                }
            }
            for (int ti = 0, vi = baseRight, y = 0; y < SegmentHeight; y++, vi++)
            {
                for (int x = 0; x < SegmentWidth; x++, ti += 6, vi++)
                {
                    triangles[baseTriRight + ti] = vi;
                    triangles[baseTriRight + ti + 3] = triangles[baseTriRight + ti + 2] = vi + 1;
                    triangles[baseTriRight + ti + 4] = triangles[baseTriRight + ti + 1] = vi + SegmentWidth + 1;
                    triangles[baseTriRight + ti + 5] = vi + SegmentWidth + 2;
                }
            }
            mesh.triangles = triangles;
        }
    }
}
