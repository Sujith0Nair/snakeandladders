using System.Collections.Generic;
using UnityEngine;

namespace SaL.Board
{
    /// <summary>
    /// Handles the visual generation and management of the game board, including cells,
    /// snakes, and ladders. This is not a NetworkBehaviour; it will be controlled
    /// by the HostController via ClientRPCs to ensure all clients build the same board.
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _snakePrefab;

        [Header("Board Configuration")]
        [SerializeField] private Transform _boardParent;
        [SerializeField] private Transform _snakesParent;
        [SerializeField] private Transform _startPoint;

        private readonly Dictionary<int, Transform> _cellTransforms = new();
        private readonly List<GameObject> _spawnedSnakes = new();
        private readonly List<GameObject> _spawnedLadders = new(); // To hold ladder references

        // Public getter for the selection manager
        public List<GameObject> GetLadderObjects() => _spawnedLadders;

        /// <summary>
        /// Generates the visual 10x10 grid of cells.
        /// </summary>
        public void GenerateBoard()
        {
            if (_cellPrefab == null || _boardParent == null || _startPoint == null)
            {
                Debug.LogError("BoardController is not fully configured. Assign prefabs and parents.");
                return;
            }

            // Clear any existing cells before generating
            foreach (Transform child in _boardParent)
            {
                Destroy(child.gameObject);
            }
            _cellTransforms.Clear();

            var delta = 0;
            for (var row = 0; row < 10; row++)
            {
                for (var column = 0; column < 10; column++)
                {
                    var resultantCol = Mathf.Abs(column - delta);
                    var cellPos = _startPoint.position + new Vector3(resultantCol * 2, 0, row * 2);
                    var cell = Instantiate(_cellPrefab, cellPos, Quaternion.identity, _boardParent);
                    
                    var cellId = row * 10 + column + 1;
                    cell.name = $"Cell_{cellId}";
                    _cellTransforms[cellId] = cell.transform;
                }
                delta = (row + 1) % 2 * 9;
            }
        }

        /// <summary>
        /// Clears existing snakes and spawns new ones based on the provided positions.
        /// </summary>
        /// <param name="newSnakePositions">An array where each Vector2Int represents a snake (x=head, y=tail).</param>
        public void UpdateSnakeVisuals(Vector2Int[] newSnakePositions)
        {
            CleanupSnakes();

            if (_snakePrefab == null || _snakesParent == null)
            {
                Debug.LogError("Snake prefab or parent is not assigned in BoardController.");
                return;
            }

            foreach (var snakeData in newSnakePositions)
            {
                if (!_cellTransforms.TryGetValue(snakeData.x, out var head) || 
                    !_cellTransforms.TryGetValue(snakeData.y, out var tail))
                {
                    Debug.LogWarning($"Could not find cells for snake from {snakeData.x} to {snakeData.y}. Skipping.");
                    continue;
                }

                var snakeObject = Instantiate(_snakePrefab, _snakesParent);
                snakeObject.name = $"Snake_{snakeData.x}_{snakeData.y}";
                
                var headPosition = head.position;
                var tailPosition = tail.position;

                // Position the snake at the midpoint between head and tail
                snakeObject.transform.position = (headPosition + tailPosition) / 2;

                // Point the snake from head to tail
                var direction = tailPosition - headPosition;
                snakeObject.transform.forward = direction.normalized;

                // Scale the snake to the correct length
                var scale = snakeObject.transform.localScale;
                scale.z = direction.magnitude;
                snakeObject.transform.localScale = scale;
                
                _spawnedSnakes.Add(snakeObject);
            }
        }

        /// <summary>
        /// Destroys all currently spawned snake GameObjects.
        /// </summary>
        public void CleanupSnakes()
        {
            foreach (var snake in _spawnedSnakes)
            {
                if (snake != null)
                {
                    Destroy(snake);
                }
            }
            _spawnedSnakes.Clear();
        }

        /// <summary>
        /// Gets the world-space position of a given cell ID.
        /// </summary>
        public Vector3 GetCellPosition(int cellId)
        {
            if (_cellTransforms.TryGetValue(cellId, out var cellTransform))
            {
                return cellTransform.position;
            }
            return _startPoint.position; // Default to start point if not found
        }
    }
}
