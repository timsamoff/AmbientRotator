using UnityEngine;
using UnityEngine.UI;

namespace AmbientRotator
{
    [RequireComponent(typeof(RectTransform))]
    public class UIAmbientRotator : AmbientRotator
    {
        [Header("UI Specific")]
        [SerializeField] private bool affectPosition = false;
        [SerializeField] private Vector2 positionAmplitude = new Vector2(5f, 5f);
        [SerializeField] private bool affectScale = false;
        [SerializeField] private Vector2 scaleAmplitude = new Vector2(0.1f, 0.1f);
        [SerializeField] private bool affectColor = false;
        [SerializeField] private Color colorMin = Color.white;
        [SerializeField] private Color colorMax = Color.gray;
        
        private RectTransform rectTransform;
        private Graphic graphic;
        private Vector2 initialPosition;
        private Vector3 initialScale;
        private Color initialColor;
        
        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            graphic = GetComponent<Graphic>();
            
            initialPosition = rectTransform.anchoredPosition;
            initialScale = rectTransform.localScale;
            if (graphic != null)
            {
                initialColor = graphic.color;
            }
        }
        
        protected override void UpdateMotion()
        {
            // Call base update first
            base.UpdateMotion();
            
            Vector3 offset = CurrentOffset;
            
            if (affectPosition)
            {
                Vector2 posOffset = new Vector2(
                    offset.x * positionAmplitude.x * 0.1f,
                    offset.y * positionAmplitude.y * 0.1f
                );
                rectTransform.anchoredPosition = initialPosition + posOffset;
            }
            
            if (affectScale)
            {
                float scaleX = 1f + offset.x * scaleAmplitude.x * 0.01f;
                float scaleY = 1f + offset.y * scaleAmplitude.y * 0.01f;
                rectTransform.localScale = new Vector3(
                    initialScale.x * scaleX,
                    initialScale.y * scaleY,
                    initialScale.z
                );
            }
            
            if (affectColor && graphic != null)
            {
                // Use Speed property instead of direct field access
                float t = Mathf.PingPong(Time.time * Speed * 0.5f, 1f);
                graphic.color = Color.Lerp(colorMin, colorMax, t);
            }
        }
    }
}