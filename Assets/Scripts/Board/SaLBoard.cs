using Game;
using Player;
using System.Linq;
using UnityEngine;
using _Main.ScriptableObjects;
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
        [SerializeField] private GameObject snakePrefab;
        [SerializeField] private GameObject ladderPrefab;
        [SerializeField] private DummyPlayer player;
        [SerializeField] private LadderCoordPreset ladderPreset;
        [SerializeField] private SnakePresetsHolder snakePresetsHolder;
        [SerializeField] private GameManager gameManager;

        public SnakeCoordPreset CurrentSnakePreset { get; private set; }

        private List<BoardCell> cells = new();
        private Dictionary<int, Ladder> ladderMap = new();
        private Dictionary<int, Snake> snakeMap = new();

        public static SaLBoard Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GenerateBoard();
            AddLaddersOnBoard();
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

        private void AddLaddersOnBoard()
        {
            var coords = ladderPreset.Coords;
            foreach (var coord in coords)
            {
                var from = coord.x;
                var to = coord.y;
                var fromCell = cells[from];
                var toCell = cells[to];
                var ladder = Instantiate(ladderPrefab, startPoint.position, Quaternion.identity);
                ladder.transform.SetParent(laddersParent);
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

        public void SpawnSnakesBasedOnPreset(int presetIndex)
        {
            var preset = snakePresetsHolder.GetPreset(presetIndex);
            SpawnSnakesOnBoard(preset);
        }

        private void SpawnSnakesOnBoard(SnakeCoordPreset preset)
        {
            snakeMap.Clear();
            CurrentSnakePreset = preset;
            
            foreach (var coord in CurrentSnakePreset.Coords)
            {
                var from = coord.x;
                var to = coord.y;
                var fromCell = cells[from];
                var toCell = cells[to];
                var snake = Instantiate(snakePrefab, startPoint.position, Quaternion.identity);
                snake.transform.SetParent(snakesParent);
                snake.name = $"Snake_{fromCell.CellIndex}_{toCell.CellIndex}";
                var direction = toCell.transform.position - fromCell.transform.position;
                snake.transform.forward = direction;
                snake.transform.position = (fromCell.transform.position + toCell.transform.position) / 2;
                snake.transform.localScale = new Vector3(snake.transform.localScale.x, snake.transform.localScale.y,
                    direction.magnitude);
                var snakeComponent = snake.GetComponent<Snake>();
                snakeComponent.Init(fromCell.CellIndex, toCell.CellIndex);
                snakeMap.Add(fromCell.CellIndex, snakeComponent);
                Debug.Log($"Snake from {from} to {to}. Snake: {snake}", snake);
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

        public bool CheckIfLadderIsBlocked(int currentIndex)
        {
            if (!ladderMap.TryGetValue(currentIndex, out var ladder)) return false;
            return ladder.IsBlocked;
        }

        public bool IsOnSnake(int currentIndex, out (int, Vector3) snakeRange)
        {
            snakeRange = default;
            if (!snakeMap.TryGetValue(currentIndex, out var snake)) return false;
            var cell = cells[snake.From - 1];
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
            Instance = null;
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

        public void ClearSnakes()
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

        public void BlockAllLadders()
        {
            foreach (var ladder in ladderMap.Values)
            {
                ladder.BlockLadder();
            }
        }

        public void UnBlockAllLadders()
        {
            foreach (var ladder in ladderMap.Values)
            {
                ladder.UnblockLadder();
            }
        }

        public int GetClosestSnakeIndex(int targetIndex)
        {
            // Find the key in the dictionary that has the minimum absolute difference from the target
            int closestKey = snakeMap.Keys.Aggregate((x, y) =>
                Mathf.Abs(x - targetIndex) < Mathf.Abs(y - targetIndex) ? x : y);

            return closestKey;
        }
    }
}