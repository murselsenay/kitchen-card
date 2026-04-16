using Modules.Singleton;
using UnityEngine;

namespace Modules.Singleton.Scriptables
{
    [CreateAssetMenu(menuName = "Scriptables/Singletons/" + nameof(ResourceWarehouse), fileName = nameof(ResourceWarehouse))]
    public class ResourceWarehouse : ScriptableSingleton<ResourceWarehouse>
    {
        [Header("UI Materials")]
        [SerializeField] private Material _grayscaleUIMaterial;
        [SerializeField] private Shader _grayscaleUIShader;

        [Header("UI Colors")]
        [SerializeField] private Color _disabledTextColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        private Material _runtimeGrayscaleMaterial;

        public Color DisabledTextColor => _disabledTextColor;

        public Material GetGrayscaleUIMaterial()
        {
            if (_grayscaleUIMaterial != null)
            {
                return _grayscaleUIMaterial;
            }

            if (_runtimeGrayscaleMaterial != null)
            {
                return _runtimeGrayscaleMaterial;
            }

            if (_grayscaleUIShader == null)
            {
                _grayscaleUIShader = Shader.Find("UI/Grayscale");
            }

            if (_grayscaleUIShader == null)
            {
                Debug.LogWarning("[ResourceWarehouse] UI/Grayscale shader could not be found.", this);
                return null;
            }

            _runtimeGrayscaleMaterial = new Material(_grayscaleUIShader)
            {
                name = "Runtime UI Grayscale Material",
                hideFlags = HideFlags.DontUnloadUnusedAsset
            };

            return _runtimeGrayscaleMaterial;
        }
    }
}