#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace AmbientRotator
{
    public class ProfileCreator : EditorWindow
    {
        private string profileName = "NewCustomProfile";
        private CustomMotionProfile newProfile;
        private bool useTemplate = false;
        private int selectedTemplate = 0;
        private string[] templates = { "Subtle", "Gentle", "Organic", "Dynamic", "Chaotic", "Figure 8", "Spiral" };
        
        [MenuItem("Assets/Create/Ambient Rotator/Custom Profile", false, 0)]
        public static void CreateProfile()
        {
            GetWindow<ProfileCreator>("Create Profile");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Create Custom Motion Profile", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Profile Name", EditorStyles.boldLabel);
            profileName = EditorGUILayout.TextField(profileName);
            
            EditorGUILayout.Space();
            
            useTemplate = EditorGUILayout.Toggle("Use Template", useTemplate);
            if (useTemplate)
            {
                selectedTemplate = EditorGUILayout.Popup("Template", selectedTemplate, templates);
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create Profile", GUILayout.Height(40)))
            {
                CreateNewProfile();
            }
        }
        
        private void CreateNewProfile()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Profile",
                profileName,
                "asset",
                "Choose where to save the profile"
            );
            
            if (string.IsNullOrEmpty(path)) return;
            
            newProfile = ScriptableObject.CreateInstance<CustomMotionProfile>();
            
            if (useTemplate)
            {
                ApplyTemplate(newProfile, selectedTemplate);
            }
            
            AssetDatabase.CreateAsset(newProfile, path);
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newProfile;
            
            Close();
        }
        
        private void ApplyTemplate(CustomMotionProfile profile, int templateIndex)
        {
            switch(templateIndex)
            {
                case 0: // Subtle
                    profile.xCurve = CreateSmoothCurve(0.3f);
                    profile.yCurve = CreateSmoothCurve(0.2f);
                    profile.zCurve = CreateSmoothCurve(0.3f);
                    break;
                case 1: // Gentle
                    profile.xCurve = CreateSmoothCurve(1f);
                    profile.yCurve = CreateSmoothCurve(0.8f);
                    profile.zCurve = CreateSmoothCurve(1f);
                    break;
                case 2: // Organic
                    profile.xCurve = CreateComplexCurve(1.5f, 0.8f);
                    profile.yCurve = CreateComplexCurve(1f, 0.5f);
                    profile.zCurve = CreateComplexCurve(1.2f, 0.6f);
                    break;
                case 3: // Dynamic
                    profile.xCurve = CreateSmoothCurve(3f);
                    profile.yCurve = CreateSmoothCurve(2f);
                    profile.zCurve = CreateSmoothCurve(2.5f);
                    break;
                case 4: // Chaotic
                    profile.xCurve = CreateChaoticCurve(4f);
                    profile.yCurve = CreateChaoticCurve(3f);
                    profile.zCurve = CreateChaoticCurve(3.5f);
                    break;
                case 5: // Figure 8
                    CreateFigure8Profile(profile);
                    break;
                case 6: // Spiral
                    CreateSpiralProfile(profile);
                    break;
            }
            
            profile.curveDuration = 2f;
            profile.loopCurve = true;
            profile.randomizeStart = true;
        }
        
        private AnimationCurve CreateSmoothCurve(float amplitude)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.25f, amplitude);
            curve.AddKey(0.5f, 0);
            curve.AddKey(0.75f, -amplitude);
            curve.AddKey(1, 0);
            
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            
            return curve;
        }
        
        private AnimationCurve CreateComplexCurve(float amplitude, float secondaryAmplitude)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.2f, amplitude);
            curve.AddKey(0.4f, amplitude * 0.5f);
            curve.AddKey(0.6f, -amplitude * 0.5f);
            curve.AddKey(0.8f, -amplitude);
            curve.AddKey(1, 0);
            
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            
            return curve;
        }
        
        private AnimationCurve CreateChaoticCurve(float amplitude)
        {
            AnimationCurve curve = new AnimationCurve();
            for (int i = 0; i <= 10; i++)
            {
                float t = i / 10f;
                float value = (Mathf.PerlinNoise(t * 3f, 0.5f) * 2 - 1) * amplitude;
                curve.AddKey(t, value);
            }
            
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            
            return curve;
        }
        
        private void CreateFigure8Profile(CustomMotionProfile profile)
        {
            AnimationCurve x = new AnimationCurve();
            x.AddKey(0, 0);
            x.AddKey(0.25f, 2);
            x.AddKey(0.5f, 0);
            x.AddKey(0.75f, -2);
            x.AddKey(1, 0);
            
            AnimationCurve y = new AnimationCurve();
            y.AddKey(0, 2);
            y.AddKey(0.25f, 0);
            y.AddKey(0.5f, -2);
            y.AddKey(0.75f, 0);
            y.AddKey(1, 2);
            
            profile.xCurve = x;
            profile.yCurve = y;
            profile.zCurve = AnimationCurve.Linear(0, 0, 1, 0);
        }
        
        private void CreateSpiralProfile(CustomMotionProfile profile)
        {
            AnimationCurve x = new AnimationCurve();
            x.AddKey(0, 0);
            x.AddKey(0.5f, 3);
            x.AddKey(1, 0);
            
            AnimationCurve y = new AnimationCurve();
            y.AddKey(0, 0);
            y.AddKey(0.5f, 0);
            y.AddKey(1, 3);
            
            AnimationCurve z = new AnimationCurve();
            z.AddKey(0, 0);
            z.AddKey(0.5f, 2);
            z.AddKey(1, -2);
            
            profile.xCurve = x;
            profile.yCurve = y;
            profile.zCurve = z;
            profile.curveDuration = 4f;
        }
    }
}
#endif