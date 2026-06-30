#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AmbientRotator
{
    [CustomEditor(typeof(WindSystemModule))]
    public class WindSystemModuleEditor : Editor
    {
        private SerializedProperty baseStrength;
        private SerializedProperty gustFrequency;
        private SerializedProperty gustAmplitude;
        private SerializedProperty baseDirection;
        private SerializedProperty directionX;
        private SerializedProperty directionY;
        private SerializedProperty directionZ;
        private SerializedProperty turbulence;
        private SerializedProperty turbulenceFrequency;
        private SerializedProperty turbulenceScale;
        private SerializedProperty maxProcessPerFrame;
        private SerializedProperty autoFindObjects;
        private SerializedProperty maxUpdateDistance;

        private void OnEnable()
        {
            baseStrength = serializedObject.FindProperty("baseStrength");
            gustFrequency = serializedObject.FindProperty("gustFrequency");
            gustAmplitude = serializedObject.FindProperty("gustAmplitude");
            baseDirection = serializedObject.FindProperty("baseDirection");
            directionX = serializedObject.FindProperty("directionX");
            directionY = serializedObject.FindProperty("directionY");
            directionZ = serializedObject.FindProperty("directionZ");
            turbulence = serializedObject.FindProperty("turbulence");
            turbulenceFrequency = serializedObject.FindProperty("turbulenceFrequency");
            turbulenceScale = serializedObject.FindProperty("turbulenceScale");
            maxProcessPerFrame = serializedObject.FindProperty("maxProcessPerFrame");
            autoFindObjects = serializedObject.FindProperty("autoFindObjects");
            maxUpdateDistance = serializedObject.FindProperty("maxUpdateDistance");
        }

        public override void OnInspectorGUI()
        {
            Debug.Log("WindSystemModuleEditor is running!");
            
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wind Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(baseStrength);
            EditorGUILayout.PropertyField(gustFrequency);
            EditorGUILayout.PropertyField(gustAmplitude);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wind Direction", EditorStyles.boldLabel);
            
            // Show the individual axis sliders
            EditorGUILayout.PropertyField(directionX, new GUIContent("X (Left/Right)"));
            EditorGUILayout.PropertyField(directionY, new GUIContent("Y (Down/Up)"));
            EditorGUILayout.PropertyField(directionZ, new GUIContent("Z (Back/Forward)"));
            
            // Show the resulting direction vector (read-only display)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Resulting Direction", "The combined direction vector from the axis values above."));
            GUI.enabled = false;
            
            // Display as a Vector3 field but disabled
            Vector3 dir = baseDirection.vector3Value;
            EditorGUILayout.Vector3Field("", dir);
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // Hide the actual baseDirection field from the Inspector
            // by not calling EditorGUILayout.PropertyField for it

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Turbulence", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(turbulence);
            EditorGUILayout.PropertyField(turbulenceFrequency);
            EditorGUILayout.PropertyField(turbulenceScale);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(maxProcessPerFrame);
            EditorGUILayout.PropertyField(autoFindObjects);
            EditorGUILayout.PropertyField(maxUpdateDistance);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif