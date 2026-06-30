#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AmbientRotator
{
    [CustomEditor(typeof(ReactiveTriggerModule))]
    public class ReactiveTriggerModuleEditor : Editor
    {
        private SerializedProperty reactionRadius;
        private SerializedProperty triggerObjects;
        private SerializedProperty reaction;
        private SerializedProperty pushAway;
        private SerializedProperty pushAwayStrength;
        private SerializedProperty attract;
        private SerializedProperty attractStrength;
        private SerializedProperty showGizmos;
        private SerializedProperty gizmoColor;
        private SerializedProperty debugLogging;

        private void OnEnable()
        {
            reactionRadius = serializedObject.FindProperty("reactionRadius");
            triggerObjects = serializedObject.FindProperty("triggerObjects");
            reaction = serializedObject.FindProperty("reaction");
            pushAway = serializedObject.FindProperty("pushAway");
            pushAwayStrength = serializedObject.FindProperty("pushAwayStrength");
            attract = serializedObject.FindProperty("attract");
            attractStrength = serializedObject.FindProperty("attractStrength");
            showGizmos = serializedObject.FindProperty("showGizmos");
            gizmoColor = serializedObject.FindProperty("gizmoColor");
            debugLogging = serializedObject.FindProperty("debugLogging");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Reactive Trigger", EditorStyles.boldLabel);

            // Trigger Settings
            EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(reactionRadius);
            EditorGUILayout.PropertyField(triggerObjects, new GUIContent("Trigger Objects"), true);

            EditorGUILayout.Space();

            // Reaction Configuration (shared with BeatSyncModule)
            EditorGUILayout.LabelField("Reaction Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(reaction, new GUIContent("Reaction Settings"), true);

            EditorGUILayout.Space();

            // Push/Attract (unique to ReactiveTrigger)
            EditorGUILayout.LabelField("Push/Attract", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(pushAway);
            if (pushAway.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(pushAwayStrength);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(attract);
            if (attract.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(attractStrength);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Debug
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showGizmos);
            EditorGUILayout.PropertyField(gizmoColor);
            EditorGUILayout.PropertyField(debugLogging);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif