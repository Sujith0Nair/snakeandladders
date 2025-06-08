using TMPro;
using UnityEngine;

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

    private void OnPlayerTurnFinished(int currentPlayerIndex)
    {
        currentPlayerTurnText.text = $"Player {currentPlayerIndex + 1} Turn!";
    }
}