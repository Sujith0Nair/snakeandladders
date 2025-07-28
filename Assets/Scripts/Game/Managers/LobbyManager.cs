using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SaL.Gameplay.Managers
{
    /// <summary>
    /// Manages the pre-game lobby state, including player connections,
    /// data, and color selection.
    /// </summary>
    public class LobbyManager : NetworkBehaviour
    {
        public event Action<ulong> OnClientDisconnected;
        
        public static LobbyManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;

        public NetworkList<Color> TakenColors { get; } = new();
        
        private Dictionary<ulong, Color> _playerColors = new();
        private Dictionary<ulong, Player.Player> _playerData = new();

        public override void OnNetworkSpawn()
        {
            if (!IsHost) return;
            Instance = this;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsHost) return;
            Instance = null;
            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        private void OnClientConnect(ulong clientId)
        {
            var playerInstance = Instantiate(_playerPrefab);
            var networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);
            _playerData[clientId] = playerInstance.GetComponent<Player.Player>();
            
            // Notify other managers about the new player
            DeckManager.Instance.InitializePlayerData(clientId);
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_playerColors.ContainsKey(clientId))
            {
                var color = _playerColors[clientId];
                for (var i = 0; i < TakenColors.Count; i++)
                {
                    if (TakenColors[i] == color)
                    {
                        TakenColors.RemoveAt(i);
                        break;
                    }
                }
                _playerColors.Remove(clientId);
            }
            _playerData.Remove(clientId);
            OnClientDisconnected?.Invoke(clientId);
        }

        public void OnPlayerRequestColor(ulong clientId, Color color)
        {
            for (var i = 0; i < TakenColors.Count; i++)
            {
                if (TakenColors[i] == color) return;
            }

            if (_playerColors.ContainsKey(clientId))
            {
                var oldColor = _playerColors[clientId];
                for (var i = 0; i < TakenColors.Count; i++)
                {
                    if (TakenColors[i] == oldColor)
                    {
                        TakenColors.RemoveAt(i);
                        break;
                    }
                }
            }
            _playerColors[clientId] = color;
            TakenColors.Add(color);
            
            // Update the player's synchronized data
            if (_playerData.TryGetValue(clientId, out var player))
            {
                player.PlayerColor.Value = color;
            }
        }

        public Player.Player GetPlayerData(ulong clientId) => _playerData.ContainsKey(clientId) ? _playerData[clientId] : null;

        public void StartGame()
        {
            if (!IsHost) return;

            // Tell the DeckManager to set up the cards
            DeckManager.Instance.BuildDeck();
            DeckManager.Instance.DealInitialHands();

            // Tell the GameManager to set up the board and start the first turn
            GameManager.Instance.StartGame();
        }
    }
}
