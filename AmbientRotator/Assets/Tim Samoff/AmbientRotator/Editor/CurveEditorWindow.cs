#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace AmbientRotator
{
    public class CurveEditorWindow : EditorWindow
    {
        private SerializedProperty elementProperty;
        private int selectedCurve = 0;
        private string[] curveNames = { "X Curve", "Y Curve", "Z Curve" };
        private AnimationCurve previewCache;
        private float previewTime;
        
        public static void ShowWindow(SerializedProperty element)
        {
            var window = GetWindow<CurveEditorWindow>("Curve Editor");
            window.elementProperty = element;
            window.minSize = new Vector2(400, 300);
            window.Show();
        }
        
        private void OnEnable()
        {
            previewCache = new AnimationCurve();
            previewCache.AddKey(0, 0);
            previewCache.AddKey(0.5f, 1);
            previewCache.AddKey(1, 0);
        }
        
        private void OnGUI()
        {
            if (elementProperty == null)
            {
                EditorGUILayout.HelpBox("No curve selected", MessageType.Warning);
                if (GUILayout.Button("Close"))
                {
                    Close();
                }
                return;
            }
            
            // Draw toolbar
            selectedCurve = GUILayout.Toolbar(selectedCurve, curveNames);
            
            EditorGUILayout.Space();
            
            // Draw selected curve
            SerializedProperty curveProp = GetCurveProperty(selectedCurve);
            if (curveProp != null)
            {
                // Draw the curve editor
                EditorGUILayout.PropertyField(curveProp, GUIContent.none, GUILayout.Height(200));
            }
            
            EditorGUILayout.Space();
            
            // Draw curve tools
            DrawCurveTools();
            
            // Draw preview
            DrawPreview();
            
            if (GUI.changed)
            {
                elementProperty.serializedObject.ApplyModifiedProperties();
            }
        }
        
        private SerializedProperty GetCurveProperty(int index)
        {
            if (elementProperty == null) return null;
            
            switch(index)
            {
                case 0: return elementProperty.FindPropertyRelative("curveX");
                case 1: return elementProperty.FindPropertyRelative("curveY");
                case 2: return elementProperty.FindPropertyRelative("curveZ");
                default: return null;
            }
        }
        
        private void DrawCurveTools()
        {
            EditorGUILayout.LabelField("Curve Tools", EditorStyles.boldLabel);
            
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Smooth All"))
                {
                    SmoothCurve(GetCurveProperty(0));
                    SmoothCurve(GetCurveProperty(1));
                    SmoothCurve(GetCurveProperty(2));
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
                if (GUILayout.Button("Copy to All"))
                {
                    CopyToAllCurves(GetCurveProperty(selectedCurve));
                }
            }
        }
        
        private void DrawPreview()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            
            previewTime = EditorGUILayout.Slider("Time", previewTime, 0f, 1f);
            
            // Get the current curve and evaluate
            SerializedProperty curveProp = GetCurveProperty(selectedCurve);
            if (curveProp != null && curveProp.animationCurveValue != null)
            {
                float value = curveProp.animationCurveValue.Evaluate(previewTime);
                EditorGUILayout.LabelField($"Value at {previewTime:F2}: {value:F2}");
            }
        }
        
        private void SmoothCurve(SerializedProperty curveProp)
        {
            if (curveProp == null) return;
            
            AnimationCurve curve = curveProp.animationCurveValue;
            if (curve == null || curve.keys.Length == 0) return;
            
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
            if (curve == null || curve.keys.Length == 0) return;
            
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
            
            // Smooth all keys
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            
            curveProp.animationCurveValue = curve;
            curveProp.serializedObject.ApplyModifiedProperties();
        }
        
        private void CopyToAllCurves(SerializedProperty sourceProp)
        {
            if (sourceProp == null) return;
            
            AnimationCurve sourceCurve = sourceProp.animationCurveValue;
            if (sourceCurve == null) return;
            
            // Copy to X
            SerializedProperty xProp = GetCurveProperty(0);
            if (xProp != null && xProp != sourceProp)
            {
                xProp.animationCurveValue = new AnimationCurve(sourceCurve.keys);
            }
            
            // Copy to Y
            SerializedProperty yProp = GetCurveProperty(1);
            if (yProp != null && yProp != sourceProp)
            {
                yProp.animationCurveValue = new AnimationCurve(sourceCurve.keys);
            }
            
            // Copy to Z
            SerializedProperty zProp = GetCurveProperty(2);
            if (zProp != null && zProp != sourceProp)
            {
                zProp.animationCurveValue = new AnimationCurve(sourceCurve.keys);
            }
            
            elementProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif