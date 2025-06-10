using Game;
using TMPro;
using UnityEngine;

namespace GameUI
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentPlayerTurnText;

        private void Start()
        {
            GameManager.Instance.OnPlayerTurnFinished += OnPlayerTurnFinished;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPlayerTurnFinished -= OnPlayerTurnFinished;
        }

        private void OnPlayerTurnFinished(int currentPlayerID)
        {
            currentPlayerTurnText.text = $"Player {currentPlayerID + 1} Turn!";
        }
    }
}