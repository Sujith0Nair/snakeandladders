using Player;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Board
{
    public class SaLBoard : MonoBehaviour
    {
        [SerializeField] private Transform boardParent;
        [SerializeField] private Transform laddersParent;
        [SerializeField] private Transform snakesParent;
        [SerializeField] private Transform startPoint;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField, Range(1, 10)] private int snakeCount;
        [SerializeField] private GameObject snakePrefab;
        [SerializeField, Range(1, 10)] private int ladderCount;
        [SerializeField] private GameObject ladderPrefab;
        [SerializeField] private DummyPlayer player;

        private List<BoardCell> cells = new();
        private Dictionary<int, Ladder> ladderMap = new();
        private Dictionary<int, Snake> snakeMap = new();

        private List<int> availableSnakeIndices = new();

        private void Start()
        {
            GenerateBoard();
            AppendLaddersOnBoard_Randomly(out var leftOverIndices);

            availableSnakeIndices = leftOverIndices;

            AppendSnakesOnBoard_Randomly(leftOverIndices);
        }

        private void GenerateBoard()
        {
            var delta = 0;
            for (var row = 0; row < 10; row++)
            {
                for (var column = 0; column < 10; column++)
                {
                    var resultantCol = Mathf.Abs(column - delta);
                    var cell = Instantiate(cellPrefab, startPoint.position + new Vector3(resultantCol * 2, 0, row * 2),
                        Quaternion.identity);
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

        private void AppendLaddersOnBoard_Randomly(out List<int> leftOverIndices)
        {
            var ladderPoints = new List<(int from, int to)>();
            var indicesList = Enumerable.Range(0, 100).ToList();
            leftOverIndices = indicesList;
            for (var i = 0; i < ladderCount; i++)
            {
                var fromIndex = Random.Range(0, indicesList.Count - 20);
                var from = indicesList[fromIndex];
                var closestFactor = from - from % 10;
                var toIndex = Random.Range(closestFactor + 10, indicesList.Count - 2);
                var to = indicesList[toIndex];
                ladderPoints.Add((from, to));
                indicesList.RemoveAt(fromIndex);
                indicesList.RemoveAt(toIndex);
            }

            foreach (var (from, to) in ladderPoints)
            {
                var ladder = Instantiate(ladderPrefab, startPoint.position, Quaternion.identity);
                ladder.transform.SetParent(laddersParent);
                var fromCell = cells[from];
                var toCell = cells[to];
                ladder.name = $"Ladder_{fromCell.CellIndex}_{toCell.CellIndex}";
                var direction = toCell.transform.position - fromCell.transform.position;
                ladder.transform.forward = direction;
                ladder.transform.position = (fromCell.transform.position + toCell.transform.position) / 2;
                ladder.transform.localScale = new Vector3(ladder.transform.localScale.x, ladder.transform.localScale.y,
                    direction.magnitude);
                var ladderComponent = ladder.GetComponent<Ladder>();
                ladderComponent.Init(fromCell.CellIndex, toCell.CellIndex);
                ladderMap.Add(fromCell.CellIndex, ladderComponent);
                Debug.Log($"Ladder from {from} to {to}. Ladder: {ladder}", ladder);
            }
        }

        private void AppendSnakesOnBoard_Randomly(List<int> indicesList)
        {
            var snakePoints = new List<(int from, int to)>();
            for (var i = 0; i < snakeCount; i++)
            {
                var fromIndex = Random.Range(0, indicesList.Count - 20);
                var from = indicesList[fromIndex];
                var closestFactor = from - from % 10;
                var toIndex = Random.Range(closestFactor + 10, indicesList.Count - 2);
                var to = indicesList[toIndex];
                snakePoints.Add((from, to));
                indicesList.RemoveAt(fromIndex);
                indicesList.RemoveAt(toIndex);
            }

            foreach (var (from, to) in snakePoints)
            {
                var snake = Instantiate(snakePrefab, startPoint.position, Quaternion.identity);
                snake.transform.SetParent(snakesParent);
                var fromCell = cells[from];
                var toCell = cells[to];
                snake.name = $"Snake_{fromCell.CellIndex}_{toCell.CellIndex}";
                var direction = toCell.transform.position - fromCell.transform.position;
                snake.transform.forward = direction;
                snake.transform.position = (fromCell.transform.position + toCell.transform.position) / 2;
                snake.transform.localScale = new Vector3(1, snake.transform.localScale.y, direction.magnitude);
                var snakeComponent = snake.GetComponent<Snake>();
                snakeComponent.Init(fromCell.CellIndex, toCell.CellIndex);
                snakeMap.Add(toCell.CellIndex, snakeComponent);
                Debug.Log($"Snake from {from} to {to}. Ladder: {snake}", snake);
            }
        }

        public bool IsOnLadder(int currentIndex, out (int, Vector3) target)
        {
            target = default;
            if (!ladderMap.TryGetValue(currentIndex, out var ladder)) return false;
            var cell = cells[ladder.To - 1];
            target = (cell.CellIndex, cell.transform.position);
            return true;
        }

        public bool IsOnSnake(int currentIndex, out (int, Vector3) snakeRange)
        {
            snakeRange = default;
            if (!snakeMap.TryGetValue(currentIndex, out var snake)) return false;
            var cell = cells[snake.To - 1];
            snakeRange = (cell.CellIndex, cell.transform.position);
            return true;
        }

        public List<BoardCell> GetPathUpto(int from, int count)
        {
            var path = new List<BoardCell>();
            var to = from + count;
            for (var i = from; i < to; i++)
            {
                path.Add(cells[i]);
            }

            Debug.Log($"Path from {from} to {to}. Count: {path.Count}");
            return path;
        }

        public List<BoardCell> GetPathInRange(int from, int to)
        {
            var path = new List<BoardCell>();

            if (from <= to)
            {
                for (var i = from; i < to; i++)
                {
                    path.Add(cells[i]);
                }
            }
            else
            {
                for (var i = from - 1; i >= to - 1; i--)
                {
                    path.Add(cells[i]);
                }
            }

            Debug.Log($"Path from {from} to {to}. Count: {path.Count}");
            return path;
        }

        private void OnDestroy()
        {
            ClearCells();
            ClearLadders();
            ClearSnakes();
        }

        private void ClearCells()
        {
            foreach (var cell in cells)
            {
                Destroy(cell.gameObject);
            }

            cells.Clear();
            cells = null;
        }

        private void ClearLadders()
        {
            foreach (var (_, ladder) in ladderMap)
            {
                if (ladder)
                {
                    Destroy(ladder.gameObject);
                }
            }

            ladderMap.Clear();
            ladderMap = null;
        }

        private void ClearSnakes()
        {
            foreach (var (_, snake) in snakeMap)
            {
                if (snake)
                {
                    Destroy(snake.gameObject);
                }
            }

            snakeMap.Clear();
            snakeMap = null;
        }

        public void RandomizeSnake()
        {
            ClearSnakes();
            snakeMap = new();
            AppendSnakesOnBoard_Randomly(availableSnakeIndices);
        }
    }
}