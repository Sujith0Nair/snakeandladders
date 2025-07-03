using Game;
using TMPro;
using UnityEngine;

namespace GameUI
{
    public class GameUIController : MonoBehaviour
    {
        public static GameUIController Instance;

        [SerializeField] private TextMeshProUGUI currentPlayerTurnText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
            GameManager.Instance.OnPlayerTurnFinished -= OnPlayerTurnFinished;
        }

        public void Init()
        {
            GameManager.Instance.OnPlayerTurnFinished += OnPlayerTurnFinished;
        }

        private void OnPlayerTurnFinished(int currentPlayerID)
        {
            currentPlayerTurnText.text = $"Player {currentPlayerID + 1} Turn!";
        }
    }
}