using UnityEditor;
using UnityEngine;

namespace Assets.PseudoCardboard.Editor
{
    [ExecuteInEditMode]
    public class HmdParamsWindow : EditorWindow
    {
        [MenuItem("PseudoVR/Settings...", false, 200)]
        private static void OpenSettings()
        {
            HmdParamsWindow window = GetWindow<HmdParamsWindow>();
            window.minSize = new Vector2(400.0f, 380.0f);
            window.maxSize = new Vector2(800.0f, 380.0f);
            window.titleContent = new GUIContent("HMD Settings");
            window.Show();
        }

        private void OnGUI()
        {
            HmdParameters hmd = HmdParameters.Instance;
            MeshParameters mesh = MeshParameters.Instance;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("HMD Settings");

            hmd.DistortionK1 = EditorGUILayout.Slider("DistortionK1", hmd.DistortionK1, 0f, 2f, GUILayout.ExpandWidth(true));
            hmd.DistortionK2 = EditorGUILayout.Slider("DistortionK2", hmd.DistortionK2, 0f, 2f, GUILayout.ExpandWidth(true));
            hmd.InterlensDistance = EditorGUILayout.Slider("InterlensDistance", hmd.InterlensDistance, 0f, 0.2f, GUILayout.ExpandWidth(true));
            hmd.ScreenToLensDist = EditorGUILayout.Slider("ScreenToLensDist", hmd.ScreenToLensDist, 0.01f, 0.2f, GUILayout.ExpandWidth(true));
            hmd.EyeOffsetY = EditorGUILayout.Slider("EyeOffsetY", hmd.EyeOffsetY, 0f, 0.2f, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("FoV");
            float fovLeft = EditorGUILayout.Slider("Left", hmd.MaxFovAngles.Left, 0f, 89f, GUILayout.ExpandWidth(true));
            float fovRight = EditorGUILayout.Slider("Right", hmd.MaxFovAngles.Right, 0f, 89f, GUILayout.ExpandWidth(true));
            float fovTop = EditorGUILayout.Slider("Top", hmd.MaxFovAngles.Top, 0f, 89f, GUILayout.ExpandWidth(true));
            float fovBottom = EditorGUILayout.Slider("Bottom", hmd.MaxFovAngles.Bottom, 0f, 89f, GUILayout.ExpandWidth(true));

            hmd.MaxFovAngles = new Fov(fovLeft, fovRight, fovBottom, fovTop);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Mesh Settings");

            mesh.SegmentWidth = EditorGUILayout.IntSlider("SegmentWidth", mesh.SegmentWidth, 1, 16, GUILayout.ExpandWidth(true));
            mesh.SegmentHeight = EditorGUILayout.IntSlider("SegmentHeight", mesh.SegmentHeight, 1, 16, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Restore defaults", GUILayout.ExpandWidth(false)))
            {
                hmd.LoadDefaults();
                mesh.LoadDefaults();
            }
        }
    }
}
