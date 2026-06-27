#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AmbientRotator
{
    [CustomEditor(typeof(BeatSyncModule))]
    public class BeatSyncModuleEditor : Editor
    {
        private BeatSyncModule module;

        private void OnEnable()
        {
            module = (BeatSyncModule)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Beat Sync Module", EditorStyles.boldLabel);

            // Audio Source
            EditorGUILayout.PropertyField(serializedObject.FindProperty("musicSource"));

            EditorGUILayout.Space();

            // Reaction Intensity
            EditorGUILayout.LabelField("Reaction Intensity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("beatReactionIntensity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("beatSmoothing"));

            EditorGUILayout.Space();

            // Beat Detection
            EditorGUILayout.LabelField("Beat Detection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minBeatThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxBeatThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spectrumSamples"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("beatCooldown"));

            EditorGUILayout.Space();

            // Reaction Types - Clean Layout (No extra headings)
            EditorGUILayout.LabelField("Reaction Types", EditorStyles.boldLabel);

            // Pulse
            SerializedProperty pulseProp = serializedObject.FindProperty("pulse");
            EditorGUILayout.PropertyField(pulseProp);
            if (pulseProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pulseHeight"));
                EditorGUI.indentLevel--;
            }

            // Rotate
            SerializedProperty rotateProp = serializedObject.FindProperty("rotate");
            EditorGUILayout.PropertyField(rotateProp);
            if (rotateProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationForceMultiplier"));
                EditorGUI.indentLevel--;
            }

            // Wobble
            SerializedProperty wobbleProp = serializedObject.FindProperty("wobble");
            EditorGUILayout.PropertyField(wobbleProp);
            if (wobbleProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("wobbleFrequency"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif