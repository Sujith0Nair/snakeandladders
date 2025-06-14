using System;
using System.Collections.Generic;
using Board;
using Deck;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private SaLBoard board;
        [SerializeField] private DeckManager deckManager;

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

        private bool isRetreatCardInUse;
        private int totalRetreatMoveCount;

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
                Debug.LogError($"Not Ur Turn!");
                return;
            }

            //Player Can Only Select Retreat card if its Prev Player Used Retreat Card
            if (isRetreatCardInUse)
            {
                if (cardData.cardType.Equals(CardType.ActionCards) &&
                    cardData.actionCardType.Equals(ActionCardType.Retreat))
                {
                }
                else
                {
                    Debug.LogError($"In Retreat Card Mode!");
                    return;
                }
            }

            Debug.LogError($"Player {playerID + 1} has played card {cardData.cardType}");

            canPreformAction = false;

            if (cardData.cardType.Equals(CardType.MovementCards))
            {
                HandleMovementCards(playerID, cardIndex, cardData);
            }
            else if (cardData.cardType.Equals(CardType.ActionCards))
            {
                HandleActionCard(playerID, cardIndex, cardData);
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
            players[playerID].MoveToCell(cardData.moveTileCount, cardIndex);
        }

        private void HandleActionCard(int playerID, int cardIndex, CardSO cardData)
        {
            if (cardData.actionCardType.Equals(ActionCardType.Retreat))
            {
                HandleRetreat(playerID, cardIndex, cardData);
            }
            else if (cardData.actionCardType.Equals(ActionCardType.Halt))
            {
                HandleHalt();
            }
            else if (cardData.actionCardType.Equals(ActionCardType.SwapPositions))
            {
                HandleSwapPositions();
            }
            else if (cardData.actionCardType.Equals(ActionCardType.LadderVandalism))
            {
                HandleLadderVandalism();
            }
        }

        private void HandleRetreat(int playerID, int cardIndex, CardSO cardData)
        {
            isRetreatCardInUse = true;
            totalRetreatMoveCount += cardData.retreatMoveTileCount;

            FinishPlayerTurn(playerID, cardIndex);
        }

        private void HandleHalt()
        {
        }

        private void HandleSwapPositions()
        {
        }

        private void HandleLadderVandalism()
        {
        }

        public void FinishPlayerTurn(int playerID, int usedCardIndex)
        {
            //When Action Happens Without Card No Need To Trigger Event
            if (usedCardIndex >= 0)
            {
                OnPlayerUsedCard?.Invoke(playerID, usedCardIndex);
            }
            else
            {
                //Player Can Still play if some action event happened
                canPreformAction = true;
                ResetCardFlags();
                return;
            }

            currentPlayerTurn++;

            if (currentPlayerTurn >= playerCount)
            {
                currentPlayerTurn = 0;
                RoundCompleted();
            }
            else
            {
                TurnCompleteCheck();
            }
        }

        private void RoundCompleted()
        {
            var randomDiceRoll = Random.Range(1, 7);
            Debug.LogError($"Dice Roll By AI : {randomDiceRoll}");

            if (randomDiceRoll == 1 || randomDiceRoll == 6)
            {
                board.RandomizeSnake();
            }

            TurnCompleteCheck();
        }

        private void TurnCompleteCheck()
        {
            OnPlayerTurnFinished?.Invoke(currentPlayerTurn);

            if (isRetreatCardInUse)
            {
                CheckForCurrentPlayerHasRetreatCard();
            }
            else
            {
                canPreformAction = true;
                ResetCardFlags();
            }
        }

        private void CheckForCurrentPlayerHasRetreatCard()
        {
            if (!deckManager.CheckIfPlayerHasRetreatCard(currentPlayerTurn))
            {
                //Player Move Back
                players[currentPlayerTurn].MoveToCell(-totalRetreatMoveCount, -1);
                isRetreatCardInUse = false;
            }
            else
            {
                canPreformAction = true;
            }
        }

        private void ResetCardFlags()
        {
            isRetreatCardInUse = false;
            totalRetreatMoveCount = 0;
        }
    }
}