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
    }

    public enum CardType
    {
        MovementCards,
        ActionCards,
        Legendary
    }
}