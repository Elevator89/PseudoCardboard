using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    [ExecuteInEditMode]
    public class RenderPlane : MonoBehaviour
    {
        public Transform Plane;
        public Camera Camera;

        void Update()
        {
            float ratio = Camera.pixelRect.width / (float)Camera.pixelRect.height;


            Vector3 scale = new Vector3(ratio * Camera.orthographicSize, Camera.orthographicSize);
            Plane.localScale = scale;
        }
    }
}
