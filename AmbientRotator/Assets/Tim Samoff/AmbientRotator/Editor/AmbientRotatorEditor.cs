#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AmbientRotator
{
    [CustomEditor(typeof(AmbientRotator))]
    public class AmbientRotatorEditor : Editor
    {
        private AmbientRotator rotator;
        private bool showPreview = false;
        private float previewSpeed = 1f;
        
        private void OnEnable()
        {
            rotator = (AmbientRotator)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ambient Rotator", EditorStyles.boldLabel);
            
            DrawQuickPresets();
            
            EditorGUILayout.Space();
            
            DrawCoreSettings();
            
            EditorGUILayout.Space();
            
            DrawMovementSettings();
            
            EditorGUILayout.Space();
            
            DrawCustomProfileSettings();
            
            EditorGUILayout.Space();
            
            DrawAdvancedSettings();
            
            EditorGUILayout.Space();
            
            DrawEvents();
            
            EditorGUILayout.Space();
            
            DrawPreviewControls();
            
            EditorGUILayout.Space();
            
            DrawDebugInfo();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawQuickPresets()
        {
            GUILayout.Label("Quick Presets", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Subtle", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Subtle, 0.5f, 0.5f);
                }
                if (GUILayout.Button("Gentle", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Gentle, 1f, 1f);
                }
                if (GUILayout.Button("Organic", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Organic, 1.5f, 0.8f);
                }
                if (GUILayout.Button("Dynamic", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Dynamic, 1.2f, 1.5f);
                }
                if (GUILayout.Button("Chaotic", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Chaotic, 2f, 2f);
                }
            }
        }
        
        private void ApplyQuickPreset(MotionProfile profile, float intensity, float speed)
        {
            var profileProp = serializedObject.FindProperty("profile");
            var intensityProp = serializedObject.FindProperty("intensity");
            var speedProp = serializedObject.FindProperty("speed");
            
            profileProp.enumValueIndex = (int)profile;
            intensityProp.floatValue = intensity;
            speedProp.floatValue = speed;
            
            serializedObject.ApplyModifiedProperties();
            
            if (EditorApplication.isPlaying)
            {
                rotator.SetProfile(profile);
                rotator.SetIntensity(intensity);
                rotator.SetSpeed(speed);
            }
            
            EditorUtility.SetDirty(rotator);
        }
        
        private void DrawCoreSettings()
        {
            EditorGUILayout.LabelField("Core Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("profile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intensity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("phaseOffset"));
        }
        
        private void DrawMovementSettings()
        {
            EditorGUILayout.LabelField("Movement Limits", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clampMovement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSpeed"));
        }
        
        private void DrawCustomProfileSettings()
        {
            EditorGUILayout.LabelField("Custom Profile", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useCustomProfile"));
            
            if (rotator.useCustomProfile)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customProfile"));
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Profile Blending", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blendProfiles"));
                
                if (rotator.blendProfiles)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryProfile"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blendWeight"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blendSpeed"));
                }
                
                if (GUILayout.Button("Open Profile Library", GUILayout.Height(25)))
                {
                    ProfileSelector.ShowWindow();
                }
            }
        }
        
        private void DrawAdvancedSettings()
        {
            EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateMethod"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useUnscaledTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useLocalRotation"));
        }
        
        private void DrawEvents()
        {
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnRotationChanged"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnMotionComplete"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPause"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnResume"));
        }
        
        private void DrawPreviewControls()
        {
            EditorGUILayout.LabelField("Preview Controls", EditorStyles.boldLabel);
            
            using (new GUILayout.HorizontalScope())
            {
                if (!showPreview)
                {
                    if (GUILayout.Button("▶ Preview", GUILayout.Height(30)))
                    {
                        showPreview = true;
                        rotator.StartPreview();
                    }
                }
                else
                {
                    if (GUILayout.Button("■ Stop Preview", GUILayout.Height(30)))
                    {
                        showPreview = false;
                        rotator.EndPreview();
                    }
                }
                
                EditorGUILayout.Space();
                
                previewSpeed = EditorGUILayout.FloatField("Speed", previewSpeed);
            }
            
            if (showPreview)
            {
                EditorGUILayout.HelpBox("Preview active. Click 'Stop Preview' to reset.", MessageType.Info);
            }
        }
        
        private void DrawDebugInfo()
        {
            EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Is Paused", rotator.IsPaused);
            EditorGUILayout.Vector3Field("Current Offset", rotator.CurrentOffset);
            EditorGUILayout.LabelField("Profile", rotator.CurrentProfile.ToString());
            EditorGUILayout.LabelField("Intensity", rotator.CurrentIntensity.ToString("F2"));
            EditorGUI.EndDisabledGroup();
        }
        
        private void OnSceneGUI()
        {
            Handles.color = Color.cyan;
            
            Vector3 position = rotator.transform.position;
            float radius = 0.5f;
            
            Handles.DrawWireArc(position, Vector3.up, Vector3.forward, 360, radius);
            Handles.DrawWireArc(position, Vector3.right, Vector3.up, 360, radius);
            Handles.DrawWireArc(position, Vector3.forward, Vector3.right, 360, radius);
            
            Handles.color = new Color(1f, 0.5f, 0f, 0.3f);
            float angleRadius = rotator.maxAngle.magnitude * 0.02f;
            Handles.DrawSolidArc(position, Vector3.up, Vector3.forward, rotator.maxAngle.x, angleRadius);
        }
    }
}
#endif