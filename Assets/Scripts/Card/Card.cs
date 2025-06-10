using Game;
using TMPro;
using UnityEngine;

namespace Deck
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText;

        public CardSO cardData { get; private set; }
        private int playerID;
        private int cardIndex;

        public void SetupData(CardSO cardData, int playerID, int cardIndex)
        {
            this.cardData = cardData;
            this.playerID = playerID;
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
            GameManager.Instance.PlayCard(playerID, cardIndex, cardData);
        }

        private void UpdateUI()
        {
            cardNameText.text = cardData.name;
        }
    }
}