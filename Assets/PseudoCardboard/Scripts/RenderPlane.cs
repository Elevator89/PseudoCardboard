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

            float len = Vector3.Distance(Plane.position, Camera.transform.position);
            float verticalScale = 2f * Mathf.Tan(0.5f * Camera.fieldOfView * Mathf.Deg2Rad) * len;

            Vector3 scale = new Vector3(ratio * verticalScale, verticalScale);
            Plane.localScale = scale;
        }
    }
}
