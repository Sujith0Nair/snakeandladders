using Board;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using _Main.ScriptableObjects;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Networking.Server
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private SnakePresetsHolder snakePresetsHolder;
        
        public static GameManager Instance { get; private set; }
        public int LocalId { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsSessionOwner)
            {
                NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
            }

            // Select the snake preset and pass it to all the players
            // snakePresetsHolder.Initialize();
            // var randomPreset = snakePresetsHolder.GetRandomPreset();
            // var index = snakePresetsHolder.GetIndexOfPreset(randomPreset);
            // SpawnSnake_Rpc(index);
            
            // Spawn player
            //SpawnPlayersLocally_Rpc(NetworkManager.Singleton.LocalClientId);
            
            // Hide the UI of the deck of the rest of the people
            
            // Choose the current person to play the game.
        }

        private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
            {
                Debug.Log($"Load Event Completed Scene Name : {sceneEvent.SceneName} ");
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SpawnSnake_Rpc(int index)
        {
            SaLBoard.Instance.SpawnSnakesBasedOnPreset(index);
        }
        
        [Rpc(SendTo.Everyone)]
        private void SpawnPlayersLocally_Rpc(ulong clientId)
        {
            LocalId = (int)clientId;
            Game.GameManager.Instance.SpawnPlayer(LocalId);
        }

        public void OnRoundCompleted()
        {
            if (!IsSessionOwner)
            {
                Debug.Log($"Since not session owner returning!");
                return;
            }
            
            var randomValue = Random.Range(1, 7);
            Debug.LogError($"Random Value : {randomValue}");

            if (randomValue is not 1 or 6)
            {
                Debug.Log($"Safe, no randomisation since the value is: {randomValue}");
                return;
            }

            RandomiseSnakePositions();
            TryMovingTheLocalPlayer_Rpc();
            ToggleLadderStatus_Rpc(true);
        }
        
        private void RandomiseSnakePositions()
        {   
            var playerOccupiedCells = Game.GameManager.Instance.Players.Where(x => x.CurrentCellIndex > 31).Select(x => x.CurrentCellIndex).ToList();
            var preset = snakePresetsHolder.GetPresetWithinInterestOfCells(SaLBoard.Instance.CurrentSnakePreset, playerOccupiedCells);
            var presetIndex = snakePresetsHolder.GetIndexOfPreset(preset);
            ClearAndSpawnBackSnakes_Rpc(presetIndex);
        }

        [Rpc(SendTo.Everyone)]
        private void ClearAndSpawnBackSnakes_Rpc(int presetIndex)
        {
            SaLBoard.Instance.ClearSnakes();
            SaLBoard.Instance.SpawnSnakesBasedOnPreset(presetIndex);
        }

        [Rpc(SendTo.Everyone)]
        private void TryMovingTheLocalPlayer_Rpc()
        {
            Game.GameManager.Instance.TryMovingPlayerToCheckForSnake();
            Game.GameManager.Instance.TurnCompleteCheck();
        }
        
        [Rpc(SendTo.Everyone)]
        private void ToggleLadderStatus_Rpc(bool status)
        {
            if (status)
            {
                SaLBoard.Instance.UnBlockAllLadders();
            }
            else
            {
                SaLBoard.Instance.BlockAllLadders();
            }
        }
    }
}