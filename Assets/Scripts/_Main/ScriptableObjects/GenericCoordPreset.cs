using UnityEngine;
using System.Collections.Generic;

namespace _Main.ScriptableObjects
{
    public class GenericCoordPreset : ScriptableObject
    {
        [SerializeField] private Vector2Int[] coords;
        
        public IReadOnlyList<Vector2Int> Coords => coords;
    }
}