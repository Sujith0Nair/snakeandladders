using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] internal int playerCount;

    internal int currentPlayerTurn;

    public Action<int> OnPlayerTurnFinished;
    public Action<int,int> OnPlayerUsedCard;

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
    }

    private void Start()
    {
        currentPlayerTurn = 0;
    }

    public void FinishPlayerTurn(int playerIndex,int usedCardIndex)
    {
        OnPlayerUsedCard?.Invoke(playerIndex, usedCardIndex);

        currentPlayerTurn++;

        if (currentPlayerTurn >= playerCount)
        {
            currentPlayerTurn = 0;
        }

        OnPlayerTurnFinished?.Invoke(currentPlayerTurn);
    }
}