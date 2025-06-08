using Player;
using UnityEngine;
using System.Collections.Generic;

namespace Board
{
    public class SaLBoard : MonoBehaviour
    {
        [SerializeField] private Transform boardParent;
        [SerializeField] private Transform startPoint;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private DummyPlayer player;
        
        private List<BoardCell> cells = new();
        
        private void Start()
        {
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            var delta = 0;
            for (var row = 0; row < 10; row++)
            {
                for (var column = 0; column < 10; column++)
                {
                    var resultantCol = Mathf.Abs(column - delta);
                    var cell = Instantiate(cellPrefab, startPoint.position + new Vector3(resultantCol * 2, 0, row * 2), Quaternion.identity);
                    cell.transform.SetParent(boardParent);
                    var cellId = row * 10 + column + 1;
                    cell.name = $"BoardCell_{cellId}";
                    var cellComponent = cell.GetComponent<BoardCell>();
                    cellComponent.Init(cellId);
                    cells.Add(cellComponent);
                }
                delta = (row + 1) % 2 * 9;
            }
        }

        public List<BoardCell> GetPath(int from, int to)
        {
            var path = new List<BoardCell>();
            for (var i = from; i < to; i++)
            {
                path.Add(cells[i]);
            }
            Debug.Log($"Path from {from} to {to}. Count: {path.Count}");
            return path;
        }

        private void OnDestroy()
        {
            foreach (var cell in cells)
            {
                Destroy(cell.gameObject);
            }
            cells.Clear();
            cells = null;
        }
    }
}