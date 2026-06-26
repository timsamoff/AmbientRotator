#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace AmbientRotator
{
    [CustomEditor(typeof(CustomMotionProfile))]
    public class CustomProfileEditor : Editor
    {
        private CustomMotionProfile profile;
        private ReorderableList layerList;
        private bool showCurvePreview = false;
        private float previewTime = 0f;
        private Vector3 previewValue;
        
        private void OnEnable()
        {
            profile = (CustomMotionProfile)target;
            SetupLayerList();
        }
        
        private void SetupLayerList()
        {
            layerList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("layers"),
                true, true, true, true
            );
            
            layerList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Motion Layers");
            };
            
            layerList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = layerList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                
                Rect nameRect = new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(nameRect, element.FindPropertyRelative("layerName"), GUIContent.none);
                
                Rect intensityRect = new Rect(rect.x + rect.width * 0.32f, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(intensityRect, element.FindPropertyRelative("intensity"), GUIContent.none);
                
                Rect speedRect = new Rect(rect.x + rect.width * 0.55f, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(speedRect, element.FindPropertyRelative("speed"), GUIContent.none);
                
                Rect curveRect = new Rect(rect.x + rect.width * 0.78f, rect.y, rect.width * 0.22f, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(curveRect, "Edit Curves"))
                {
                    CurveEditorWindow.ShowWindow(element);
                }
            };
            
            layerList.elementHeightCallback = (int index) => {
                return EditorGUIUtility.singleLineHeight + 4;
            };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom Motion Profile", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Main Motion", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("yCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zCurve"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Curve Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("curveDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("randomizeStart"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Layers", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Layers add complexity and organic feel to motion", MessageType.Info);
            layerList.DoLayoutList();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Secondary Motion", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useSecondaryMotion"));
            if (profile.useSecondaryMotion)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryIntensity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryXCurve"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryYCurve"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryZCurve"));
            }
            
            EditorGUILayout.Space();
            DrawPreviewSection();
            
            EditorGUILayout.Space();
            DrawPresetButtons();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawPreviewSection()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("▶ Play Preview"))
                {
                    showCurvePreview = true;
                }
                if (GUILayout.Button("■ Stop"))
                {
                    showCurvePreview = false;
                }
                if (GUILayout.Button("↺ Reset"))
                {
                    previewTime = 0f;
                }
            }
            
            if (showCurvePreview)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("previewSpeed"));
                
                previewTime += Time.deltaTime * profile.previewSpeed;
                previewValue = profile.Evaluate(previewTime);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector3Field("Current Value", previewValue);
                EditorGUI.EndDisabledGroup();
                
                Rect previewRect = GUILayoutUtility.GetRect(200, 100);
                DrawMotionPathPreview(previewRect);
                
                if (Event.current.type == EventType.Repaint)
                {
                    Repaint();
                }
            }
        }
        
        private void DrawMotionPathPreview(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
            
            Handles.color = Color.gray;
            for (int i = 0; i < 10; i++)
            {
                float x = rect.x + (rect.width / 10) * i;
                float y = rect.y + (rect.height / 10) * i;
                Handles.DrawLine(new Vector3(x, rect.y, 0), new Vector3(x, rect.y + rect.height, 0));
                Handles.DrawLine(new Vector3(rect.x, y, 0), new Vector3(rect.x + rect.width, y, 0));
            }
            
            Vector3 center = new Vector3(rect.x + rect.width / 2, rect.y + rect.height / 2, 0);
            float scale = Mathf.Min(rect.width, rect.height) * 0.4f;
            
            Vector3 pos = center + new Vector3(
                previewValue.x * scale,
                previewValue.y * scale,
                0
            );
            
            Handles.color = Color.cyan;
            Handles.DrawSolidDisc(pos, Vector3.forward, 5);
        }
        
        private void DrawPresetButtons()
        {
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
            
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Subtle Sway"))
                {
                    SetCurvePreset(0.3f, 0.2f, 0.3f);
                }
                if (GUILayout.Button("Organic Wave"))
                {
                    SetCurvePreset(1.5f, 1f, 1.2f);
                }
                if (GUILayout.Button("Dynamic Dance"))
                {
                    SetCurvePreset(3f, 2f, 2.5f);
                }
                if (GUILayout.Button("Chaotic"))
                {
                    SetCurvePreset(4f, 3f, 3.5f);
                }
            }
            
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Figure 8"))
                {
                    SetFigure8Preset();
                }
                if (GUILayout.Button("Spiral"))
                {
                    SetSpiralPreset();
                }
                if (GUILayout.Button("Breathing"))
                {
                    SetBreathingPreset();
                }
                if (GUILayout.Button("Clear"))
                {
                    profile.ResetCurves();
                }
            }
        }
        
        private void SetCurvePreset(float xAmp, float yAmp, float zAmp)
        {
            profile.xCurve = CreateSmoothCurve(xAmp);
            profile.yCurve = CreateSmoothCurve(yAmp);
            profile.zCurve = CreateSmoothCurve(zAmp);
            profile.curveDuration = 2f;
            EditorUtility.SetDirty(profile);
        }
        
        private void SetFigure8Preset()
        {
            AnimationCurve x = new AnimationCurve();
            x.AddKey(0, 0);
            x.AddKey(0.25f, 2);
            x.AddKey(0.5f, 0);
            x.AddKey(0.75f, -2);
            x.AddKey(1, 0);
            
            AnimationCurve y = new AnimationCurve();
            y.AddKey(0, 2);
            y.AddKey(0.25f, 0);
            y.AddKey(0.5f, -2);
            y.AddKey(0.75f, 0);
            y.AddKey(1, 2);
            
            profile.xCurve = x;
            profile.yCurve = y;
            profile.zCurve = AnimationCurve.Linear(0, 0, 1, 0);
            profile.curveDuration = 3f;
            EditorUtility.SetDirty(profile);
        }
        
        private void SetSpiralPreset()
        {
            AnimationCurve x = new AnimationCurve();
            x.AddKey(0, 0);
            x.AddKey(0.5f, 3);
            x.AddKey(1, 0);
            
            AnimationCurve y = new AnimationCurve();
            y.AddKey(0, 0);
            y.AddKey(0.5f, 0);
            y.AddKey(1, 3);
            
            AnimationCurve z = new AnimationCurve();
            z.AddKey(0, 0);
            z.AddKey(0.5f, 2);
            z.AddKey(1, -2);
            
            profile.xCurve = x;
            profile.yCurve = y;
            profile.zCurve = z;
            profile.curveDuration = 4f;
            EditorUtility.SetDirty(profile);
        }
        
        private void SetBreathingPreset()
        {
            AnimationCurve y = new AnimationCurve();
            y.AddKey(0, 0);
            y.AddKey(0.25f, 1);
            y.AddKey(0.5f, 0);
            y.AddKey(0.75f, -0.5f);
            y.AddKey(1, 0);
            
            AnimationCurve scale = new AnimationCurve();
            scale.AddKey(0, 1);
            scale.AddKey(0.25f, 1.2f);
            scale.AddKey(0.5f, 1);
            scale.AddKey(0.75f, 0.9f);
            scale.AddKey(1, 1);
            
            profile.xCurve = AnimationCurve.Linear(0, 0, 1, 0);
            profile.yCurve = y;
            profile.zCurve = AnimationCurve.Linear(0, 0, 1, 0);
            
            profile.useSecondaryMotion = true;
            profile.secondaryIntensity = 0.2f;
            profile.secondaryXCurve = scale;
            profile.secondaryYCurve = scale;
            profile.secondaryZCurve = scale;
            
            profile.curveDuration = 2f;
            EditorUtility.SetDirty(profile);
        }
        
        private AnimationCurve CreateSmoothCurve(float amplitude)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.25f, amplitude);
            curve.AddKey(0.5f, 0);
            curve.AddKey(0.75f, -amplitude);
            curve.AddKey(1, 0);
            
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            
            return curve;
        }
    }
}
#endif