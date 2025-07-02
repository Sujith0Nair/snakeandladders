using System.Collections.Generic;
using System.Linq;
using _Main;
using Game;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Deck
{
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance;
        
        [SerializeField] private int deckSize;
        [SerializeField] private GameObject deckCardUIPrefab;

        [SerializeField] private GameObject player1CardAttachPoint;
        [SerializeField] private GameObject player2CardAttachPoint;
        [SerializeField] private GameObject player3CardAttachPoint;
        [SerializeField] private GameObject player4CardAttachPoint;

        [SerializeField] private List<GameObject> retreatCancelUIs;
        [SerializeField] private List<GameObject> haltUIs;
        [SerializeField] private List<GameObject> swapPlayerUIs;
        [SerializeField] private List<GameObject> forceToSnakeUIs;

        public List<CardSO> allCardTypes;

        private List<CardSO> deckMap;
        private Queue<int> pile;
        private List<int> playerHands;
        
        private int moveCardMaxIndex;
        
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
            GameManager.Instance.OnPlayerUsedCard -= OnPlayerUsedCard;
        }

        public void Init()
        {
            deckMap = new();
            pile = new();
            playerHands = new();

            GameManager.Instance.OnPlayerUsedCard += OnPlayerUsedCard;

            ResetHaltUIs();
            ResetSwapPlayerUIs();
            ResetRetreatCancelUIs();
            ResetForcePlayerToSnakeUIs();

            InitializeDeck();
        }

        public void SetupDeckUI()
        {
            player1CardAttachPoint.SetActive(Networking.Server.GameManager.Instance.LocalPlayerIndex == 0);
            player2CardAttachPoint.SetActive(Networking.Server.GameManager.Instance.LocalPlayerIndex == 1 );
            player3CardAttachPoint.SetActive(Networking.Server.GameManager.Instance.LocalPlayerIndex == 2);
            player4CardAttachPoint.SetActive(Networking.Server.GameManager.Instance.LocalPlayerIndex == 3 );
        }

        private void OnPlayerUsedCard(int playerID, int cardIndex)
        {
            /*// Debug.LogError($"Player Index -> {playerID} & Card Index -> {cardIndex}");
            var usedCard = playerHands[playerID][cardIndex].cardData;
            var newCard = GetNewCardFromPile();
            Debug.LogError($"ID {playerID + 1} Used Card -> {usedCard.cardName} & New Card -> {newCard.cardName}");
            playerHands[playerID][cardIndex].UpdateData(newCard);
            AddCardToPile(usedCard);*/
        }

        private void InitializeDeck()
        {
            deckMap.Clear();

            foreach (var entry in allCardTypes)
            {
                for (var i = 0; i < entry.cardDeckCount; i++)
                {
                    if (entry.cardType.Equals(CardType.MovementCards))
                    {
                        moveCardMaxIndex++;
                    }
                    
                    deckMap.Add(entry);
                }
            }
        }

        public void DealCardsToPlayers()
        {
            var movementCardIndexList = Enumerable.Range(0, moveCardMaxIndex).ToList();
            ShuffleList(movementCardIndexList);
            
            pile = new Queue<int>(Enumerable.Reverse(movementCardIndexList));

            for (int j = 0; j < deckSize; j++)
            {
                foreach (var connectedClientsId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (pile.Count > 0)
                    {
                        var cardIndexOfDeck = GetNewCardFromPile();
                        Networking.Server.GameManager.Instance.SpawnCardUIToPlayerDeck(connectedClientsId, cardIndexOfDeck);
                    }
                    else
                    {
                        Debug.LogError("Not enough movement cards to deal full hands.");
                        break;
                    }
                }
            }

            var remainingPile = pile.ToList(); //get dealt pile
            remainingPile.AddRange(Enumerable.Range(moveCardMaxIndex, deckMap.Count - moveCardMaxIndex).ToList()); //Add left Over Cards From Deck

            ShuffleList(remainingPile); //Shuffle the full pile

            pile = new Queue<int>(Enumerable.Reverse(remainingPile)); //recreate pile
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int rnd = Random.Range(i, list.Count);
                (list[i], list[rnd]) = (list[rnd], list[i]);
            }
        }

        public void ResetHaltUIs()
        {
            foreach (var haltUI in haltUIs)
            {
                haltUI.SetActive(false);
            }
        }

        public void ResetSwapPlayerUIs()
        {
            foreach (var swapPlayerUI in swapPlayerUIs)
            {
                swapPlayerUI.SetActive(false);
            }
        }

        public void ResetRetreatCancelUIs()
        {
            foreach (var retreatCancelUI in retreatCancelUIs)
            {
                retreatCancelUI.SetActive(false);
            }
        }

        public void ResetForcePlayerToSnakeUIs()
        {
            foreach (var forceToSnakeUI in forceToSnakeUIs)
            {
                forceToSnakeUI.SetActive(false);
            }
        }

        private int GetNewCardFromPile()
        {
            return pile.Dequeue();
        }

        private void AddCardToPile(int cardIndex)
        {
            pile.Enqueue(cardIndex);
        }

        public void SpawnCardUIToPlayerDeck(int playerIndex, int cardIndexOfDeck)
        {
            Transform cardParent;

            if (playerIndex == 0)
            {
                cardParent = player1CardAttachPoint.transform;
            }
            else if (playerIndex == 1)
            {
                cardParent = player2CardAttachPoint.transform;
            }
            else if (playerIndex == 2)
            {
                cardParent = player3CardAttachPoint.transform;
            }
            else if (playerIndex == 3)
            {
                cardParent = player4CardAttachPoint.transform;
            }
            else
            {
                Debug.LogError($"{playerIndex} is not a valid player index");
                return;
            }

            var cardData = deckMap[cardIndexOfDeck];
            
            var spawnedCard = Instantiate(deckCardUIPrefab, cardParent).GetComponent<Card>();
            spawnedCard.SetupData(cardData, playerIndex, cardIndexOfDeck);

            playerHands.Add(cardIndexOfDeck);
        }

        public bool CheckIfPlayerHasRetreatCard(int playerID)
        {
            return false;
            /*return playerHands[playerID]
                .Find(x => x.cardData.cardType.Equals(CardType.ActionCards) &&
                           x.cardData.actionCardType.Equals(ActionCardType.Retreat));*/
        }

        public void ShowRetreatCancelUI(int currentPlayerTurn)
        {
            for (int i = 0; i < GameManager.Instance.playerCount; i++)
            {
                if (i == currentPlayerTurn)
                {
                    retreatCancelUIs[i].SetActive(true);
                }
            }
        }

        public void ShowHaltUI(int currentPlayerTurn)
        {
            for (int i = 0; i < GameManager.Instance.playerCount; i++)
            {
                if (i != currentPlayerTurn)
                {
                    haltUIs[i].SetActive(true);
                }
            }
        }

        public void ShowSwapPositionUI(int currentPlayerTurn)
        {
            for (int i = 0; i < GameManager.Instance.playerCount; i++)
            {
                if (i != currentPlayerTurn)
                {
                    swapPlayerUIs[i].SetActive(true);
                }
            }
        }

        public void ShowForcePlayerToSnakeUI(int currentPlayerTurn)
        {
            for (int i = 0; i < GameManager.Instance.playerCount; i++)
            {
                if (i != currentPlayerTurn)
                {
                    forceToSnakeUIs[i].SetActive(true);
                }
            }
        }

        public bool HasHoldYourGround(int playerID, out int cardIndex)
        {
            cardIndex = -1;
            return false;
            /*var foundCard = playerHands[playerID]
                .Find(x => x.cardData.cardType.Equals(CardType.DefensiveCards) &&
                           x.cardData.defensiveCardType.Equals(DefensiveCardType.HoldYourGround));

            cardIndex = foundCard ? foundCard.cardIndex : -1;

            return foundCard;*/
        }

        public bool HasSnakeTamer(int playerID, out int cardIndex)
        {
            cardIndex = -1;
            return false;
            /*var foundCard = playerHands[playerID]
                .Find(x => x.cardData.cardType.Equals(CardType.DefensiveCards) &&
                           x.cardData.defensiveCardType.Equals(DefensiveCardType.SnakeTamer));

            cardIndex = foundCard ? foundCard.cardIndex : -1;

            return foundCard;*/
        }
    }
}