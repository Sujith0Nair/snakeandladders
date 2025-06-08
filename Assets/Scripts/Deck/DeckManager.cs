using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private int deckSize;
    [SerializeField] private GameObject deckCardUIPrefab;

    [SerializeField] private GameObject player1CardAttachPoint;
    [SerializeField] private GameObject player2CardAttachPoint;
    [SerializeField] private GameObject player3CardAttachPoint;
    [SerializeField] private GameObject player4CardAttachPoint;

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

        InitializeDeck();
        ShuffleDeck();
        DealCardsToPlayers(deckSize);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerUsedCard -= OnPlayerUsedCard;
    }

    private void OnPlayerUsedCard(int playerIndex, int cardIndex)
    {
        Debug.LogError($"Player Index -> {playerIndex} & Card Index -> {cardIndex}");

        var usedCard = playerHands[playerIndex][cardIndex].cardData;
        playerHands[playerIndex][cardIndex].UpdateData(GetNewCardFromPile());
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

    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rnd = Random.Range(i, deck.Count);
            var temp = deck[i];
            deck[i] = deck[rnd];
            deck[rnd] = temp;
        }
    }

    private void DealCardsToPlayers(int cardsPerPlayer)
    {
        for (int i = 0; i < GameManager.Instance.playerCount; i++)
        {
            playerHands[i] = new List<Card>();

            for (int j = 0; j < cardsPerPlayer; j++)
            {
                if (deck.Count > 0)
                {
                    var cardData = deck[0];
                    deck.RemoveAt(0);

                    //Spawn Card UI to play Deck
                    SpawnCardUIToPlayerDeck(cardData, i, j);
                }
            }
        }

        // Remaining goes to pile
        foreach (var card in deck) pile.Enqueue(card);

        deck.Clear();
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
}