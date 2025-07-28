using UnityEngine;

namespace SaL.Board
{
    /// <summary>
    /// A ScriptableObject that holds the head and tail coordinates for one
    /// specific layout of snakes on the board.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSnakePreset", menuName = "SaL/Snake Preset")]
    public class SnakePreset : ScriptableObject
    {
        public Vector2Int[] SnakePositions;
    }
}
