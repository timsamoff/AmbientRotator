#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace AmbientRotator
{
    public class CreatePresets : EditorWindow
    {
        [MenuItem("Tools/Ambient Rotator/Create Example Presets")]
        public static void CreateExamplePresets()
        {
            string presetPath = Application.dataPath + "/AmbientRotator/Examples/Presets";
            Directory.CreateDirectory(presetPath);

            // 1. Nature Preset
            AmbientPreset nature = ScriptableObject.CreateInstance<AmbientPreset>();
            nature.profile = MotionProfile.Organic;
            nature.intensity = 1.2f;
            nature.speed = 0.7f;
            nature.maxAngle = new Vector3(20, 15, 20);
            nature.responsiveToWind = true;
            nature.windMultiplier = 1.5f;
            nature.gizmoColor = new Color(0.2f, 0.8f, 0.2f);
            SavePreset(nature, presetPath + "/Nature.asset");

            // 2. UI Preset
            AmbientPreset ui = ScriptableObject.CreateInstance<AmbientPreset>();
            ui.profile = MotionProfile.Subtle;
            ui.intensity = 0.4f;
            ui.speed = 0.3f;
            ui.maxAngle = new Vector3(5, 5, 5);
            ui.responsiveToWind = false;
            ui.responsiveToPlayer = false;
            ui.gizmoColor = new Color(0.2f, 0.4f, 0.8f);
            SavePreset(ui, presetPath + "/UI.asset");

            // 3. Fantasy Preset
            AmbientPreset fantasy = ScriptableObject.CreateInstance<AmbientPreset>();
            fantasy.profile = MotionProfile.Dynamic;
            fantasy.intensity = 1.8f;
            fantasy.speed = 1.5f;
            fantasy.maxAngle = new Vector3(35, 35, 35);
            fantasy.responsiveToWind = true;
            fantasy.windMultiplier = 0.5f;
            fantasy.gizmoColor = new Color(0.8f, 0.2f, 0.8f);
            SavePreset(fantasy, presetPath + "/Fantasy.asset");

            // 4. SciFi Preset
            AmbientPreset scifi = ScriptableObject.CreateInstance<AmbientPreset>();
            scifi.profile = MotionProfile.Chaotic;
            scifi.intensity = 1.0f;
            scifi.speed = 2.0f;
            scifi.maxAngle = new Vector3(45, 45, 45);
            scifi.responsiveToWind = false;
            scifi.responsiveToPlayer = true;
            scifi.playerInfluenceRadius = 5f;
            scifi.gizmoColor = new Color(0.2f, 0.8f, 0.8f);
            SavePreset(scifi, presetPath + "/SciFi.asset");

            AssetDatabase.Refresh();
            Debug.Log("Example presets created in: " + presetPath);
        }

        private static void SavePreset(AmbientPreset preset, string path)
        {
            // Convert absolute path to relative path for Unity
            string relativePath = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.CreateAsset(preset, relativePath);
            Debug.Log($"Created: {relativePath}");
        }

        [MenuItem("Tools/Ambient Rotator/Create Preset From Current Selection")]
        public static void CreatePresetFromSelection()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning("Select a GameObject with an AmbientRotator first.");
                return;
            }

            AmbientRotator rotator = Selection.activeGameObject.GetComponent<AmbientRotator>();
            if (rotator == null)
            {
                Debug.LogWarning("Selected object doesn't have an AmbientRotator.");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Preset",
                "NewPreset",
                "asset",
                "Save preset from current settings"
            );

            if (string.IsNullOrEmpty(path)) return;

            AmbientPreset preset = ScriptableObject.CreateInstance<AmbientPreset>();
            preset.profile = rotator.CurrentProfile;
            preset.intensity = rotator.CurrentIntensity;
            // Note: You may need to expose more properties in AmbientRotator
            // to save ALL settings
            
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = preset;
            
            Debug.Log($"Preset saved to: {path}");
        }
    }
}
#endif