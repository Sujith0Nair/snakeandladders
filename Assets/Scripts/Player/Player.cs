using SaL.Core;
using SaL.Gameplay.Managers;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SaL.Player
{
    /// <summary>
    /// Represents a single player in the game. This NetworkBehaviour holds all the
    /// synchronized data for a player, such as their ID, color, and position on the board.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        public NetworkVariable<FixedString32Bytes> PlayerName = new();
        public NetworkVariable<Color> PlayerColor = new();
        public NetworkVariable<int> CurrentCellIndex = new();

        // This will be assigned on the client after the pawn is spawned
        private PlayerPawn _pawn;
        private Board.BoardController _board;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient)
            {
                CurrentCellIndex.OnValueChanged += OnCellIndexChanged;
                // We cache the board controller for efficiency
                _board = FindFirstObjectByType<Board.BoardController>();
                
                // Request the spawner to create a visual pawn for this player data
                FindFirstObjectByType<PlayerPawnSpawner>()?.SpawnPawnFor(this);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                CurrentCellIndex.OnValueChanged -= OnCellIndexChanged;
                
                // Request the spawner to destroy the visual pawn
                FindFirstObjectByType<PlayerPawnSpawner>()?.DespawnPawnFor(this);
            }
            base.OnNetworkDespawn();
        }

        public void SetPawn(PlayerPawn pawn)
        {
            _pawn = pawn;
        }

        private void OnCellIndexChanged(int previousValue, int newValue)
        {
            if (_pawn != null && _board != null)
            {
                var targetPosition = _board.GetCellPosition(newValue);
                _pawn.MoveToCell(targetPosition);
            }
        }

        [ServerRpc]
        public void RequestColorServerRpc(Color color)
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayerRequestColor(OwnerClientId, color);
            }
        }
    }
}
