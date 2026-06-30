#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AmbientRotator
{
    [CustomEditor(typeof(BeatSyncModule))]
    public class BeatSyncModuleEditor : Editor
    {
        private SerializedProperty musicSource;
        private SerializedProperty beatReactionIntensity;
        private SerializedProperty beatSmoothing;
        private SerializedProperty minBeatThreshold;
        private SerializedProperty maxBeatThreshold;
        private SerializedProperty spectrumSamples;
        private SerializedProperty reaction;
        private SerializedProperty debugLogging;

        private void OnEnable()
        {
            musicSource = serializedObject.FindProperty("musicSource");
            beatReactionIntensity = serializedObject.FindProperty("beatReactionIntensity");
            beatSmoothing = serializedObject.FindProperty("beatSmoothing");
            minBeatThreshold = serializedObject.FindProperty("minBeatThreshold");
            maxBeatThreshold = serializedObject.FindProperty("maxBeatThreshold");
            spectrumSamples = serializedObject.FindProperty("spectrumSamples");
            reaction = serializedObject.FindProperty("reaction");
            debugLogging = serializedObject.FindProperty("debugLogging");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Beat Sync Module", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(musicSource);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Reaction Intensity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(beatReactionIntensity, new GUIContent("Beat Intensity (0-100)"));
            EditorGUILayout.PropertyField(beatSmoothing, new GUIContent("Smoothing"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Beat Detection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(minBeatThreshold);
            EditorGUILayout.PropertyField(maxBeatThreshold);
            EditorGUILayout.PropertyField(spectrumSamples);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Reaction Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Pulse: Jump height (0-5) + Intensity (0-100)\n" +
                "Rotation: Max angle (0-360°) + Intensity (0-100)\n" +
                "Rotation Axis: (0,1,0) for Y-axis spin\n" +
                "Wobble: Amount (0-5) + Intensity (0-100) + Speed", 
                MessageType.Info);
            
            EditorGUILayout.PropertyField(reaction, new GUIContent("Reaction Settings"), true);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(debugLogging);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif