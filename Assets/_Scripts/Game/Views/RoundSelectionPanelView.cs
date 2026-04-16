using Cysharp.Threading.Tasks;
using Game.Controllers;
using Game.Core.Constants;
using Game.Models.Stages;
using Modules.AdressableSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Views
{
    public class RoundSelectionPanelView : MonoBehaviour
    {
        [SerializeField] private Transform _itemRoot;
        [SerializeField] private GameObject _contentRoot;

        private readonly List<RoundSelectionItemView> _spawnedItems = new List<RoundSelectionItemView>();
        private int _renderVersion;

        private void OnEnable()
        {
            RoundSelectionController.Bind(this);
        }

        private void OnDisable()
        {
            RoundSelectionController.Unbind(this);
        }

        public async UniTask Render(IReadOnlyList<RoundSelectionData> items)
        {
            Clear();

            if (_itemRoot == null) return;

            var renderVersion = _renderVersion;

            for (int i = 0; i < items.Count; i++)
            {
                var item = await AddressableManager.InstantiateAsync<RoundSelectionItemView>(GameConstants.ROUND_SELECTION_ITEM_ADDRESSABLE_KEY, _itemRoot);
                if (item == null) continue;

                if (renderVersion != _renderVersion)
                {
                    AddressableManager.ReleaseInstance(item.gameObject);
                    continue;
                }

                item.Init(items[i]);
                _spawnedItems.Add(item);
            }
        }

        public void Clear()
        {
            _renderVersion++;

            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                if (_spawnedItems[i] == null) continue;
                AddressableManager.ReleaseInstance(_spawnedItems[i].gameObject);
            }

            _spawnedItems.Clear();
        }

        public void Show()
        {
            GetContentRoot().SetActive(true);
        }

        public void Hide()
        {
            GetContentRoot().SetActive(false);
        }

        private GameObject GetContentRoot()
        {
            if (_contentRoot != null)
                return _contentRoot;

            if (_itemRoot != null)
                return _itemRoot.gameObject;

            return gameObject;
        }
    }
}