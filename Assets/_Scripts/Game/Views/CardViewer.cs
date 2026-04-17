using System;
using UnityEngine;

namespace Game.Views
{
    public class CardViewer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundSpriteRenderer;
        [SerializeField] private SpriteRenderer _iconSpriteRenderer;

        public event Action Clicked;

        public void SetIcon(Sprite iconSprite)
        {
            if (_iconSpriteRenderer == null) return;

            _iconSpriteRenderer.sprite = iconSprite;
        }

        private void OnMouseDown()
        {
            Clicked?.Invoke();
        }
    }
}