using DG.Tweening;
using Modules.Singleton.Scriptables;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utilities.UI
{
    public class CustomButton : Button
    {
        private static readonly Vector3 DefaultRestingScale = Vector3.one;

        [Header("Click Animation")]
        [SerializeField] private float _pressedScale = 0.96f;
        [SerializeField] private float _pressDuration = 0.08f;
        [SerializeField] private float _releaseDuration = 0.12f;
        [SerializeField] private Ease _pressEase = Ease.Linear;
        [SerializeField] private Ease _releaseEase = Ease.Linear;
        [SerializeField] private TMP_Text _label;

        private readonly Dictionary<Image, Material> _originalImageMaterials = new Dictionary<Image, Material>();
        private readonly Dictionary<TMP_Text, Color> _originalTextColors = new Dictionary<TMP_Text, Color>();

        private Tween _scaleTween;
        private Vector3 _initialScale;
        private bool _isPressedVisual;
        private bool _hasCachedInitialScale;

        protected override void Awake()
        {
            base.Awake();
            CacheInitialScaleIfNeeded();
            EnsureValidScaleState();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CacheInitialScaleIfNeeded();
            EnsureValidScaleState();
            _isPressedVisual = false;

            if (interactable)
            {
                RestoreVisualState();
            }
            else
            {
                ApplyDisabledVisualState();
            }
        }

        protected override void OnDisable()
        {
            ResetScale();
            base.OnDisable();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!IsActive() || !IsInteractable())
            {
                return;
            }

            SetPressedVisual(true);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (!IsActive())
            {
                return;
            }

            SetPressedVisual(false);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (!IsActive())
            {
                return;
            }

            SetPressedVisual(false);
        }

        public void Disable()
        {
            interactable = false;
            ApplyDisabledVisualState();
            ResetScale();
        }

        public void Enable()
        {
            interactable = true;
            RestoreVisualState();
            ResetScale();
        }

        public void SetText(string text)
        {
            if (_label == null)
            {
                _label = GetComponentInChildren<TMP_Text>(true);
            }

            if (_label == null) return;
            _label.text = text;
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);

            if (!IsActive() || !IsInteractable())
            {
                return;
            }

            SetPressedVisual(true);
            DOVirtual.DelayedCall(_pressDuration, () =>
            {
                if (this == null || !isActiveAndEnabled)
                {
                    return;
                }

                SetPressedVisual(false);
            }).SetUpdate(true);
        }

        private void SetPressedVisual(bool isPressed)
        {
            if (_isPressedVisual == isPressed)
            {
                return;
            }

            _isPressedVisual = isPressed;
            var targetScale = isPressed ? _initialScale * _pressedScale : _initialScale;
            var duration = isPressed ? _pressDuration : _releaseDuration;
            var ease = isPressed ? _pressEase : _releaseEase;
            AnimateScale(targetScale, duration, ease);
        }

        private void CacheInitialScaleIfNeeded()
        {
            var currentScale = transform.localScale;

            if (!_hasCachedInitialScale)
            {
                _initialScale = IsZeroScale(currentScale) ? DefaultRestingScale : currentScale;
                _hasCachedInitialScale = true;
                return;
            }

            if (IsZeroScale(_initialScale) && !IsZeroScale(currentScale))
            {
                _initialScale = currentScale;
            }
        }

        private void EnsureValidScaleState()
        {
            if (!_hasCachedInitialScale || IsZeroScale(_initialScale))
            {
                _initialScale = DefaultRestingScale;
                _hasCachedInitialScale = true;
            }

            if (IsZeroScale(transform.localScale))
            {
                transform.localScale = _initialScale;
            }
        }

        private void AnimateScale(Vector3 targetScale, float duration, Ease ease)
        {
            if ((transform.localScale - targetScale).sqrMagnitude <= 0.000001f)
            {
                return;
            }

            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(targetScale, duration).SetEase(ease).SetUpdate(true);
        }

        private void ResetScale()
        {
            _scaleTween?.Kill();
            _isPressedVisual = false;
            EnsureValidScaleState();
            transform.localScale = _initialScale;
        }

        private static bool IsZeroScale(Vector3 scale)
        {
            return Mathf.Abs(scale.x) <= 0.0001f
                && Mathf.Abs(scale.y) <= 0.0001f
                && Mathf.Abs(scale.z) <= 0.0001f;
        }

        private void ApplyDisabledVisualState()
        {
            CacheCurrentVisualState();

            var grayscaleMaterial = ResourceWarehouse.Instance.GetGrayscaleUIMaterial();
            var disabledTextColor = ResourceWarehouse.Instance.DisabledTextColor;

            var images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null || grayscaleMaterial == null) continue;
                images[i].material = grayscaleMaterial;
            }

            var texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null) continue;
                texts[i].color = disabledTextColor;
            }
        }

        private void RestoreVisualState()
        {
            foreach (var pair in _originalImageMaterials)
            {
                if (pair.Key == null) continue;
                pair.Key.material = pair.Value;
            }

            foreach (var pair in _originalTextColors)
            {
                if (pair.Key == null) continue;
                pair.Key.color = pair.Value;
            }

            _originalImageMaterials.Clear();
            _originalTextColors.Clear();
        }

        private void CacheCurrentVisualState()
        {
            var images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null || _originalImageMaterials.ContainsKey(images[i])) continue;
                _originalImageMaterials.Add(images[i], images[i].material);
            }

            var texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null || _originalTextColors.ContainsKey(texts[i])) continue;
                _originalTextColors.Add(texts[i], texts[i].color);
            }
        }
    }
}