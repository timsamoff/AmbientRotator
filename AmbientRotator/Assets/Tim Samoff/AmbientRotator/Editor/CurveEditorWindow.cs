#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace AmbientRotator
{
    public class CurveEditorWindow : EditorWindow
    {
        private SerializedProperty elementProperty;
        private AnimationCurvePreviewCache previewCache;
        private int selectedCurve = 0;
        private string[] curveNames = { "X Curve", "Y Curve", "Z Curve" };
        
        public static void ShowWindow(SerializedProperty element)
        {
            var window = GetWindow<CurveEditorWindow>("Curve Editor");
            window.elementProperty = element;
            window.Show();
        }
        
        private void OnGUI()
        {
            if (elementProperty == null)
            {
                EditorGUILayout.HelpBox("No curve selected", MessageType.Warning);
                return;
            }
            
            selectedCurve = GUILayout.Toolbar(selectedCurve, curveNames);
            
            EditorGUILayout.Space();
            
            SerializedProperty curveProp = GetCurveProperty(selectedCurve);
            if (curveProp != null)
            {
                EditorGUILayout.PropertyField(curveProp, GUIContent.none);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Curve Tools", EditorStyles.boldLabel);
            
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Smooth"))
                {
                    SmoothCurve(GetCurveProperty(selectedCurve));
                }
                if (GUILayout.Button("Invert"))
                {
                    InvertCurve(GetCurveProperty(selectedCurve));
                }
                if (GUILayout.Button("Reset"))
                {
                    ResetCurve(GetCurveProperty(selectedCurve));
                }
            }
            
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sine Wave"))
                {
                    SetWaveform(GetCurveProperty(selectedCurve), WaveformType.Sine);
                }
                if (GUILayout.Button("Square Wave"))
                {
                    SetWaveform(GetCurveProperty(selectedCurve), WaveformType.Square);
                }
                if (GUILayout.Button("Triangle"))
                {
                    SetWaveform(GetCurveProperty(selectedCurve), WaveformType.Triangle);
                }
            }
            
            if (GUI.changed)
            {
                elementProperty.serializedObject.ApplyModifiedProperties();
            }
        }
        
        private SerializedProperty GetCurveProperty(int index)
        {
            switch(index)
            {
                case 0: return elementProperty.FindPropertyRelative("curveX");
                case 1: return elementProperty.FindPropertyRelative("curveY");
                case 2: return elementProperty.FindPropertyRelative("curveZ");
                default: return null;
            }
        }
        
        private void SmoothCurve(SerializedProperty curveProp)
        {
            if (curveProp == null) return;
            
            AnimationCurve curve = curveProp.animationCurveValue;
            Keyframe[] keys = curve.keys;
            
            for (int i = 0; i < keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            
            curveProp.animationCurveValue = curve;
            curveProp.serializedObject.ApplyModifiedProperties();
        }
        
        private void InvertCurve(SerializedProperty curveProp)
        {
            if (curveProp == null) return;
            
            AnimationCurve curve = curveProp.animationCurveValue;
            Keyframe[] keys = curve.keys;
            
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].value = -keys[i].value;
            }
            
            curve.keys = keys;
            curveProp.animationCurveValue = curve;
            curveProp.serializedObject.ApplyModifiedProperties();
        }
        
        private void ResetCurve(SerializedProperty curveProp)
        {
            if (curveProp == null) return;
            curveProp.animationCurveValue = AnimationCurve.Linear(0, 0, 1, 0);
            curveProp.serializedObject.ApplyModifiedProperties();
        }
        
        private enum WaveformType { Sine, Square, Triangle }
        
        private void SetWaveform(SerializedProperty curveProp, WaveformType type)
        {
            if (curveProp == null) return;
            
            AnimationCurve curve = new AnimationCurve();
            int resolution = 20;
            
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                float value = 0f;
                
                switch(type)
                {
                    case WaveformType.Sine:
                        value = Mathf.Sin(t * Mathf.PI * 2);
                        break;
                    case WaveformType.Square:
                        value = Mathf.Sign(Mathf.Sin(t * Mathf.PI * 2));
                        break;
                    case WaveformType.Triangle:
                        value = Mathf.PingPong(t * 2, 1) * 2 - 1;
                        break;
                }
                
                curve.AddKey(t, value);
            }
            
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            
            curveProp.animationCurveValue = curve;
            curveProp.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif