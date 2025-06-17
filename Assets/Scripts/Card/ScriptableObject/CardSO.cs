using UnityEngine;

namespace Deck
{
    [CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
    public class CardSO : ScriptableObject
    {
        [Header("Details")] 
        public string cardName;
        public int cardDeckCount;
        public CardType cardType;

        [Header("Movement Type Data")] 
        public int moveTileCount;

        [Header("Action Type Data")] 
        public ActionCardType actionCardType;
        public int retreatMoveTileCount;

        [Header("Defensive Card Data")] 
        public DefensiveCardType defensiveCardType;
    }

    public enum CardType
    {
        MovementCards,
        ActionCards,
        DefensiveCards
    }

    public enum ActionCardType
    {
        Retreat,
        Halt,
        SwapPositions,
        LadderVandalism,
        ForceToSnake,
        LadderLockout
    }

    public enum DefensiveCardType
    {
        SnakeTamer,
        HoldYourGround
    }
}