using Game;
using TMPro;
using UnityEngine;

namespace Deck
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText;

        public CardSO cardData { get; private set; }
        public int cardIndexInDeck { get; private set; }
        public int cardIndexInUI { get; private set; }

        private int playerID;

        public void SetupData(CardSO cardData, int playerID, int cardIndexInDeck, int cardIndexInUI)
        {
            this.cardData = cardData;
            this.playerID = playerID;
            this.cardIndexInDeck = cardIndexInDeck;
            this.cardIndexInUI = cardIndexInUI;

            UpdateUI();
        }

        public void UpdateData(CardSO cardData)
        {
            this.cardData = cardData;
            UpdateUI();
        }

        public void PlayCard()
        {
            GameManager.Instance.PlayCard(playerID, cardIndexInDeck, cardIndexInUI);
        }

        private void UpdateUI()
        {
            cardNameText.text = cardData.name;
        }
    }
}