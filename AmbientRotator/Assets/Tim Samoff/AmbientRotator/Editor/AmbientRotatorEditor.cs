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
            
            DrawStartDelaySettings();
            
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
                    ApplyQuickPreset(MotionProfile.Subtle);
                }
                if (GUILayout.Button("Gentle", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Gentle);
                }
                if (GUILayout.Button("Organic", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Organic);
                }
                if (GUILayout.Button("Dynamic", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Dynamic);
                }
                if (GUILayout.Button("Chaotic", GUILayout.Height(25)))
                {
                    ApplyQuickPreset(MotionProfile.Chaotic);
                }
            }
        }
        
        private void ApplyQuickPreset(MotionProfile profile)
        {
            var profileProp = serializedObject.FindProperty("profile");
            profileProp.enumValueIndex = (int)profile;
            serializedObject.ApplyModifiedProperties();
            
            // The preset values will be applied by OnValidate in the runtime script
            if (EditorApplication.isPlaying)
            {
                rotator.SetProfile(profile);
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
            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clampMovement"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("decisionDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("completeRotation"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSpeed"));
        }
        
        private void DrawStartDelaySettings()
        {
            EditorGUILayout.LabelField("Start Delay", EditorStyles.boldLabel);
            
            SerializedProperty useDelay = serializedObject.FindProperty("useStartDelay");
            SerializedProperty minDelay = serializedObject.FindProperty("startDelayMin");
            SerializedProperty maxDelay = serializedObject.FindProperty("startDelayMax");
            SerializedProperty randomize = serializedObject.FindProperty("randomizeStartDelay");
            
            EditorGUILayout.PropertyField(useDelay);
            
            if (useDelay.boolValue)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(randomize);
                
                if (randomize.boolValue)
                {
                    EditorGUILayout.PropertyField(minDelay, new GUIContent("Min Delay"));
                    EditorGUILayout.PropertyField(maxDelay, new GUIContent("Max Delay"));
                }
                else
                {
                    EditorGUILayout.PropertyField(minDelay, new GUIContent("Delay"));
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawCustomProfileSettings()
        {
            EditorGUILayout.LabelField("Custom Profile", EditorStyles.boldLabel);
            
            SerializedProperty useCustomProfileProp = serializedObject.FindProperty("useCustomProfile");
            SerializedProperty customProfileProp = serializedObject.FindProperty("customProfile");
            SerializedProperty blendProfilesProp = serializedObject.FindProperty("blendProfiles");
            SerializedProperty secondaryProfileProp = serializedObject.FindProperty("secondaryProfile");
            SerializedProperty blendWeightProp = serializedObject.FindProperty("blendWeight");
            SerializedProperty blendSpeedProp = serializedObject.FindProperty("blendSpeed");
            
            EditorGUILayout.PropertyField(useCustomProfileProp);
            
            if (useCustomProfileProp.boolValue)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(customProfileProp);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Profile Blending", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(blendProfilesProp);
                
                if (blendProfilesProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(secondaryProfileProp);
                    EditorGUILayout.PropertyField(blendWeightProp);
                    EditorGUILayout.PropertyField(blendSpeedProp);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Open Profile Library", GUILayout.Height(25)))
                {
                    ProfileSelector.ShowWindow();
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawAdvancedSettings()
        {
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
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
            float angleRadius = rotator.MaxAngle.magnitude * 0.02f;
            Handles.DrawSolidArc(position, Vector3.up, Vector3.forward, rotator.MaxAngle.x, angleRadius);
        }
    }
}
#endif