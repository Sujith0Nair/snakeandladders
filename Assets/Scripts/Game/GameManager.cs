using System;
using System.Collections.Generic;
using _Main;
using Board;
using Deck;
using GameUI;
using Player;
using Unity.Netcode;
using UnityEngine;

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

        public void Init()
        {
            playerCount = World.Get.Board.PlayerCountInMatch;

            canPreformAction = true;

            currentPlayerTurn = 0;

            players = new List<PlayerController>(playerCount);

            Networking.Server.GameManager.Instance.RegisterAsLocalPlayer_RPC(Networking.Server.GameManager.Instance
                .LocalClientId);

            SpawnPlayer();

            DeckManager.Instance.Init();
            GameUIController.Instance.Init();
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
                playerController.Init(i, GetPlayerColor(i));
                players.Add(playerController);
            }
        }

        public Color GetPlayerColor(int playerID)
        {
            return playerID switch
            {
                0 => player1Color,
                1 => player2Color,
                2 => player3Color,
                _ => player4Color
            };
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

        public void PlayCard(int playerID, int cardIndexInDeck, int cardIndexInUI)
        {
            if (!Networking.Server.GameManager.Instance.IsSessionOwner)
            {
                Networking.Server.GameManager.Instance.PlayCard_RPC(playerID, cardIndexInDeck,cardIndexInUI);
                return;
            }
            
            if (!canPreformAction)
            {
                return;
            }

            if (currentPlayerTurn != playerID)
            {
                Debug.LogError($"Not Ur Turn!");
                return;
            }

            var cardData = DeckManager.Instance.GetCardData(cardIndexInDeck);

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
                HandleMovementCards(playerID, cardIndexInUI, cardData);
            }
            else if (cardData.cardType.Equals(CardType.ActionCards))
            {
                HandleActionCard(playerID, cardIndexInUI, cardData);
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

        private void HandleMovementCards(int playerID, int cardIndexInUI, CardSO cardData)
        {
            //To Check If Player is Valid Move Card
            if (players[playerID].CurrentCellIndex + cardData.moveTileCount <= 100)
            {
                players[playerID].MoveToCell(cardData.moveTileCount, cardIndexInUI, true);
                Networking.Server.GameManager.Instance.MovePlayerToCell_RPC(playerID, cardData.moveTileCount,
                    cardIndexInUI);
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

        public void FinishPlayerTurn(int playerID, int usedCardIndexInUI)
        {
            //When Action Happens Without Card No Need To Trigger Event
            if (usedCardIndexInUI >= 0)
            {
                //Only Run On LocalPlayer
                if (playerID == Networking.Server.GameManager.Instance.LocalPlayerID)
                {
                    //Will Updated Card Used & Pick A New Card From Pile
                    OnPlayerUsedCard?.Invoke(playerID, usedCardIndexInUI);
                }
            }
            else
            {
                //Only Run On Server
                if (!Networking.Server.GameManager.Instance.IsSessionOwner)
                {
                    return;
                }
                
                //Player Can Still play if some action event happened (Retreat)
                canPreformAction = true;
                ResetRetreatCardFlags();
                return;
            }

            //Only Run On Server
            if (Networking.Server.GameManager.Instance.IsSessionOwner)
            {
                UpdateToNextPlayerTurn();
            }
        }

        private void UpdateToNextPlayerTurn()
        {
            currentPlayerTurn++;

            if (currentPlayerTurn >= playerCount)
            {
                currentPlayerTurn = 0;
                Networking.Server.GameManager.Instance.OnRoundCompleted();
            }
            else
            {
                TurnCompleteCheck();
            }
        }

        public void UpdateCanPerformAction(bool canPerformAction)
        {
            this.canPreformAction = canPerformAction;
        }

        // private void RoundCompleted()
        // {   
        //     var randomDiceRoll = Random.Range(1, 7);
        //     Debug.LogError($"Dice Roll By AI : {randomDiceRoll}");
        //     
        //     if (randomDiceRoll is 1 or 6)
        //     {
        //         Networking.Server.GameManager.Instance.RandomiseSnakePositions();
        //     }
        //
        //     //Make Sure Player Moves In Same Index To Trigger Snake Check
        //     foreach (var player in players)
        //     {
        //         player.MoveToCell(0, -1, false);
        //     }
        //
        //     //UnBlock All Ladders
        //     board.UnBlockAllLadders();
        //
        //     TurnCompleteCheck();
        // }

        public void TryMovingPlayerToCheckForSnake()
        {
            var player = players[Networking.Server.GameManager.Instance.LocalPlayerID];
            player.MoveToCell(0, -1, false);
        }

        public void TurnCompleteCheck()
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

        public void MovePlayerToCell(int playerID, int moveCount, int cardIndexInUI)
        {
            players[playerID].MoveToCell(moveCount, cardIndexInUI, true);
        }
    }
}