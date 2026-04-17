using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Views
{
    public class CardAnimator : MonoBehaviour
    {
        [SerializeField] private float _selectedYOffset = 2f;
        [SerializeField] private float _recipeMergeDuration = 0.3f;
        [SerializeField] private float _recipeMergeScale = 0.5f;
        [SerializeField] private float _looseConsumePunchScale = 1.08f;
        [SerializeField] private float _looseConsumeDuration = 0.24f;
        [SerializeField] private float _spawnDuration = 0.28f;

        private Vector3 _baseLocalPosition;
        private bool _hasBaseLocalPosition;

        public void SetBaseLocalPosition(Vector3 baseLocalPosition)
        {
            _baseLocalPosition = baseLocalPosition;
            _hasBaseLocalPosition = true;
        }

        public void ApplySelectionState(bool isSelected, bool isPreviewSelected)
        {
            EnsureBaseLocalPosition();

            float yOffset = isSelected || isPreviewSelected ? _selectedYOffset : 0f;
            transform.localPosition = new Vector3(_baseLocalPosition.x, _baseLocalPosition.y + yOffset, _baseLocalPosition.z);
        }

        public void SnapToBaseState()
        {
            EnsureBaseLocalPosition();
            transform.DOKill();
            transform.localPosition = _baseLocalPosition;
            transform.localScale = Vector3.one;
        }

        public async UniTask PlayRecipeMergeAsync(Vector3 targetWorldPosition)
        {
            EnsureBaseLocalPosition();
            transform.DOKill();

            var sequence = DOTween.Sequence();
            sequence.Join(transform.DOMove(targetWorldPosition, _recipeMergeDuration).SetEase(Ease.InQuad));
            sequence.Join(transform.DOScale(_recipeMergeScale, _recipeMergeDuration).SetEase(Ease.InQuad));
            sequence.Append(transform.DOScale(Vector3.zero, _recipeMergeDuration * 0.55f).SetEase(Ease.InBack));

            await sequence.AsyncWaitForCompletion().AsUniTask();
        }

        public async UniTask PlayLooseConsumeAsync()
        {
            EnsureBaseLocalPosition();
            transform.DOKill();

            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(_looseConsumePunchScale, _looseConsumeDuration * 0.4f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(Vector3.zero, _looseConsumeDuration * 0.6f).SetEase(Ease.InBack));

            await sequence.AsyncWaitForCompletion().AsUniTask();
        }

        public async UniTask PlaySpawnAsync()
        {
            EnsureBaseLocalPosition();
            transform.DOKill();
            transform.localPosition = _baseLocalPosition;
            transform.localScale = Vector3.zero;

            var tween = transform.DOScale(Vector3.one, _spawnDuration).SetEase(Ease.OutBack);
            await tween.AsyncWaitForCompletion().AsUniTask();
        }

        private void EnsureBaseLocalPosition()
        {
            if (_hasBaseLocalPosition) return;

            _baseLocalPosition = transform.localPosition;
            _hasBaseLocalPosition = true;
        }
    }
}