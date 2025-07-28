using Deck;
using SaL.UI;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using System.Collections;
using SaL.Gameplay.Managers;
using System.Collections.Generic;

namespace SaL.Core
{
    /// <summary>
    /// A client-side helper for managing UI interactions and communicating with the HostController.
    /// This is a standard MonoBehaviour and does not need to be a NetworkBehaviour.
    /// </summary>
    public class ClientController : MonoBehaviour
    {
        [SerializeField] private HandUI _handUI;
        public static ClientController Instance { get; private set; }
        public Board.BoardController BoardController { get; private set; }
        
        private readonly List<CardSO> _currentHand = new();
        private int _cardIdToPlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            
            BoardController = FindFirstObjectByType<Board.BoardController>();
        }

        private IEnumerator Start()
        {
            while (GameManager.Instance == null)
            {
                yield return null;
            }
            GameManager.Instance.CurrentPlayerTurn.OnValueChanged += OnTurnChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentPlayerTurn.OnValueChanged -= OnTurnChanged;
            }
        }

        private void OnTurnChanged(int previousValue, int newValue)
        {
            var isMyTurn = (ulong)newValue == NetworkManager.Singleton.LocalClientId;
            _handUI.SetTurnStatus(isMyTurn);
        }

        public void SetInitialHand(int[] cardIds)
        {
            _currentHand.Clear();
            foreach (var cardId in cardIds)
            {
                var card = CardRegistry.Instance.GetCardById(cardId);
                if (card != null)
                {
                    _currentHand.Add(card);
                }
            }
            _handUI.UpdateHand(_currentHand);
        }

        public void AddCardToHand(int cardId)
        {
            var card = CardRegistry.Instance.GetCardById(cardId);
            if (card != null)
            {
                _currentHand.Add(card);
            }
            _handUI.UpdateHand(_currentHand);
        }

        public void OnCardClicked(int cardId)
        {
            var card = CardRegistry.Instance.GetCardById(cardId);
            if (card == null) return;

            _cardIdToPlay = cardId;

            if (RequiresPlayerTarget(card.ActionType))
            {
                var targets = PlayerPawnSpawner.PlayerPawns
                    .Where(kvp => kvp.Key != NetworkManager.Singleton.LocalClientId)
                    .Select(kvp => kvp.Value.gameObject)
                    .ToList();
                
                SelectionManager.Instance.StartSelection(targets, OnPlayerTargetSelected);
            }
            else if (RequiresLadderTarget(card.ActionType))
            {
                var targets = BoardController.GetLadderObjects();
                SelectionManager.Instance.StartSelection(targets, OnLadderTargetSelected);
            }
            else
            {
                PlayCard(cardId, ulong.MaxValue);
            }
        }

        private void OnPlayerTargetSelected(GameObject targetPawn)
        {
            // Find the OwnerClientId associated with this pawn GameObject
            var targetClientId = PlayerPawnSpawner.PlayerPawns
                .FirstOrDefault(kvp => kvp.Value.gameObject == targetPawn).Key;
            
            PlayCard(_cardIdToPlay, targetClientId);
        }

        private void OnLadderTargetSelected(GameObject targetLadder)
        {
            // We need a way to get the ID from the ladder object.
            // Assuming the ladder's name is "Ladder_{BaseCellId}_{TopCellId}"
            var nameParts = targetLadder.name.Split('_');
            if (nameParts.Length == 3 && int.TryParse(nameParts[1], out var ladderId))
            {
                PlayCard(_cardIdToPlay, ulong.MaxValue, ladderId);
            }
        }

        private void PlayCard(int cardId, ulong targetId, int ladderId = -1)
        {
            var cardToRemove = _currentHand.Find(c => c.CardId == cardId);
            if (cardToRemove != null)
            {
                _currentHand.Remove(cardToRemove);
                _handUI.UpdateHand(_currentHand);
            }
            
            GameManager.Instance.PlayCardServerRpc(cardId, targetId, ladderId);
        }

        private static bool RequiresPlayerTarget(CardSO.ActionCardType type)
        {
            switch (type)
            {
                case CardSO.ActionCardType.FreezeOpponent:
                case CardSO.ActionCardType.SwapPositions:
                case CardSO.ActionCardType.ForceToSnake:
                    return true;
                default:
                    return false;
            }
        }

        private static bool RequiresLadderTarget(CardSO.ActionCardType type)
        {
            return type is CardSO.ActionCardType.LadderVandalism or CardSO.ActionCardType.LadderLockout;
        }
    }
}
