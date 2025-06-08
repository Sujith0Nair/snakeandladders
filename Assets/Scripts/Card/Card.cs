using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;

    public CardSO cardData { get; private set; }
    private int playerIndex;
    private int cardIndex;

    public void SetupData(CardSO cardData, int playerIndex, int cardIndex)
    {
        this.cardData = cardData;
        this.playerIndex = playerIndex;
        this.cardIndex = cardIndex;

        UpdateUI();
    }

    public void UpdateData(CardSO cardData)
    {
        this.cardData = cardData;
        UpdateUI();
    }

    public void PlayCard()
    {
        if (GameManager.Instance.currentPlayerTurn != playerIndex)
        {
            Debug.LogError($"Not Ur Turn!");
            return;
        }

        Debug.LogError($"Player {playerIndex + 1} Used {cardData.name}");
        GameManager.Instance.FinishPlayerTurn(playerIndex, cardIndex);
    }

    public void UpdateUI()
    {
        cardNameText.text = cardData.name;
    }
}