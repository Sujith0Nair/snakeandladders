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
        
        [Header("Legendary Card Data")]
        public LegendaryCardType legendaryCardType;
    }

    public enum CardType
    {
        MovementCards,
        ActionCards,
        Legendary
    }

    public enum ActionCardType
    {
        Retreat,
        Halt,
        SwapPositions,
        LadderVandalism,
    }

    public enum LegendaryCardType
    {
        SnakeTamer,
        LadderLockout,
        TemporalShift
    }
}