#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AmbientRotator
{
    [CustomPropertyDrawer(typeof(RangeAttribute))]
    public class RangeTooltipDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangeAttribute range = (RangeAttribute)attribute;

            // Try to get tooltip from the field
            string tooltip = null;
            var tooltips = fieldInfo?.GetCustomAttributes(typeof(TooltipAttribute), true);
            if (tooltips != null && tooltips.Length > 0)
            {
                tooltip = ((TooltipAttribute)tooltips[0]).tooltip;
            }

            // Create label with tooltip if available
            GUIContent newLabel = new GUIContent(label.text, tooltip);

            // Draw the appropriate range control based on property type
            if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = EditorGUI.Slider(position, newLabel, property.floatValue, range.min, range.max);
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.IntSlider(position, newLabel, property.intValue, (int)range.min, (int)range.max);
            }
            else
            {
                // Fallback - shouldn't happen
                EditorGUI.PropertyField(position, property, newLabel);
            }
        }
    }
}
#endif