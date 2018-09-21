using UnityEditor;
using UnityEngine;

namespace Assets.PseudoCardboard.Editor
{
    [ExecuteInEditMode]
    public class HmdParamsWindow : EditorWindow
    {
        [MenuItem("PseudoVR/HMD settings...", false, 200)]
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

            hmd.DistortionK1 = EditorGUILayout.Slider("DistortionK1", hmd.DistortionK1, 0f, 2f, GUILayout.ExpandWidth(true));
            hmd.DistortionK2 = EditorGUILayout.Slider("DistortionK2", hmd.DistortionK2, 0f, 2f, GUILayout.ExpandWidth(true));
            hmd.InterlensDistance = EditorGUILayout.Slider("InterlensDistance", hmd.InterlensDistance, 0f, 0.2f, GUILayout.ExpandWidth(true));
            hmd.ScreenToLensDist = EditorGUILayout.Slider("ScreenToLensDist", hmd.ScreenToLensDist, 0.01f, 0.2f, GUILayout.ExpandWidth(true));
            hmd.EyeOffsetY = EditorGUILayout.Slider("EyeOffsetY", hmd.EyeOffsetY, 0f, 0.2f, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("FoV Settings");
            float fovLeft = EditorGUILayout.Slider("Left", hmd.MaxFovAngles.Left, 0f, 89f, GUILayout.ExpandWidth(true));
            float fovRight = EditorGUILayout.Slider("Right", hmd.MaxFovAngles.Right, 0f, 89f, GUILayout.ExpandWidth(true));
            float fovTop = EditorGUILayout.Slider("Top", hmd.MaxFovAngles.Top, 0f, 89f, GUILayout.ExpandWidth(true));
            float fovBottom = EditorGUILayout.Slider("Bottom", hmd.MaxFovAngles.Bottom, 0f, 89f, GUILayout.ExpandWidth(true));

            hmd.MaxFovAngles = new Fov(fovLeft, fovRight, fovBottom, fovTop);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Restore defaults", GUILayout.ExpandWidth(false)))
            {
                hmd.DistortionK1 = 0.51f;
                hmd.DistortionK2 = 0.16f;
                hmd.InterlensDistance = 0.065f;
                hmd.ScreenToLensDist = 0.045f;
                hmd.EyeOffsetY = 0.035f;
                hmd.MaxFovAngles = new Fov { Left = 50, Right = 50, Top = 50, Bottom = 50 };
            }
        }
    }
}
