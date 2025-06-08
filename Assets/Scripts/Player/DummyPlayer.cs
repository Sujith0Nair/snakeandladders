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
            var allToMoveCells = board.GetPath(CurrentCellIndex, cellIndex);
            while (allToMoveCells.Count > 0)
            {
                var cell = allToMoveCells[0];
                yield return StartCoroutine(MovePlayer(cell.transform));
                Debug.Log($"Player moving to cell {cell.CellIndex}", cell.transform.gameObject);
                allToMoveCells.RemoveAt(0);
                CurrentCellIndex = cellIndex;
            }
            playerMoveCoroutine = null;
        }
        
        private IEnumerator MovePlayer(Transform targetTransform)
        {
            while (Vector3.Distance(transform.position, targetTransform.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, moveSpeed * Time.deltaTime);
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