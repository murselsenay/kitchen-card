using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Models.Recipes;
using UnityEngine;

namespace Game.Views
{
    public class RecipeItemView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _iconSpriteRenderer;
        [SerializeField] private float _spawnDuration = 0.28f;
        [SerializeField] private float _consumeDuration = 0.18f;

        private void Awake()
        {
            HideImmediate();
        }

        public async UniTask PrepareAsync(RecipeScriptable recipe)
        {
            EnsureRenderer();
            transform.DOKill();
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
            _iconSpriteRenderer.sprite = null;

            if (recipe == null) return;

            _iconSpriteRenderer.sprite = await recipe.GetSprite();
        }

        public void ShowPrepared()
        {
            transform.DOKill();
            gameObject.SetActive(true);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        public async UniTask PlaySpawnAsync()
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;

            var tween = transform.DOScale(Vector3.one, _spawnDuration).SetEase(Ease.OutBack);
            await tween.AsyncWaitForCompletion().AsUniTask();
        }

        public async UniTask PlayConsumeAsync()
        {
            transform.DOKill();
            var tween = transform.DOScale(Vector3.zero, _consumeDuration).SetEase(Ease.InBack);
            await tween.AsyncWaitForCompletion().AsUniTask();
        }

        public void HideImmediate()
        {
            EnsureRenderer();
            transform.DOKill();
            _iconSpriteRenderer.sprite = null;
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        private void EnsureRenderer()
        {
            if (_iconSpriteRenderer != null) return;

            _iconSpriteRenderer = GetComponent<SpriteRenderer>();
            if (_iconSpriteRenderer == null)
            {
                _iconSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
    }
}