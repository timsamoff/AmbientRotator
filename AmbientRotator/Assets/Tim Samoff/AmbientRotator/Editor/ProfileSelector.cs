#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AmbientRotator
{
    public class ProfileSelector : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<CustomMotionProfile> profiles = new List<CustomMotionProfile>();
        private int selectedProfile = -1;
        private Texture2D previewTexture;
        
        [MenuItem("Window/Ambient Rotator/Profile Library")]
        public static void ShowWindow()
        {
            GetWindow<ProfileSelector>("Profile Library");
        }
        
        private void OnEnable()
        {
            LoadProfiles();
            CreatePreviewTexture();
        }
        
        private void LoadProfiles()
        {
            profiles.Clear();
            string[] guids = AssetDatabase.FindAssets("t:CustomMotionProfile");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<CustomMotionProfile>(path);
                if (profile != null)
                {
                    profiles.Add(profile);
                }
            }
        }
        
        private void CreatePreviewTexture()
        {
            previewTexture = new Texture2D(64, 64);
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    previewTexture.SetPixel(x, y, new Color(0.2f, 0.2f, 0.2f));
                }
            }
            previewTexture.Apply();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Profile Library", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
            EditorGUILayout.TextField("", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("⟳", GUILayout.Width(30)))
            {
                LoadProfiles();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 40) / 120);
            columns = Mathf.Max(1, columns);
            
            int rowIndex = 0;
            for (int i = 0; i < profiles.Count; i++)
            {
                if (i % columns == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                
                DrawProfileCard(profiles[i], i);
                
                if ((i + 1) % columns == 0 || i == profiles.Count - 1)
                {
                    EditorGUILayout.EndHorizontal();
                }
                rowIndex++;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawProfileCard(CustomMotionProfile profile, int index)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(120), GUILayout.Height(160));
            
            Rect rect = GUILayoutUtility.GetRect(100, 80);
            EditorGUI.DrawPreviewTexture(rect, previewTexture);
            
            GUILayout.Label(profile.name, EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label($"Curves: {profile.xCurve.keys.Length}", EditorStyles.miniLabel);
            
            if (GUILayout.Button("Select", GUILayout.Width(80)))
            {
                selectedProfile = index;
                Selection.activeObject = profile;
            }
            
            if (GUILayout.Button("Apply", GUILayout.Width(80)))
            {
                ApplyProfileToSelected(profile);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void ApplyProfileToSelected(CustomMotionProfile profile)
        {
            if (Selection.activeGameObject == null) return;
            
            var rotator = Selection.activeGameObject.GetComponent<AmbientRotator>();
            if (rotator != null)
            {
                rotator.SetCustomProfile(profile);
                EditorUtility.SetDirty(rotator);
                Debug.Log($"Applied profile '{profile.name}' to {rotator.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("Selected object doesn't have an AmbientRotator");
            }
        }
    }
}
#endif