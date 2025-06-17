using UnityEngine;

namespace _Main.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Snake Presets Holder", menuName = "Scriptable Objects/Snake Presets Holder", order = 0)]
    public class SnakePresetsHolder : ScriptableObject
    {
        [SerializeField] private SnakeCoordPreset[] presets;
        
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
    }
}