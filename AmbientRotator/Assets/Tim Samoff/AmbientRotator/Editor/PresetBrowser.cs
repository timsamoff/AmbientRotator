#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace AmbientRotator
{
    public class PresetBrowser : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<AmbientPreset> presets = new List<AmbientPreset>();
        private string[] presetPaths;
        private int selectedPresetIndex = -1;
        // Removed unused field: private bool showPreview = false;
        
        [MenuItem("Window/Ambient Rotator/Preset Browser")]
        public static void ShowWindow()
        {
            GetWindow<PresetBrowser>("Preset Browser");
        }
        
        private void OnEnable()
        {
            LoadPresets();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space();
            
            GUILayout.Label("Ambient Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
            EditorGUILayout.TextField("", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("+ Create New Preset", GUILayout.Height(30)))
            {
                CreateNewPreset();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("Available Presets", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            if (presets.Count == 0)
            {
                EditorGUILayout.HelpBox("No presets found. Create one!", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < presets.Count; i++)
                {
                    DrawPresetItem(i);
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            if (selectedPresetIndex >= 0 && selectedPresetIndex < presets.Count)
            {
                DrawSelectedPresetInfo();
            }
            
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(selectedPresetIndex < 0 || Selection.activeGameObject == null);
            if (GUILayout.Button("Apply to Selected", GUILayout.Height(35)))
            {
                ApplyPresetToSelected();
            }
            EditorGUI.EndDisabledGroup();
        }
        
        private void LoadPresets()
        {
            presets.Clear();
            presetPaths = AssetDatabase.FindAssets("t:AmbientPreset");
            
            foreach (string guid in presetPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var preset = AssetDatabase.LoadAssetAtPath<AmbientPreset>(path);
                if (preset != null)
                {
                    presets.Add(preset);
                }
            }
        }
        
        private void DrawPresetItem(int index)
        {
            var preset = presets[index];
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            
            bool isSelected = (selectedPresetIndex == index);
            if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)))
            {
                selectedPresetIndex = index;
            }
            
            EditorGUILayout.BeginVertical();
            GUILayout.Label(preset.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Profile: {preset.profile}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"Intensity: {preset.intensity:F1}, Speed: {preset.speed:F1}");
            EditorGUILayout.EndVertical();
            
            if (GUILayout.Button("Preview", GUILayout.Width(60)))
            {
                ApplyPresetToSelected(preset);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSelectedPresetInfo()
        {
            var preset = presets[selectedPresetIndex];
            
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Selected Preset Details", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Preset", preset, typeof(AmbientPreset), false);
            EditorGUILayout.EnumPopup("Profile", preset.profile);
            EditorGUILayout.FloatField("Intensity", preset.intensity);
            EditorGUILayout.FloatField("Speed", preset.speed);
            EditorGUILayout.Vector3Field("Max Angle", preset.maxAngle);
            EditorGUILayout.Toggle("Clamp Movement", preset.clampMovement);
            EditorGUILayout.Toggle("Responsive to Wind", preset.responsiveToWind);
            EditorGUILayout.Toggle("Responsive to Player", preset.responsiveToPlayer);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
        }
        
        private void ApplyPresetToSelected(AmbientPreset specificPreset = null)
        {
            if (Selection.activeGameObject == null) return;
            
            AmbientPreset preset = specificPreset ?? presets[selectedPresetIndex];
            if (preset == null) return;
            
            var rotators = Selection.activeGameObject.GetComponentsInChildren<AmbientRotator>();
            foreach (var rotator in rotators)
            {
                preset.ApplyTo(rotator);
                EditorUtility.SetDirty(rotator);
            }
            
            var groups = Selection.activeGameObject.GetComponentsInChildren<AmbientGroup>();
            foreach (var group in groups)
            {
                preset.ApplyToGroup(group);
                EditorUtility.SetDirty(group);
            }
            
            Debug.Log($"Applied preset '{preset.name}' to {rotators.Length} rotators and {groups.Length} groups.");
        }
        
        private void CreateNewPreset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Preset",
                "NewAmbientPreset",
                "asset",
                "Choose where to save the new preset"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                var preset = ScriptableObject.CreateInstance<AmbientPreset>();
                AssetDatabase.CreateAsset(preset, path);
                AssetDatabase.SaveAssets();
                LoadPresets();
                
                selectedPresetIndex = presets.Count - 1;
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = preset;
            }
        }
        
        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
#endif