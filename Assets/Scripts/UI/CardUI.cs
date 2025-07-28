using Deck;
using SaL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SaL.UI
{
    /// <summary>
    /// Manages the visual representation and interaction of a single card in the player's hand.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _cardArt;
        // [SerializeField] private TooltipTrigger _tooltip; // Assuming a tooltip script exists

        private CardSO _cardData;
        private bool _isInteractable;
        private Vector3 _originalPosition;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
            _originalPosition = transform.position;
        }

        /// <summary>
        /// Initializes the card with its ScriptableObject data.
        /// </summary>
        public void Initialize(CardSO data)
        {
            _cardData = data;
            _nameText.text = _cardData.CardName;
            // if (_tooltip != null)
            // {
            //     _tooltip.message = _cardData.cardDescription;
            // }
        }

        /// <summary>
        /// Sets whether the card can be interacted with (i.e., if it's the player's turn).
        /// </summary>
        public void SetInteractable(bool isInteractable)
        {
            _isInteractable = isInteractable;
        }

        /// <summary>
        /// Called when the card's Button component is clicked.
        /// </summary>
        public void OnClick()
        {
            if (_isInteractable && _cardData != null)
            {
                ClientController.Instance.OnCardClicked(_cardData.CardId);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isInteractable)
            {
                // Simple hover effect: move up slightly.
                transform.position = _originalPosition + new Vector3(0, 20f, 0);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Reset position when hover ends.
            transform.position = _originalPosition;
        }
    }
}
