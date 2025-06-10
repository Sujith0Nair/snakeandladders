using System.Collections;
using Board;
using Game;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private MeshRenderer model;
        [SerializeField] private float moveSpeed;

        private int CurrentCellIndex { get; set; }
        private int LastUsedCardIndex { get; set; }

        private SaLBoard board;

        private int playerID;

        private Coroutine playerMoveCoroutine;

        public void Init(SaLBoard board, int playerID, Color playerColor)
        {
            this.board = board;
            this.playerID = playerID;
            model.material.color = playerColor;
        }

        public void MoveToCell(int cellIndex, int lastUsedCardIndex)
        {
            LastUsedCardIndex = lastUsedCardIndex;

            playerMoveCoroutine = StartCoroutine(MoveToCellCoroutine(cellIndex));
        }

        private IEnumerator MoveToCellCoroutine(int cellIndex)
        {
            var allToMoveCells = board.GetPathInRange(CurrentCellIndex, cellIndex);
            while (allToMoveCells.Count > 0)
            {
                var cell = allToMoveCells[0];
                Debug.Log($"Player moving to cell {cell.CellIndex}", cell.transform.gameObject);
                yield return StartCoroutine(MovePlayer(cell.transform));
                allToMoveCells.RemoveAt(0);
                CurrentCellIndex = cellIndex;
            }

            var isOnLadder = board.IsOnLadder(cellIndex, out (int index, Vector3 position) target);
            if (isOnLadder)
            {
                Debug.Log($"Player moving to ladder {CurrentCellIndex}");
                yield return StartCoroutine(MovePlayer(target.position));
                CurrentCellIndex = target.index;
                goto final;
            }

            var isOnSnake = board.IsOnSnake(cellIndex, out target);
            if (isOnSnake)
            {
                Debug.Log($"Player moving to snake {CurrentCellIndex}");
                yield return StartCoroutine(MovePlayer(target.position));
                CurrentCellIndex = target.index;
                goto final;
            }

            if (CurrentCellIndex == 100)
            {
                Debug.Log($"Player won. Cell index: {CurrentCellIndex}");
            }

            final:
            GameManager.Instance.FinishPlayerTurn(playerID, LastUsedCardIndex, CurrentCellIndex);
            playerMoveCoroutine = null;
        }

        private IEnumerator MovePlayer(Transform targetTransform)
        {
            yield return StartCoroutine(MovePlayer(targetTransform.position));
        }

        private IEnumerator MovePlayer(Vector3 targetPosition)
        {
            while ((transform.position - targetPosition).sqrMagnitude > 0.1f * 0.1f)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}