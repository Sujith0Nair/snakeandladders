using System.Collections.Generic;
using Deck;
using SaL.Gameplay.Managers;
using UnityEngine;
using UnityEngine.UI;
using DeckManager = SaL.Gameplay.Managers.DeckManager;

namespace SaL.UI
{
    /// <summary>
    /// Manages the visual representation of a player's hand. This is a client-side script
    /// that receives card data from the HostController and displays it.
    /// </summary>
    public class HandUI : MonoBehaviour
    {
        [SerializeField] private Button _washHandButton;
        [SerializeField] private CardUI _cardPrefab;
        [SerializeField] private Transform _handContainer;

        private readonly List<CardUI> _cardsInHand = new();

        private void Start()
        {
            // TODO: Re-enable this if the wash hand button is part of the final UI
            // _washHandButton.onClick.AddListener(OnWashHandClicked);
        }

        private void OnWashHandClicked()
        {
            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.WashHandServerRpc();
            }
        }

        /// <summary>
        /// Called by the ClientController when it receives new hand data from the host.
        /// Clears the old hand and instantiates new card visuals.
        /// </summary>
        public void UpdateHand(List<CardSO> cards)
        {
            // Clear existing cards
            foreach (var card in _cardsInHand)
            {
                Destroy(card.gameObject);
            }
            _cardsInHand.Clear();

            // Instantiate new card prefabs based on the received data
            foreach (var cardData in cards)
            {
                var newCard = Instantiate(_cardPrefab, _handContainer);
                newCard.Initialize(cardData);
                _cardsInHand.Add(newCard);
            }
        }

        /// <summary>
        /// Sets the interactable state for all cards in the hand.
        /// </summary>
        /// <param name="isMyTurn">True if it's the local player's turn.</param>
        public void SetTurnStatus(bool isMyTurn)
        {
            foreach (var card in _cardsInHand)
            {
                card.SetInteractable(isMyTurn);
            }
        }
    }
}
