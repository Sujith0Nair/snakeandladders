using System.Linq;
using Board;
using UnityEngine;
using Unity.Netcode;
using _Main.ScriptableObjects;

namespace Networking.Server
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private SnakePresetsHolder snakePresetsHolder;
        
        public static GameManager Instance { get; private set; }

        
        public override void OnNetworkSpawn()
        {
            // Select the snake preset and pass it to all the players
            snakePresetsHolder.Initialize();
            var randomPreset = snakePresetsHolder.GetRandomPreset();
            var index = snakePresetsHolder.GetIndexOfPreset(randomPreset);
            SpawnSnake_Rpc(index);
            
            // Spawn player
            SpawnPlayersLocally_Rpc(NetworkManager.Singleton.LocalClientId);
            
            // Hide the UI of the deck of the rest of the people
            
            // Choose the current person to play the game.
        }

        [Rpc(SendTo.Everyone)]
        private void SpawnSnake_Rpc(int index)
        {
            SaLBoard.Instance.SpawnSnakesBasedOnPreset(index);
        }
        
        [Rpc(SendTo.Everyone)]
        private void SpawnPlayersLocally_Rpc(ulong clientId)
        {
            Game.GameManager.Instance.SpawnPlayer((int)clientId);
        }
        
        public void RandomiseSnakePositions()
        {
            if (!IsSessionOwner)
            {
                Debug.Log($"Not doing any randomise snake positions since it is not a session owner!");
                return;
            }
            
            var playerOccupiedCells = Game.GameManager.Instance.Players.Where(x => x.CurrentCellIndex > 31).Select(x => x.CurrentCellIndex).ToList();
            var preset = snakePresetsHolder.GetPresetWithinInterestOfCells(SaLBoard.Instance.CurrentSnakePreset, playerOccupiedCells);
            var presetIndex = snakePresetsHolder.GetIndexOfPreset(preset);
            ClearAndSpawnBackSnakes(presetIndex);
        }

        [Rpc(SendTo.Everyone)]
        private void ClearAndSpawnBackSnakes(int presetIndex)
        {
            SaLBoard.Instance.ClearSnakes();
            SaLBoard.Instance.SpawnSnakesBasedOnPreset(presetIndex);
        }
    }
}