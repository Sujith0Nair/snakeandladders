using UnityEngine;

namespace SaL.Board
{
    /// <summary>
    /// A ScriptableObject that holds the head and tail coordinates for all
    /// ladders on the board. Ladders are static and do not change.
    /// </summary>
    [CreateAssetMenu(fileName = "LadderPreset", menuName = "SaL/Ladder Preset")]
    public class LadderPreset : ScriptableObject
    {
        public Vector2Int[] LadderPositions;
    }
}
