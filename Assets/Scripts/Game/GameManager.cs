using System;
using System.Collections.Generic;
using Board;
using Deck;
using Player;
using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private SaLBoard board;

        [SerializeField] internal int playerCount;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform spawnPoint;

        [SerializeField] private Color player1Color;
        [SerializeField] private Color player2Color;
        [SerializeField] private Color player3Color;
        [SerializeField] private Color player4Color;

        internal int currentPlayerTurn;

        public Action<int> OnPlayerTurnFinished;
        public Action<int, int> OnPlayerUsedCard;

        private List<PlayerController> players;

        private bool canPreformAction;

        private int currentCellIndexP1;
        private int currentCellIndexP2;
        private int currentCellIndexP3;
        private int currentCellIndexP4;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Start()
        {
            canPreformAction = true;

            currentPlayerTurn = 0;
            
            currentCellIndexP1 = 0;
            currentCellIndexP2 = 0;
            currentCellIndexP3 = 0;
            currentCellIndexP4 = 0;

            players = new List<PlayerController>();

            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            for (int i = 0; i < playerCount; i++)
            {
                var spawnedPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
                var playerController = spawnedPlayer.GetComponent<PlayerController>();
                playerController.Init(board, i, GetPlayerColor(i));
                players.Add(playerController);
            }
        }

        private Color GetPlayerColor(int playerID)
        {
            if (playerID == 0)
            {
                return player1Color;
            }

            if (playerID == 1)
            {
                return player2Color;
            }

            if (playerID == 2)
            {
                return player3Color;
            }

            return player4Color;
        }

        public void PlayCard(int playerID, int cardIndex, CardSO cardData)
        {
            if (!canPreformAction)
            {
                return;
            }

            if (currentPlayerTurn != playerID)
            {
                // Debug.LogError($"Not Ur Turn!");
                return;
            }

            canPreformAction = false;

            if (cardData.cardType.Equals(CardType.MovementCards))
            {
                HandleMovementCards(playerID, cardIndex, cardData);
            }
            else if (cardData.cardType.Equals(CardType.ActionCards))
            {
            }
            else if (cardData.cardType.Equals(CardType.Legendary))
            {
            }
            else
            {
                Debug.LogError($"Invalid card type {cardData.cardType}");
            }
        }

        private void HandleMovementCards(int playerID, int cardIndex, CardSO cardData)
        {
            var toMoveCell = 0;

            if (playerID == 0)
            {
                toMoveCell = currentCellIndexP1 + cardData.moveTileCount;
            }
            else if (playerID == 1)
            {
                toMoveCell = currentCellIndexP2 + cardData.moveTileCount;
            }
            else if (playerID == 2)
            {
                toMoveCell = currentCellIndexP3 + cardData.moveTileCount;
            }
            else if (playerID == 3)
            {
                toMoveCell = currentCellIndexP4 + cardData.moveTileCount;
            }
            else
            {
                Debug.LogError($"Invalid player index {playerID}");
            }

            players[playerID].MoveToCell(toMoveCell, cardIndex);
        }

        public void FinishPlayerTurn(int playerID, int usedCardIndex, int finalCellReachedIndex)
        {
            OnPlayerUsedCard?.Invoke(playerID, usedCardIndex);

            if (playerID == 0)
            {
                currentCellIndexP1 = finalCellReachedIndex;
            }
            else if (playerID == 1)
            {
                currentCellIndexP2 = finalCellReachedIndex;
            }
            else if (playerID == 2)
            {
                currentCellIndexP3 = finalCellReachedIndex;
            }
            else if (playerID == 3)
            {
                currentCellIndexP4 = finalCellReachedIndex;
            }
            else
            {
                Debug.LogError($"Invalid player index {playerID}");
            }

            currentPlayerTurn++;

            if (currentPlayerTurn >= playerCount)
            {
                currentPlayerTurn = 0;
            }

            OnPlayerTurnFinished?.Invoke(currentPlayerTurn);
            
            canPreformAction = true;
        }
    }
}