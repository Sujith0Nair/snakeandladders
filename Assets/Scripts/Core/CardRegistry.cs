using System.Collections.Generic;
using System.Linq;
using Deck;
using UnityEngine;

namespace SaL.Core
{
    /// <summary>
    /// A client-side registry to easily map a Card ID to its ScriptableObject asset.
    /// </summary>
    public class CardRegistry : MonoBehaviour
    {
        public static CardRegistry Instance { get; private set; }
        
        [SerializeField] private List<CardSO> _cardAssets;
        private Dictionary<int, CardSO> _cardMap;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                BuildMap();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void BuildMap()
        {
            _cardMap = _cardAssets.ToDictionary(card => card.CardId, card => card);
        }

        public CardSO GetCardById(int cardId)
        {
            _cardMap.TryGetValue(cardId, out var card);
            return card;
        }
    }
}
