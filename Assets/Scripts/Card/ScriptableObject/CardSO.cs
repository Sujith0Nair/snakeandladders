using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
public class CardSO : ScriptableObject
{
    [Header("Details")] public string cardName;
    public int cardDeckCount;
    public CardType cardType;
}

public enum CardType
{
    MovementCards,
    ActionCards,
    Legendary
}