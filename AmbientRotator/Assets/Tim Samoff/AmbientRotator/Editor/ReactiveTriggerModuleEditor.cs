#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AmbientRotator
{
    [CustomEditor(typeof(ReactiveTriggerModule))]
    public class ReactiveTriggerModuleEditor : Editor
    {
        private SerializedProperty sourceTag;
        private SerializedProperty debugLogging;

        private void OnEnable()
        {
            sourceTag = serializedObject.FindProperty("sourceTag");
            debugLogging = serializedObject.FindProperty("debugLogging");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Reactive Trigger Module (Listener)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Listens for nearby ReactiveTriggerObject sources.\n" +
                "The reaction is defined on the ReactiveTriggerObject.", 
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Filter Settings", EditorStyles.boldLabel);
            
            string currentTag = sourceTag.stringValue;
            string newTag = EditorGUILayout.TagField("Source Tag", currentTag);
            if (newTag != currentTag)
            {
                sourceTag.stringValue = newTag;
            }
            
            EditorGUILayout.HelpBox(
                "Leave 'Untagged' to react to all sources. Select a tag to only react to sources with that tag.", 
                MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(debugLogging);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif