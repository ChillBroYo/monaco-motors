using UnityEngine;

namespace MonacoMotors.Vehicle
{
    public class CarCustomization : MonoBehaviour
    {
        [Header("Material Slots")]
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private int bodyMaterialIndex = 0;
        [SerializeField] private Renderer wheelRenderers;
        [SerializeField] private Renderer glassRenderer;

        [Header("Current Customization")]
        [SerializeField] private Color bodyColor = Color.white;
        [SerializeField] private Color wheelColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private float metallicValue = 0.8f;
        [SerializeField] private float smoothnessValue = 0.9f;

        private MaterialPropertyBlock propertyBlock;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            ApplyCustomization();
        }

        public void SetBodyColor(Color color)
        {
            bodyColor = color;
            ApplyCustomization();
        }

        public void SetWheelColor(Color color)
        {
            wheelColor = color;
            ApplyCustomization();
        }

        public void SetMetallic(float value)
        {
            metallicValue = Mathf.Clamp01(value);
            ApplyCustomization();
        }

        public void SetSmoothness(float value)
        {
            smoothnessValue = Mathf.Clamp01(value);
            ApplyCustomization();
        }

        public void ApplyCustomization()
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.GetPropertyBlock(propertyBlock, bodyMaterialIndex);
                propertyBlock.SetColor(BaseColorId, bodyColor);
                propertyBlock.SetFloat(MetallicId, metallicValue);
                propertyBlock.SetFloat(SmoothnessId, smoothnessValue);
                bodyRenderer.SetPropertyBlock(propertyBlock, bodyMaterialIndex);
            }
        }

        public CustomizationData GetCurrentCustomization()
        {
            return new CustomizationData
            {
                bodyColorHex = ColorUtility.ToHtmlStringRGB(bodyColor),
                wheelColorHex = ColorUtility.ToHtmlStringRGB(wheelColor),
                metallic = metallicValue,
                smoothness = smoothnessValue
            };
        }

        public void LoadCustomization(CustomizationData data)
        {
            if (ColorUtility.TryParseHtmlString("#" + data.bodyColorHex, out Color body))
                bodyColor = body;

            if (ColorUtility.TryParseHtmlString("#" + data.wheelColorHex, out Color wheel))
                wheelColor = wheel;

            metallicValue = data.metallic;
            smoothnessValue = data.smoothness;

            ApplyCustomization();
        }
    }

    [System.Serializable]
    public class CustomizationData
    {
        public string bodyColorHex;
        public string wheelColorHex;
        public float metallic;
        public float smoothness;
    }
}
