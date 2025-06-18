using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace _Main.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Snake Presets Holder", menuName = "Scriptable Objects/Snake Presets Holder", order = 0)]
    public class SnakePresetsHolder : ScriptableObject
    {
        [SerializeField] private SnakeCoordPreset[] presets;
        
        private HashSet<CellHash> uniqueOccupiedCells;

        public void Initialize()
        {
            uniqueOccupiedCells = new HashSet<CellHash>();
            for (var index = 0; index < presets.Length; index++)
            {
                var preset = presets[index];
                foreach (var coord in preset.Coords)
                {
                    if (uniqueOccupiedCells.TryGetValue(new CellHash() { CellIndex = coord.x }, out var hash))
                    {
                        hash.AvailablePresets.Add(index);
                    }
                    else
                    {
                        hash = new CellHash
                        {
                            CellIndex = coord.x,
                            AvailablePresets = new HashSet<int>() { index }
                        };
                        uniqueOccupiedCells.Add(hash);
                    }
                    
                }
            }
        }

        public SnakeCoordPreset GetPresetWithinInterestOfCells(SnakeCoordPreset currentPreset, IReadOnlyList<int> cells)
        {
            var currentPresetIndex = Array.IndexOf(presets, currentPreset);
            if (currentPresetIndex == -1)
            {
                return null;
            }
            
            var presetWeights = new int[presets.Length];
            
            foreach (var cell in cells)
            {
                var hash = new CellHash
                {
                    CellIndex = cell,
                    AvailablePresets = new HashSet<int>()
                };
                var isFound = uniqueOccupiedCells.TryGetValue(hash, out var cachedHash);
                if (!isFound) continue;
                foreach (var availablePresetIndex in cachedHash.AvailablePresets.Where(availablePresetIndex => availablePresetIndex != currentPresetIndex))
                {
                    presetWeights[availablePresetIndex]++;
                }
            }

            var bestFitIndex = -1;
            var bestWeight = 0;
            for (var i = 0; i < presetWeights.Length; i++)
            {
                var weight = presetWeights[i];
                if (weight <= bestWeight) continue;
                bestWeight = weight;
                bestFitIndex = i;
            }

            return bestFitIndex == -1 ? GetRandomPreset() : GetPreset(bestFitIndex);
        }
        
        public SnakeCoordPreset GetRandomPreset()
        {
            return GetPreset(Random.Range(0, presets.Length));
        }

        private SnakeCoordPreset GetPreset(int index)
        {
            if (index >= 0 && index < presets.Length)
            {
                return presets[index]; 
            }
            return null;
        }

        private class CellHash : IEquatable<CellHash>
        {
            public int CellIndex;
            
            public HashSet<int> AvailablePresets;

            public bool Equals(CellHash other)
            {
                return other != null && CellIndex == other.CellIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is CellHash other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(CellIndex, AvailablePresets);
            }
        }
    }
}