using Modules.AdressableSystem;
using Game.Controllers;
using Game.Core.Constants;
using Game.Models.Cards;
using Game.Views;
using Modules.Logger;
using NaughtyAttributes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private Transform _cardHolder;
    [SerializeField] private TMP_Text _recipeTypeText;

    private List<CardItem> _cardItems = new List<CardItem>();
    async void Start()
    {
        await CardController.Init();
        await RecipeController.Init();
        await StageController.Init();
        await DeckController.Init();
    }

    private void SetPossibleRecipe(List<CardScriptable> handCards)
    {
        var recipes = DeckController.EvaluateCardsWithRecipe(handCards);

        _recipeTypeText.text = string.Empty;

        if (recipes.Count > 0)
        {
            var str = string.Empty;
            foreach (var item in recipes)
            {
                str += $" {item.GetName()} - ({item.GetRecipeCategory()}) |";
            }
            _recipeTypeText.text = str.Remove(str.Length - 1);
        }
        else
        {
            _recipeTypeText.text = $"No matches found";
        }
    }
    [Button]

    public async void TestIt()
    {
        _cardHolder.gameObject.SetActive(false);

        DeckController.GetSelectedCards().Clear();

        foreach (var cardItem in _cardItems)
        {
            AddressableManager.ReleaseInstance(cardItem.gameObject);
        }
        _cardItems.Clear();

        var handCards = DeckController.CreateTestHand();

        SetPossibleRecipe(handCards);

        float spacing = 10f; // Sahne ölçeğine göre ayarlayın (örneğin 2 veya 3)
        float totalWidth = (handCards.Count - 1) * spacing;
        for (int i = 0; i < handCards.Count; i++)
        {
            var item = handCards[i];
            var cardItem = await AddressableManager.InstantiateAsync<CardItem>(GameConstants.CARD_ITEM_SCRIPTABLE_ADDRESSABLE_KEY, _cardHolder);
            await cardItem.Init(item);
            float x = i * spacing - totalWidth / 2f;
            cardItem.transform.localPosition = new Vector3(x, 0, 0);
            _cardItems.Add(cardItem);
        }

        _cardHolder.gameObject.SetActive(true);
    }
    [Button]
    public async void SendHand()
    {
        var selectedCards = DeckController.GetSelectedCards();

        DebugLogger.Log("Selected Cards" + selectedCards.Count);

        foreach (var item in selectedCards)
        {
            var randomCard = DeckController.GetRandomCard(item.GetCardCategory(), _cardItems.ConvertAll(x => x.CardScriptable));
            var sentCard = _cardItems.Find(x => x.CardScriptable.GetIngredientType() == item.GetIngredientType());
            sentCard.gameObject.SetActive(false);
            var cardItem = await AddressableManager.InstantiateAsync<CardItem>(GameConstants.CARD_ITEM_SCRIPTABLE_ADDRESSABLE_KEY, _cardHolder);
            await cardItem.Init(randomCard);
            cardItem.transform.localPosition = new Vector3(sentCard.transform.localPosition.x, 0, sentCard.transform.localPosition.z);
            _cardItems.Add(cardItem);
        }

        var selectedItems = new List<GameObject>();
        foreach (var item in selectedCards)
        {
            var cardItem = _cardItems.Find(x => x.CardScriptable.GetIngredientType() == item.GetIngredientType());
            selectedItems.Add(cardItem.gameObject);
            _cardItems.Remove(cardItem);
        }
        foreach (var item in selectedItems)
        {
            AddressableManager.ReleaseInstance(item);
        }
        DeckController.GetSelectedCards().Clear();

        SetPossibleRecipe(_cardItems.ConvertAll(x => x.CardScriptable));
    }
}
