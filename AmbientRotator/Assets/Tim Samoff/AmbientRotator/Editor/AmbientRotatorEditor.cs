#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AmbientRotator
{
    [CustomEditor(typeof(AmbientRotator))]
    public class AmbientRotatorEditor : Editor
    {
        private AmbientRotator rotator;

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

            // Apply preset values to the runtime object
            rotator.SetProfile(profile);

            // Force the Inspector to refresh
            serializedObject.Update();

            // Mark as dirty so Unity saves the changes
            EditorUtility.SetDirty(rotator);

            // Force a repaint of the Inspector
            Repaint();
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
            if (rotator == null || rotator.transform == null) return;

            // Draw a simple rotation indicator
            Handles.color = Color.cyan;
            Vector3 position = rotator.transform.position;

            // Draw small rings
            float ringSize = 0.5f;
            Handles.DrawWireDisc(position, Vector3.up, ringSize);
            Handles.DrawWireDisc(position, Vector3.right, ringSize);
            Handles.DrawWireDisc(position, Vector3.forward, ringSize);

            // Draw direction arrow
            Handles.color = Color.yellow;
            Handles.ArrowHandleCap(0, position, rotator.transform.rotation, 0.8f, EventType.Repaint);
        }
    }
}
#endif