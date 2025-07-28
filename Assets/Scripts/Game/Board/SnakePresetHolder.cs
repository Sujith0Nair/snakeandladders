using UnityEngine;

namespace SaL.Board
{
    /// <summary>
    /// A ScriptableObject that holds an array of all available SnakePreset assets.
    /// This provides a single point of reference for the HostController.
    /// </summary>
    [CreateAssetMenu(fileName = "SnakePresetHolder", menuName = "SaL/Snake Preset Holder")]
    public class SnakePresetHolder : ScriptableObject
    {
        public SnakePreset[] Presets;
    }
}
