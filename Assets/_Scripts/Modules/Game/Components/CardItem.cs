using Cysharp.Threading.Tasks;
using Modules.Game.Managers;
using Modules.Game.Scriptables.Card;
using System;
using UnityEngine;

namespace Modules.Game.Components
{
    public class CardItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundSpriteRenderer;
        [SerializeField] private SpriteRenderer _iconSpriteRenderer;

        private CardScriptable _cardScriptable;
        public CardScriptable CardScriptable => _cardScriptable;

        private bool _isSelected => DeckManager.GetSelectedCards().Contains(_cardScriptable);
        public async UniTask Init(CardScriptable cardScriptable)
        {
            _cardScriptable = cardScriptable;
            _iconSpriteRenderer.sprite = await cardScriptable.GetIconSprite();
        }

        public void Select()
        {
            if (DeckManager.AddSelectedCard(_cardScriptable))
                transform.localPosition = new Vector3(transform.localPosition.x, 2f, transform.localPosition.z);
        }

        public void Deselect()
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);

            DeckManager.RemoveSelectedCard(_cardScriptable);
        }

        private void OnMouseDown()
        {
            if (_isSelected)
            {
                Deselect();
            }
            else
            {
                Select();
            }
        }
    }
}