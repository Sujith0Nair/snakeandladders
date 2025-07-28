using UnityEngine;
using NaughtyAttributes;
using JetBrains.Annotations;

namespace Deck
{
    [CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
    public class CardSO : ScriptableObject
    {
        [Header("Details")] 
        [SerializeField] private int cardId;
        [SerializeField] private string cardName;
        [SerializeField] private int cardDeckCount;
        [SerializeField] private CardType cardType;
        [SerializeField] private string cardDescription;

        [Header("Movement Type Data")]
        [ShowIf("cardType", CardType.Movement)]
        [SerializeField] private int moveTileCount;

        [Header("Action Type Data")] 
        [ShowIf("cardType", CardType.Action)]
        [SerializeField] private ActionCardType actionCardType;
        [ShowIf("IsCardRetreat")]
        [SerializeField] private int retreatMoveTileCount;
        [ShowIf("cardType", CardType.Action)]
        [SerializeField] private DefensiveCardType counteredBy;
        [ShowIf("cardType", CardType.Action)]
        [SerializeField] private EffectDuration effectDuration;

        [Header("Defensive Card Data")] 
        [ShowIf("cardType", CardType.Defensive)]
        [SerializeField] private DefensiveCardType defensiveCardType;
        
        [UsedImplicitly] public bool IsCardRetreat => cardType == CardType.Action && actionCardType == ActionCardType.Retreat;
        public string CardName => cardName;
        public int CardId => cardId;
        public int CardDeckCount => cardDeckCount;
        public int MoveTileCount => moveTileCount;
        public int RetreatMoveTileCount => retreatMoveTileCount;
        public DefensiveCardType CounteredBy => counteredBy;
        public ActionCardType ActionType => actionCardType;
        public EffectDuration Duration => effectDuration;
        public DefensiveCardType DefensiveType => defensiveCardType;
        public CardType BaseCardType => cardType;

        public enum CardType
        {
            Movement,
            Action,
            Defensive
        }

        public enum EffectDuration
        {
            Turn,
            Round
        }

        public enum ActionCardType
        {
            Retreat,
            FreezeOpponent,
            SwapPositions,
            LadderVandalism,
            ForceToSnake,
            LadderLockout,
        }

        public enum DefensiveCardType
        {
            None,
            SnakeTamer,
            HoldYourGround,
            SlipperyFeet
        }
    }
}