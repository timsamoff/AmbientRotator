#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AmbientRotator
{
    [CustomEditor(typeof(AmbientRotator))]
    [CanEditMultipleObjects]
    public class AmbientRotatorEditor : Editor
    {
        private SerializedProperty profileProp;
        private SerializedProperty intensityProp;
        private SerializedProperty speedProp;
        private SerializedProperty phaseOffsetProp;
        private SerializedProperty maxAngleProp;
        private SerializedProperty clampMovementProp;
        private SerializedProperty decisionDurationProp;
        private SerializedProperty completeRotationProp;
        private SerializedProperty useStartDelayProp;
        private SerializedProperty startDelayMinProp;
        private SerializedProperty startDelayMaxProp;
        private SerializedProperty randomizeStartDelayProp;
        private SerializedProperty useCustomProfileProp;
        private SerializedProperty customProfileProp;
        private SerializedProperty blendProfilesProp;
        private SerializedProperty secondaryProfileProp;
        private SerializedProperty blendWeightProp;
        private SerializedProperty blendSpeedProp;
        private SerializedProperty influenceModeProp;
        private SerializedProperty externalDecayProp;
        private SerializedProperty maxExternalOffsetProp;
        private SerializedProperty updateMethodProp;
        private SerializedProperty useUnscaledTimeProp;
        private SerializedProperty autoStartProp;
        private SerializedProperty useLocalRotationProp;
        private SerializedProperty smoothTimeProp;
        private SerializedProperty maxSpeedProp;
        private SerializedProperty onRotationChangedProp;
        private SerializedProperty onMotionCompleteProp;
        private SerializedProperty onPauseProp;
        private SerializedProperty onResumeProp;

        private void OnEnable()
        {
            profileProp = serializedObject.FindProperty("profile");
            intensityProp = serializedObject.FindProperty("intensity");
            speedProp = serializedObject.FindProperty("speed");
            phaseOffsetProp = serializedObject.FindProperty("phaseOffset");
            maxAngleProp = serializedObject.FindProperty("maxAngle");
            clampMovementProp = serializedObject.FindProperty("clampMovement");
            decisionDurationProp = serializedObject.FindProperty("decisionDuration");
            completeRotationProp = serializedObject.FindProperty("completeRotation");
            useStartDelayProp = serializedObject.FindProperty("useStartDelay");
            startDelayMinProp = serializedObject.FindProperty("startDelayMin");
            startDelayMaxProp = serializedObject.FindProperty("startDelayMax");
            randomizeStartDelayProp = serializedObject.FindProperty("randomizeStartDelay");
            useCustomProfileProp = serializedObject.FindProperty("useCustomProfile");
            customProfileProp = serializedObject.FindProperty("customProfile");
            blendProfilesProp = serializedObject.FindProperty("blendProfiles");
            secondaryProfileProp = serializedObject.FindProperty("secondaryProfile");
            blendWeightProp = serializedObject.FindProperty("blendWeight");
            blendSpeedProp = serializedObject.FindProperty("blendSpeed");
            influenceModeProp = serializedObject.FindProperty("influenceMode");
            externalDecayProp = serializedObject.FindProperty("externalDecay");
            maxExternalOffsetProp = serializedObject.FindProperty("maxExternalOffset");
            updateMethodProp = serializedObject.FindProperty("updateMethod");
            useUnscaledTimeProp = serializedObject.FindProperty("useUnscaledTime");
            autoStartProp = serializedObject.FindProperty("autoStart");
            useLocalRotationProp = serializedObject.FindProperty("useLocalRotation");
            smoothTimeProp = serializedObject.FindProperty("smoothTime");
            maxSpeedProp = serializedObject.FindProperty("maxSpeed");
            onRotationChangedProp = serializedObject.FindProperty("OnRotationChanged");
            onMotionCompleteProp = serializedObject.FindProperty("OnMotionComplete");
            onPauseProp = serializedObject.FindProperty("OnPause");
            onResumeProp = serializedObject.FindProperty("OnResume");
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
                    ApplyPresetDirectly(MotionProfile.Subtle);
                }
                if (GUILayout.Button("Gentle", GUILayout.Height(25)))
                {
                    ApplyPresetDirectly(MotionProfile.Gentle);
                }
                if (GUILayout.Button("Organic", GUILayout.Height(25)))
                {
                    ApplyPresetDirectly(MotionProfile.Organic);
                }
                if (GUILayout.Button("Dynamic", GUILayout.Height(25)))
                {
                    ApplyPresetDirectly(MotionProfile.Dynamic);
                }
                if (GUILayout.Button("Chaotic", GUILayout.Height(25)))
                {
                    ApplyPresetDirectly(MotionProfile.Chaotic);
                }
            }
        }

        private void ApplyPresetDirectly(MotionProfile profile)
        {
            // Update the serialized property first
            profileProp.enumValueIndex = (int)profile;
            
            // Apply preset values directly to serialized properties
            switch (profile)
            {
                case MotionProfile.Subtle:
                    intensityProp.floatValue = 0.3f;
                    speedProp.floatValue = 0.3f;
                    decisionDurationProp.floatValue = 4f;
                    maxAngleProp.vector3Value = new Vector3(30f, 30f, 30f);
                    clampMovementProp.boolValue = true;
                    smoothTimeProp.floatValue = 0.8f;
                    maxSpeedProp.floatValue = 50f;
                    break;
                    
                case MotionProfile.Gentle:
                    intensityProp.floatValue = 0.6f;
                    speedProp.floatValue = 0.5f;
                    decisionDurationProp.floatValue = 3f;
                    maxAngleProp.vector3Value = new Vector3(60f, 60f, 60f);
                    clampMovementProp.boolValue = true;
                    smoothTimeProp.floatValue = 0.6f;
                    maxSpeedProp.floatValue = 80f;
                    break;
                    
                case MotionProfile.Organic:
                    intensityProp.floatValue = 1.0f;
                    speedProp.floatValue = 0.7f;
                    decisionDurationProp.floatValue = 2.5f;
                    maxAngleProp.vector3Value = new Vector3(90f, 90f, 90f);
                    clampMovementProp.boolValue = true;
                    smoothTimeProp.floatValue = 0.4f;
                    maxSpeedProp.floatValue = 120f;
                    break;
                    
                case MotionProfile.Dynamic:
                    intensityProp.floatValue = 1.5f;
                    speedProp.floatValue = 1.0f;
                    decisionDurationProp.floatValue = 2f;
                    maxAngleProp.vector3Value = new Vector3(180f, 180f, 180f);
                    clampMovementProp.boolValue = false;
                    smoothTimeProp.floatValue = 0.3f;
                    maxSpeedProp.floatValue = 150f;
                    break;
                    
                case MotionProfile.Chaotic:
                    intensityProp.floatValue = 2.0f;
                    speedProp.floatValue = 1.5f;
                    decisionDurationProp.floatValue = 1.5f;
                    maxAngleProp.vector3Value = new Vector3(360f, 360f, 360f);
                    clampMovementProp.boolValue = false;
                    smoothTimeProp.floatValue = 0.2f;
                    maxSpeedProp.floatValue = 200f;
                    break;
                    
                case MotionProfile.Custom:
                default:
                    break;
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Also apply to the runtime object for immediate effect
            foreach (var targetObj in targets)
            {
                var rotator = targetObj as AmbientRotator;
                if (rotator != null)
                {
                    rotator.SetProfile(profile);
                    EditorUtility.SetDirty(rotator);
                }
            }
            
            Repaint();
        }

        private void DrawCoreSettings()
        {
            EditorGUILayout.LabelField("Core Settings", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(profileProp);
            if (EditorGUI.EndChangeCheck())
            {
                // If user changed the dropdown, apply the preset
                int selected = profileProp.enumValueIndex;
                if (selected != (int)MotionProfile.Custom)
                {
                    ApplyPresetDirectly((MotionProfile)selected);
                }
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(intensityProp);
            EditorGUILayout.PropertyField(speedProp);
            EditorGUILayout.PropertyField(phaseOffsetProp);
            if (EditorGUI.EndChangeCheck())
            {
                if (profileProp.enumValueIndex != (int)MotionProfile.Custom)
                {
                    profileProp.enumValueIndex = (int)MotionProfile.Custom;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawMovementSettings()
        {
            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(maxAngleProp);
            EditorGUILayout.PropertyField(clampMovementProp);
            EditorGUILayout.PropertyField(decisionDurationProp);
            EditorGUILayout.PropertyField(completeRotationProp);
            EditorGUILayout.PropertyField(smoothTimeProp);
            EditorGUILayout.PropertyField(maxSpeedProp);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (profileProp.enumValueIndex != (int)MotionProfile.Custom)
                {
                    profileProp.enumValueIndex = (int)MotionProfile.Custom;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawStartDelaySettings()
        {
            EditorGUILayout.LabelField("Start Delay", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(useStartDelayProp);

            if (useStartDelayProp.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(randomizeStartDelayProp);

                if (randomizeStartDelayProp.boolValue)
                {
                    EditorGUILayout.PropertyField(startDelayMinProp, new GUIContent("Min Delay"));
                    EditorGUILayout.PropertyField(startDelayMaxProp, new GUIContent("Max Delay"));
                }
                else
                {
                    EditorGUILayout.PropertyField(startDelayMinProp, new GUIContent("Delay"));
                }

                EditorGUI.indentLevel--;
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                if (profileProp.enumValueIndex != (int)MotionProfile.Custom)
                {
                    profileProp.enumValueIndex = (int)MotionProfile.Custom;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawCustomProfileSettings()
        {
            EditorGUILayout.LabelField("Custom Profile", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            
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

                EditorGUI.indentLevel--;
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                if (profileProp.enumValueIndex != (int)MotionProfile.Custom)
                {
                    profileProp.enumValueIndex = (int)MotionProfile.Custom;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawAdvancedSettings()
        {
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(updateMethodProp);
            EditorGUILayout.PropertyField(useUnscaledTimeProp);
            EditorGUILayout.PropertyField(autoStartProp);
            EditorGUILayout.PropertyField(useLocalRotationProp);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (profileProp.enumValueIndex != (int)MotionProfile.Custom)
                {
                    profileProp.enumValueIndex = (int)MotionProfile.Custom;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawEvents()
        {
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onRotationChangedProp);
            EditorGUILayout.PropertyField(onMotionCompleteProp);
            EditorGUILayout.PropertyField(onPauseProp);
            EditorGUILayout.PropertyField(onResumeProp);
        }

        private void DrawDebugInfo()
        {
            EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            
            if (targets.Length == 1)
            {
                var rotator = target as AmbientRotator;
                if (rotator != null)
                {
                    EditorGUILayout.Toggle("Is Paused", rotator.IsPaused);
                    EditorGUILayout.Vector3Field("Current Offset", rotator.CurrentOffset);
                    EditorGUILayout.LabelField("Profile", rotator.CurrentProfile.ToString());
                    EditorGUILayout.LabelField("Intensity", rotator.CurrentIntensity.ToString("F2"));
                }
            }
            else
            {
                EditorGUILayout.LabelField("Is Paused", "Multiple objects selected");
                EditorGUILayout.LabelField("Current Offset", "Multiple objects selected");
                EditorGUILayout.LabelField("Profile", "Multiple objects selected");
                EditorGUILayout.LabelField("Intensity", "Multiple objects selected");
            }
            
            EditorGUI.EndDisabledGroup();
        }

        private void OnSceneGUI()
        {
            var rotator = target as AmbientRotator;
            if (rotator == null || rotator.transform == null) return;

            Handles.color = Color.cyan;
            Vector3 position = rotator.transform.position;

            float ringSize = 0.5f;
            Handles.DrawWireDisc(position, Vector3.up, ringSize);
            Handles.DrawWireDisc(position, Vector3.right, ringSize);
            Handles.DrawWireDisc(position, Vector3.forward, ringSize);

            Handles.color = Color.yellow;
            Handles.ArrowHandleCap(0, position, rotator.transform.rotation, 0.8f, EventType.Repaint);
        }
    }
}
#endif