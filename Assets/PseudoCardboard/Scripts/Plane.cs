using UnityEngine;

namespace Assets.PseudoCardboard.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Plane : MonoBehaviour
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

            int baseInd = 0;
            int limit = (SegmentWidth + 1) * (SegmentHeight + 1);

            Vector3[] vertices = new Vector3[limit];
            Vector2[] uv = new Vector2[limit];

            float startX = -0.5f * Width;
            float startY = -0.5f * Height;

            float stepX = Width / (float)SegmentWidth;
            float stepY = Height / (float)SegmentHeight;

            for (int i = 0, y = 0; y <= SegmentHeight; y++)
            {
                for (int x = 0; x <= SegmentWidth; x++, i++)
                {
                    vertices[baseInd + i] = new Vector3(startX + x * stepX, startY + y * stepY);
                    uv[baseInd + i] = new Vector2(x / (float)SegmentWidth, y / (float)SegmentHeight);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;

            int baseTri = 0;
            int triLimit = SegmentWidth * SegmentHeight * 6;

            int[] triangles = new int[2 * triLimit];

            for (int ti = 0, vi = baseInd, y = 0; y < SegmentHeight; y++, vi++)
            {
                for (int x = 0; x < SegmentWidth; x++, ti += 6, vi++)
                {
                    triangles[baseTri + ti] = vi;
                    triangles[baseTri + ti + 3] = triangles[baseTri + ti + 2] = vi + 1;
                    triangles[baseTri + ti + 4] = triangles[baseTri + ti + 1] = vi + SegmentWidth + 1;
                    triangles[baseTri + ti + 5] = vi + SegmentWidth + 2;
                }
            }
            mesh.triangles = triangles;
        }
    }
}
