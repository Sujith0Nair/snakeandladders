using System;
using System.Collections;
using System.Collections.Generic;
using _Main;
using Board;
using Deck;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        [SerializeField] private Camera raycastCamera;
        [SerializeField] private LayerMask ladderLayer = -1;
        [SerializeField] private float maxRaycastDistance = 100f;

        internal int currentPlayerTurn;

        public Action<int> OnPlayerTurnFinished;
        public Action<int, int> OnPlayerUsedCard;

        private List<PlayerController> players;
        
        public IReadOnlyList<PlayerController> Players => players;

        private bool canPreformAction;

        private bool isRetreatCardInUse;
        private int totalRetreatMoveCount;

        private bool isHaltCardInUse;
        private int haltPlayerID;

        private bool checkForLadderSelectRaycast;

        private int lastUsedCardIndex;

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

        private IEnumerator Start()
        {
            playerCount = World.Get.Board.PlayerCountInMatch;
            
            canPreformAction = true;

            currentPlayerTurn = 0;

            players = new List<PlayerController>();
            
            var currentScene = gameObject.scene;
            
            yield return new WaitUntil(() => SceneManager.GetActiveScene() == currentScene);

            SpawnPlayer();
        }

        private void Update()
        {
            if (checkForLadderSelectRaycast)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    SelectLadder();
                }
            }
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

        private void SelectLadder()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, ladderLayer))
            {
                GameObject selectedObject = hit.collider.gameObject;
                Debug.LogError($"Selected Ladder: {selectedObject.name}", selectedObject);

                var ladderScript = selectedObject.GetComponent<Ladder>();
                ladderScript.BlockLadder();

                checkForLadderSelectRaycast = false;
                FinishPlayerTurn(currentPlayerTurn, lastUsedCardIndex);
                ResetLadderBlockFlags();
            }
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

            canPreformAction = false;

            if (cardData.cardType.Equals(CardType.MovementCards))
            {
                HandleMovementCards(playerID, cardIndex, cardData);
            }
            else if (cardData.cardType.Equals(CardType.ActionCards))
            {
                HandleActionCard(playerID, cardIndex, cardData);
            }
            else if (cardData.cardType.Equals(CardType.DefensiveCards))
            {
                Debug.LogError($"Can't Use Defensive Card");
                canPreformAction = true;
            }
            else
            {
                Debug.LogError($"Invalid card type {cardData.cardType}");
            }
        }

        private void HandleMovementCards(int playerID, int cardIndex, CardSO cardData)
        {
            //To Check If Player is Valid Move Card
            if (players[playerID].CurrentCellIndex + playerID <= 100)
            {
                players[playerID].MoveToCell(cardData.moveTileCount, cardIndex, true);
            }
            else
            {
                Debug.LogError($"Cant Choose This Card!");
            }
        }

        private void HandleActionCard(int playerID, int cardIndex, CardSO cardData)
        {
            if (cardData.actionCardType.Equals(ActionCardType.Retreat))
            {
                HandleRetreat(playerID, cardIndex, cardData);
            }
            else if (cardData.actionCardType.Equals(ActionCardType.Halt))
            {
                HandleHalt(cardIndex);
            }
            else if (cardData.actionCardType.Equals(ActionCardType.SwapPositions))
            {
                HandleSwapPositions(cardIndex);
            }
            else if (cardData.actionCardType.Equals(ActionCardType.LadderVandalism))
            {
                HandleLadderVandalism(cardIndex);
            }
            else if (cardData.actionCardType.Equals(ActionCardType.ForceToSnake))
            {
                HandleForceToSnake(cardIndex);
            }
            else if (cardData.actionCardType.Equals(ActionCardType.LadderLockout))
            {
                HandleLadderLockOut(playerID, cardIndex);
            }
            else
            {
                Debug.LogError($"Invalid Action card type {cardData.cardType}");
            }
        }

        private void HandleRetreat(int playerID, int cardIndex, CardSO cardData)
        {
            deckManager.ResetRetreatCancelUIs();
            
            isRetreatCardInUse = true;
            totalRetreatMoveCount += cardData.retreatMoveTileCount;

            FinishPlayerTurn(playerID, cardIndex);
        }

        private void HandleHalt(int cardIndex)
        {
            lastUsedCardIndex = cardIndex;
            isHaltCardInUse = true;
            deckManager.ShowHaltUI(currentPlayerTurn);
        }

        private void HandleSwapPositions(int cardIndex)
        {
            lastUsedCardIndex = cardIndex;
            deckManager.ShowSwapPositionUI(currentPlayerTurn);
        }

        private void HandleLadderVandalism(int cardIndex)
        {
            checkForLadderSelectRaycast = true;
            lastUsedCardIndex = cardIndex;
        }

        private void HandleForceToSnake(int cardIndex)
        {
            lastUsedCardIndex = cardIndex;
            deckManager.ShowForcePlayerToSnakeUI(currentPlayerTurn);
        }

        private void HandleLadderLockOut(int playerID, int cardIndex)
        {
            board.BlockAllLadders();
            FinishPlayerTurn(playerID, cardIndex);
        }

        private void HandleSnakeTamer(int playerID, int cardIndex)
        {
            OnPlayerUsedCard?.Invoke(playerID, cardIndex);
        }

        private void HandleHoldYourGround(int playerID, int cardIndex)
        {
            OnPlayerUsedCard?.Invoke(playerID, cardIndex);
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
                //Player Can Still play if some action event happened (Retreat)
                canPreformAction = true;
                ResetRetreatCardFlags();
                return;
            }

            UpdateToNextPlayerTurn();
        }

        private void UpdateToNextPlayerTurn()
        {
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

            //Make Sure Player Moves In Same Index To Trigger Snake Check
            foreach (var player in players)
            {
                player.MoveToCell(0, -1, false);
            }

            //UnBlock All Ladders
            board.UnBlockAllLadders();

            TurnCompleteCheck();
        }

        private void TurnCompleteCheck()
        {
            OnPlayerTurnFinished?.Invoke(currentPlayerTurn);

            if (isRetreatCardInUse)
            {
                CheckForCurrentPlayerHasRetreatCard();
            }
            else if (isHaltCardInUse && haltPlayerID == currentPlayerTurn)
            {
                SkipPlayerTurn();
            }
            else
            {
                canPreformAction = true;
                ResetRetreatCardFlags();
            }
        }

        private void CheckForCurrentPlayerHasRetreatCard()
        {
            if (!deckManager.CheckIfPlayerHasRetreatCard(currentPlayerTurn))
            {
                //Player Move Back
                players[currentPlayerTurn].MoveToCell(-totalRetreatMoveCount, -1, true);
                isRetreatCardInUse = false;
            }
            else
            {
                canPreformAction = true;
                deckManager.ShowRetreatCancelUI(currentPlayerTurn);
            }
        }

        private void SkipPlayerTurn()
        {
            Debug.LogError($"Player {currentPlayerTurn + 1} is Skipped");
            ResetHaltCardFlags();
            UpdateToNextPlayerTurn();
        }

        private void ResetRetreatCardFlags()
        {
            isRetreatCardInUse = false;
            totalRetreatMoveCount = 0;
        }

        private void ResetHaltCardFlags()
        {
            isHaltCardInUse = false;
            haltPlayerID = -1;
            lastUsedCardIndex = -1;
        }

        private void ResetSwapPositionFlags()
        {
            lastUsedCardIndex = -1;
        }

        private void ResetLadderBlockFlags()
        {
            lastUsedCardIndex = -1;
        }

        private void ResetForcePlayerToSnakeFlags()
        {
            lastUsedCardIndex = -1;
        }

        public void CancelRetreat()
        {
            deckManager.ResetRetreatCancelUIs();
            players[currentPlayerTurn].MoveToCell(-totalRetreatMoveCount, -1, true);
            ResetRetreatCardFlags();
        }

        public void BlockPlayer(int playerID)
        {
            Debug.LogError($"Player {playerID + 1} is Selected for Skip Turn");
            deckManager.ResetHaltUIs();
            haltPlayerID = playerID;
            FinishPlayerTurn(currentPlayerTurn, lastUsedCardIndex);
        }

        public void SwapPlayer(int playerID)
        {
            Debug.LogError($"Player {playerID + 1} is Selected for Swap Turn");

            deckManager.ResetSwapPlayerUIs();

            //Check if We Defence Card Hold Your Ground
            if (!deckManager.HasHoldYourGround(playerID, out var specialCardIndex))
            {
                var currentPlayerCellIndex = players[currentPlayerTurn].CurrentCellIndex;
                var swapPlayerCellIndex = players[playerID].CurrentCellIndex;
                var currentPlayerMoveCount = 0;
                var swapPlayerMoveCount = 0;

                //Check if we need Move Forward
                if (currentPlayerCellIndex <= swapPlayerCellIndex)
                {
                    currentPlayerMoveCount = swapPlayerCellIndex - currentPlayerCellIndex;
                    swapPlayerMoveCount = -currentPlayerMoveCount;
                }
                else
                {
                    currentPlayerMoveCount = swapPlayerCellIndex - currentPlayerCellIndex;
                    swapPlayerMoveCount = -currentPlayerMoveCount;
                }

                players[currentPlayerTurn].MoveToCell(currentPlayerMoveCount, -1, false);
                players[playerID].MoveToCell(swapPlayerMoveCount, -1, false);
            }
            else
            {
                Debug.LogError($"Player {playerID + 1} Used Hold Your Ground!");
                HandleHoldYourGround(playerID, specialCardIndex);
            }

            FinishPlayerTurn(currentPlayerTurn, lastUsedCardIndex);

            ResetSwapPositionFlags();
        }

        public void ForceToSnake(int playerID)
        {
            Debug.LogError($"Player {playerID + 1} is Selected for Force To Snake Turn");

            deckManager.ResetForcePlayerToSnakeUIs();

            //Check if We Defence Card Snake Tamer
            if (!deckManager.HasSnakeTamer(playerID, out var specialCardIndex))
            {
                var currentPlayerCellIndex = players[playerID].CurrentCellIndex;
                var closestSnakeIndex = board.GetClosestSnakeIndex(players[playerID].CurrentCellIndex);
                var moveCount = 0;

                //Check if we need Move Forward
                if (currentPlayerCellIndex <= closestSnakeIndex)
                {
                    moveCount = closestSnakeIndex - currentPlayerCellIndex;
                }
                else
                {
                    moveCount = -(closestSnakeIndex - currentPlayerCellIndex);
                }

                players[playerID].MoveToCell(moveCount, -1, false);
            }
            else
            {
                Debug.LogError($"Player {playerID + 1} Used Snake Tamer!");
                HandleSnakeTamer(playerID, specialCardIndex);
            }

            FinishPlayerTurn(currentPlayerTurn, lastUsedCardIndex);

            ResetForcePlayerToSnakeFlags();
        }
    }
}