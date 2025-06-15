using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace _Main.GameMechanic.Snake
{
    [BurstCompile]
    public struct FindSnakePlacementJob : IJob
    {
        // --- INPUT DATA (read-only for the job) ---
        [ReadOnly] public int HeadToPlace; // If > 0, this is the required head position (targeting a player). If 0, find a random head.
        [ReadOnly] public NativeHashMap<int, bool> ForbiddenCells; // Cells occupied by ladders or other snakes being placed.

        // Configuration
        [ReadOnly] public int BoardSize;
        [ReadOnly] public int MinDropRows;
        [ReadOnly] public int MaxDropRows;
        [ReadOnly] public int MaxPlacementAttempts;
        public uint Seed; // A seed for the random number generator

        // --- OUTPUT DATA (the job writes its result here) ---
        public NativeArray<int2> Result; // We use int2 to store the (Head, Tail) pair.

        /// <summary>
        /// This is the method that runs on the worker thread.
        /// </summary>
        public void Execute()
        {
            // Create a random number generator. It MUST be created inside the job.
            var random = new Random(Seed);

            if (HeadToPlace > 0)
            {
                // Player Targeting Logic: Find a tail for the given head.
                var tail = FindValidTailForHead(HeadToPlace, ref random);
                Result[0] = (tail > 0) ? new int2(HeadToPlace, tail) : int2.zero;
            }
            else
            {
                // Random Placement Logic: Find a valid random head and tail pair.
                Result[0] = FindRandomValidSnakePair(ref random);
            }
        }

        /// <summary>
        /// Finds a valid (Head, Tail) pair completely at random.
        /// </summary>
        private int2 FindRandomValidSnakePair(ref Random random)
        {
            for (var i = 0; i < MaxPlacementAttempts; i++)
            {
                var randomHead = random.NextInt(1, BoardSize + 1);

                // Rule Checks for the head
                if (ForbiddenCells.ContainsKey(randomHead)) continue;
                var col = (randomHead - 1) % 10;
                if (col is 0 or 9) continue;

                // If the head is valid, try to find a tail
                var tail = FindValidTailForHead(randomHead, ref random);
                if (tail > 0)
                {
                    return new int2(randomHead, tail); // Success
                }
            }
            return int2.zero; // Failure
        }

        /// <summary>
        /// Given a specific head cell, tries to find a valid tail for it.
        /// </summary>
        private int FindValidTailForHead(int head, ref Random random)
        {
            var headRow = (head - 1) / 10;
            var headCol = (head - 1) % 10;

            for (var i = 0; i < MaxPlacementAttempts; i++)
            {
                // 1. Pick a random vertical drop
                var rowDrop = random.NextInt(MinDropRows, MaxDropRows + 1);
                var targetRow = headRow - rowDrop;
                if (targetRow < 0) continue;

                // 2. Pick a random horizontal shift, respecting the verticality rule
                var colShift = random.NextInt(-rowDrop, rowDrop + 1);
                var targetCol = headCol + colShift;
                if (targetCol is < 0 or >= 10) continue;

                // 3. Convert back to a cell number and validate it
                var tailCell = (targetRow * 10) + targetCol + 1;
                if (tailCell >= head || ForbiddenCells.ContainsKey(tailCell)) continue;

                return tailCell; // Success
            }
            return 0; // Failure
        }
    }
}