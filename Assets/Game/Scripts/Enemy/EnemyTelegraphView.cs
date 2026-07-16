using UnityEngine;

namespace BAA
{
    public sealed class EnemyTelegraphView : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color warningColor = new Color(1f, 0.1f, 0.1f, 1f);

        private MaterialPropertyBlock _propertyBlock;
        private Color _baseColor = Color.white;
        private int _colorPropertyId;
        private bool _isReady;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                Debug.LogError("EnemyTelegraphView requires a target Renderer.", this);
                enabled = false;
                return;
            }

            var material = targetRenderer.sharedMaterial;
            if (material != null && material.HasProperty("_BaseColor"))
            {
                _colorPropertyId = Shader.PropertyToID("_BaseColor");
            }
            else
            {
                _colorPropertyId = Shader.PropertyToID("_Color");
            }

            if (material != null && material.HasProperty(_colorPropertyId))
            {
                _baseColor = material.GetColor(_colorPropertyId);
            }

            _propertyBlock = new MaterialPropertyBlock();
            _isReady = true;
        }

        public void SetWarning(bool isWarning)
        {
            if (!_isReady)
            {
                return;
            }

            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(_colorPropertyId, isWarning ? warningColor : _baseColor);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
