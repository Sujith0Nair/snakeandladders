using _Main;
using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Services.Multiplayer;

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

        private uint joinedPlayers = 1;
        private Dictionary<ulong, Color> _playerColors = new();
        private Dictionary<ulong, Player.Player> _playerData = new();
        private static ISession CurrentSession => World.Get.ActiveSession;

        public override void OnNetworkSpawn()
        {
            if (!CurrentSession.IsHost) return;
            Instance = this;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            
            var currentConnectedClients = NetworkManager.Singleton.ConnectedClients;
            foreach (var (id, _) in currentConnectedClients)
            {
                OnClientConnect(id);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!CurrentSession.IsHost) return;
            Instance = null;
            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        private void OnClientConnect(ulong clientId)
        {
            Debug.Log($"[LobbyManager] Client connected. Client ID:  {clientId}");
            var playerInstance = Instantiate(_playerPrefab);
            var networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId, true);
            _playerData[clientId] = playerInstance.GetComponent<Player.Player>();
            
            // Notify other managers about the new player
            DeckManager.Instance.InitializePlayerData(clientId);
        }

        private ulong? GetSessionOwnerId()
        {
            var connectedClients = NetworkManager.Singleton.ConnectedClients;
            foreach (var (id, _) in connectedClients)
            {
                if (_playerData.ContainsKey(id)) continue;
                return id;
            }
            return null;
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

        [ContextMenu("Start Game")]
        public void StartGame()
        {
            if (!CurrentSession.IsHost)
            {
                Debug.LogWarning("[LobbyManager] You are not the host. Please ask host to start the game.");
                return;
            }
            
            Debug.Log("Building the deck");

            // Tell the DeckManager to set up the cards
            DeckManager.Instance.BuildDeck();
            
            Debug.Log("Dealing initial hand");
            
            DeckManager.Instance.DealInitialHands();

            // Tell the GameManager to set up the board and start the first turn
            Debug.Log("Asking game manager to start the game");
            GameManager.Instance.StartGame();
        }
    }
}
