using Unity.Jobs;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections;
using System.Collections.Generic;

namespace _Main.GameMechanic.Snake
{
    public class DynamicDangerController : MonoBehaviour
    {
        [Header("Game Rules")]
        [SerializeField] private int minEligibleCell = 31;
        [SerializeField] private int boardSize = 100;

        [Header("Job Configuration")]
        [SerializeField] private int minDropRows = 2;
        [SerializeField] private int maxDropRows = 6;
        [SerializeField] private int placementAttempts = 50;

        // --- Job Management Data ---
        private List<JobHandle> jobHandles;
        private List<NativeArray<int2>> jobResults;
        private NativeHashMap<int, bool> forbiddenCellsMap;

        private void Awake()
        {
            jobHandles = new List<JobHandle>();
            jobResults = new List<NativeArray<int2>>();
        }

        public void ExecuteDynamicDanger(List<Player> allPlayers, List<Snake> allSnakes, HashSet<int> ladderPositions)
        {
            Debug.Log("--- DynamicDanger Event Triggered! ---");
            
            // Clear lists from the previous run
            jobHandles.Clear();
            jobResults.Clear();

            var targetablePlayers = allPlayers
                .Where(IsValid)
                .ToList();

            // Prepare shared data. Note: We use Allocator.Persistent because its lifetime is tied to the coroutine.
            forbiddenCellsMap = new NativeHashMap<int, bool>(ladderPositions.Count, Allocator.Persistent);
            foreach (var pos in ladderPositions)
            {
                forbiddenCellsMap.TryAdd(pos, true);
            }

            List<Snake> snakesToUpdate;
            if (targetablePlayers.Count > 0)
            {
                Debug.Log($"{targetablePlayers.Count} player(s) are vulnerable. Scheduling Player Attack jobs.");
                snakesToUpdate = HandlePlayerAttack(targetablePlayers, allSnakes);
            }
            else
            {
                Debug.Log("No players are targetable. Scheduling Board Shuffle jobs.");
                snakesToUpdate = HandleBoardShuffle(allSnakes);
            }

            // Start the coroutine to wait for jobs and process results
            StartCoroutine(ProcessJobResults(snakesToUpdate));
        }

        private bool IsValid(Player player)
        {
            var previousCell = player.currentCell - 1;
            return player.currentCell >= minEligibleCell && previousCell % 10 != 0 && previousCell % 10 != 9;
        }

        private List<Snake> HandlePlayerAttack(List<Player> playersToAttack, List<Snake> allSnakes)
        {
            var snakesNeeded = Mathf.Min(playersToAttack.Count, allSnakes.Count);
            var snakesToUpdate = new List<Snake>();

            // Schedule one job for each player being attacked
            for (var i = 0; i < snakesNeeded; i++)
            {
                SchedulePlacementJob(playersToAttack[i].currentCell, (uint)i);
                snakesToUpdate.Add(allSnakes[i]);
            }
            return snakesToUpdate;
        }

        private List<Snake> HandleBoardShuffle(List<Snake> allSnakes)
        {
            // Schedule one job for each snake to find a new random position
            for (var i = 0; i < allSnakes.Count; i++)
            {
                SchedulePlacementJob(0, (uint)i); // HeadToPlace = 0 for random placement
            }
            return allSnakes; // All snakes will be updated
        }
        
        /// <summary>
        /// Helper method to create, configure, and schedule a single placement job.
        /// </summary>
        private void SchedulePlacementJob(int head, uint seedOffset)
        {
            var resultArray = new NativeArray<int2>(1, Allocator.Persistent);
            var job = new FindSnakePlacementJob
            {
                HeadToPlace = head,
                ForbiddenCells = forbiddenCellsMap,
                BoardSize = boardSize,
                MinDropRows = minDropRows,
                MaxDropRows = maxDropRows,
                MaxPlacementAttempts = placementAttempts,
                Seed = (uint)(UnityEngine.Random.Range(1, 100000)) + seedOffset,
                Result = resultArray
            };
            
            jobHandles.Add(job.Schedule());
            jobResults.Add(resultArray); // Store the result array to read from later
        }

        /// <summary>
        /// Coroutine that waits for all jobs to complete then applies the results.
        /// </summary>
        private IEnumerator ProcessJobResults(List<Snake> snakesToUpdate)
        {
            // Combine all job handles into a single handle
            var combinedHandle = JobHandle.CombineDependencies(new NativeArray<JobHandle>(jobHandles.ToArray(), Allocator.Temp));

            // Yield until the jobs are complete. This pauses the coroutine without freezing the game.
            yield return new WaitUntil(() => combinedHandle.IsCompleted);
            combinedHandle.Complete(); // Finalize completion

            Debug.Log("All jobs complete. Applying results...");

            // --- Apply Results ---
            for (var i = 0; i < jobResults.Count; i++)
            {
                var newPosition = jobResults[i][0];
                if (newPosition.Equals(int2.zero))
                {
                    Debug.LogWarning($"Job for snake {i} failed to find a valid position. Snake will not be moved.");
                }
                else
                {
                    snakesToUpdate[i].headCell = newPosition.x;
                    snakesToUpdate[i].tailCell = newPosition.y;
                    Debug.Log($"Snake {i} has been moved to Head: {newPosition.x}, Tail: {newPosition.y}");
                    // TODO: Trigger visual updates for the snake GameObject here
                }
            }

            // --- Cleanup ---
            // This is a CRITICAL step to prevent memory leaks.
            forbiddenCellsMap.Dispose();
            foreach (var resultArray in jobResults)
            {
                resultArray.Dispose();
            }
            Debug.Log("Job data cleaned up.");
        }
        
        
        [System.Serializable]
        public class Player
        {
            public string playerName;
            public int currentCell;
            // TODO: Add reference to the player's GameObject for visual updates
        }

        [System.Serializable]
        public class Snake
        {
            public int headCell;
            public int tailCell;
            // TODO: Add reference to the snake's GameObject for visual updates
        }
    }
}