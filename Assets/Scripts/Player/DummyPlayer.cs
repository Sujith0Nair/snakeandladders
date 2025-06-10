using Board;
using UnityEngine;
using System.Collections;

namespace Player
{
    public class DummyPlayer : MonoBehaviour
    {
        private int CurrentCellIndex { get; set; }
        
        [SerializeField] private SaLBoard board;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Camera mainCamera;
        
        private bool IsPlayerMoving => playerMoveCoroutine != null;
        private Coroutine playerMoveCoroutine;

        private void MoveToCell(int cellIndex)
        {
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
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private void Update()
        {
            if (IsPlayerMoving) return;
            if (!Input.GetMouseButtonDown(0)) return;
            if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit)) return;
            if (!hit.transform.TryGetComponent(out BoardCell cell)) return;

            var cellIndex = cell.CellIndex;
            if (cellIndex <= CurrentCellIndex) return;
            Debug.Log($"Selected cell {cellIndex}");
            MoveToCell(cellIndex);
        }
    }
}