using System.Collections.Generic;
using System.Linq;
using _Main;
using SaL.Core;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using CardSO = Deck.CardSO;
using CardType = Deck.CardSO.CardType;

namespace SaL.Gameplay.Managers
{
    /// <summary>
    /// Manages the game's deck, including building, shuffling, and dealing cards.
    /// </summary>
    public class DeckManager : NetworkBehaviour
    {
        public static DeckManager Instance { get; private set; }

        [SerializeField] private List<CardSO> _cardAssets;
        
        private List<int> _drawPile = new();
        private Dictionary<ulong, List<int>> _playerHands = new();
        private Dictionary<ulong, int> _washHandUses = new();
        private static ISession CurrentSession => World.Get.ActiveSession;

        private void Awake()
        {
            Instance = this;
        }

        public void InitializePlayerData(ulong clientId)
        {
            if (!CurrentSession.IsHost) return;
            _washHandUses[clientId] = 2; // GDD: 2 uses per game session
        }

        public void BuildDeck()
        {
            _drawPile.Clear();
            foreach (var cardAsset in _cardAssets)
            {
                for (var i = 0; i < cardAsset.CardDeckCount; i++)
                {
                    _drawPile.Add(cardAsset.CardId);
                }
            }
            
            var random = new System.Random();
            _drawPile = _drawPile.OrderBy(x => random.Next()).ToList();
        }

        public void DealInitialHands()
        {
            _playerHands.Clear();
            var connectedClients = NetworkManager.Singleton.ConnectedClientsIds;
            foreach (var clientId in connectedClients)
            {
                var hand = new List<int>();
                for (var i = 0; i < 6; i++)
                {
                    if(_drawPile.Count > 0)
                    {
                        hand.Add(DrawCard());
                    }
                }
                _playerHands[clientId] = hand;

                var handArray = hand.ToArray();
                DealHandClientRpc(handArray, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
            }
        }

        private int DrawCard()
        {
            if (_drawPile.Count == 0)
            {
                Debug.LogError("Draw pile is empty!");
                return -1;
            }

            var cardId = _drawPile[0];
            _drawPile.RemoveAt(0);
            return cardId;
        }

        public void RemoveCardAndDrawNew(ulong clientId, int cardIdToRemove)
        {
            if (!_playerHands.ContainsKey(clientId)) return;

            _playerHands[clientId].Remove(cardIdToRemove);
            var newCard = DrawCard();
            if (newCard != -1)
            {
                _playerHands[clientId].Add(newCard);
                DrawCardClientRpc(newCard, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
            }
        }

        public bool PlayerHasCounter(ulong playerId, CardSO actionCard)
        {
            return GetCounterCard(playerId, actionCard) != null;
        }

        public CardSO GetCounterCard(ulong playerId, CardSO actionCard)
        {
            if (!_playerHands.ContainsKey(playerId)) return null;

            var targetHand = _playerHands[playerId];
            foreach (var cardInHandId in targetHand)
            {
                var cardInHand = CardRegistry.Instance.GetCardById(cardInHandId);
                if (cardInHand.BaseCardType == CardType.Defensive && cardInHand.DefensiveType == actionCard.CounteredBy)
                {
                    return cardInHand;
                }
            }
            return null;
        }

        public bool TryConsumeDefensiveCard(ulong playerId, CardSO.DefensiveCardType cardType)
        {
            if (!_playerHands.ContainsKey(playerId)) return false;

            var targetHand = _playerHands[playerId];
            var cardToConsume = -1;
            
            foreach (var cardInHandId in targetHand)
            {
                var cardInHand = CardRegistry.Instance.GetCardById(cardInHandId);
                if (cardInHand.BaseCardType == CardType.Defensive && cardInHand.DefensiveType == cardType)
                {
                    cardToConsume = cardInHandId;
                    break;
                }
            }

            if (cardToConsume != -1)
            {
                RemoveCardAndDrawNew(playerId, cardToConsume);
                return true;
            }

            return false;
        }

        [ServerRpc(RequireOwnership = false)]
        public void WashHandServerRpc(ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;

            if (!_washHandUses.ContainsKey(clientId) || _washHandUses[clientId] <= 0)
            {
                return; // Player has no uses left
            }

            _washHandUses[clientId]--;

            // Return the player's old hand to the draw pile
            var oldHand = _playerHands[clientId];
            _drawPile.AddRange(oldHand);

            // Shuffle the draw pile with the returned cards
            var random = new System.Random();
            _drawPile = _drawPile.OrderBy(x => random.Next()).ToList();

            // Deal a new hand to the player
            var newHand = new List<int>();
            for (var i = 0; i < 6; i++) // GDD: 6 cards per hand
            {
                if (_drawPile.Count > 0)
                {
                    newHand.Add(DrawCard());
                }
            }
            _playerHands[clientId] = newHand;

            // Send the new hand to the client
            var handArray = newHand.ToArray();
            DealHandClientRpc(handArray, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
        }

        [ClientRpc]
        private void DealHandClientRpc(int[] hand, ClientRpcParams rpcParams = default)
        {
            ClientController.Instance.SetInitialHand(hand);
        }
        
        [ClientRpc]
        private void DrawCardClientRpc(int newCardId, ClientRpcParams rpcParams = default)
        {
            ClientController.Instance.AddCardToHand(newCardId);
        }
    }
}
