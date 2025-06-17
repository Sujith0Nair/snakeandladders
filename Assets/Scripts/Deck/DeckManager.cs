using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;

namespace Deck
{
    public class DeckManager : MonoBehaviour
    {
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

        private List<CardSO> deck;
        private Queue<CardSO> pile;

        private List<Card>[] playerHands;

        private void Start()
        {
            deck = new List<CardSO>();
            pile = new Queue<CardSO>();
            playerHands = new List<Card>[GameManager.Instance.playerCount];

            player1CardAttachPoint.SetActive(GameManager.Instance.playerCount >= 1);
            player2CardAttachPoint.SetActive(GameManager.Instance.playerCount >= 2);
            player3CardAttachPoint.SetActive(GameManager.Instance.playerCount >= 3);
            player4CardAttachPoint.SetActive(GameManager.Instance.playerCount >= 4);

            GameManager.Instance.OnPlayerUsedCard += OnPlayerUsedCard;

            ResetHaltUIs();
            ResetSwapPlayerUIs();
            ResetRetreatCancelUIs();
            ResetForcePlayerToSnakeUIs();

            InitializeDeck();
            DealCardsToPlayers(deckSize);
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPlayerUsedCard -= OnPlayerUsedCard;
        }

        private void OnPlayerUsedCard(int playerID, int cardIndex)
        {
            // Debug.LogError($"Player Index -> {playerID} & Card Index -> {cardIndex}");
            var usedCard = playerHands[playerID][cardIndex].cardData;
            var newCard = GetNewCardFromPile();
            Debug.LogError($"ID {playerID + 1} Used Card -> {usedCard.cardName} & New Card -> {newCard.cardName}");
            playerHands[playerID][cardIndex].UpdateData(newCard);
            AddCardToPile(usedCard);
        }

        private void InitializeDeck()
        {
            deck.Clear();

            foreach (var entry in allCardTypes)
            {
                for (int i = 0; i < entry.cardDeckCount; i++)
                {
                    deck.Add(entry);
                }
            }
        }

        private void DealCardsToPlayers(int cardsPerPlayer)
        {
            var movementCards = new List<CardSO>();
            var otherCards = new List<CardSO>();

            foreach (var card in deck)
            {
                if (card.cardType == CardType.MovementCards)
                {
                    movementCards.Add(card);
                }
                else
                {
                    otherCards.Add(card);
                }
            }

            ShuffleList(movementCards);

            pile = new Queue<CardSO>(movementCards);

            for (int i = 0; i < GameManager.Instance.playerCount; i++)
            {
                playerHands[i] = new List<Card>();

                for (int j = 0; j < cardsPerPlayer; j++)
                {
                    if (pile.Count > 0)
                    {
                        var cardData = GetNewCardFromPile();
                        SpawnCardUIToPlayerDeck(cardData, i, j);
                    }
                    else
                    {
                        Debug.LogError("Not enough movement cards to deal full hands.");
                        break;
                    }
                }
            }

            var remainingPile = pile.ToList(); //get dealt pile
            remainingPile.AddRange(otherCards); //add other cards

            ShuffleList(remainingPile); //Shuffle the full pile

            pile = new Queue<CardSO>(remainingPile); //recreate pile

            deck.Clear();
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

        private CardSO GetNewCardFromPile()
        {
            return pile.Dequeue();
        }

        private void AddCardToPile(CardSO card)
        {
            pile.Enqueue(card);
        }

        private void SpawnCardUIToPlayerDeck(CardSO cardData, int playerIndex, int cardIndex)
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

            var spawnedCard = Instantiate(deckCardUIPrefab, cardParent).GetComponent<Card>();
            spawnedCard.SetupData(cardData, playerIndex, cardIndex);

            playerHands[playerIndex].Add(spawnedCard);
        }

        public bool CheckIfPlayerHasRetreatCard(int playerID)
        {
            return playerHands[playerID]
                .Find(x => x.cardData.cardType.Equals(CardType.ActionCards) &&
                           x.cardData.actionCardType.Equals(ActionCardType.Retreat));
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
            var foundCard = playerHands[playerID]
                .Find(x => x.cardData.cardType.Equals(CardType.DefensiveCards) &&
                           x.cardData.defensiveCardType.Equals(DefensiveCardType.HoldYourGround));

            cardIndex = foundCard ? foundCard.cardIndex : -1;

            return foundCard;
        }

        public bool HasSnakeTamer(int playerID, out int cardIndex)
        {
            var foundCard = playerHands[playerID]
                .Find(x => x.cardData.cardType.Equals(CardType.DefensiveCards) &&
                           x.cardData.defensiveCardType.Equals(DefensiveCardType.SnakeTamer));

            cardIndex = foundCard ? foundCard.cardIndex : -1;

            return foundCard;
        }
    }
}