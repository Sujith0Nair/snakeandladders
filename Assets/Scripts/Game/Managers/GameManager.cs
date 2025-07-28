using System.Collections.Generic;
using System.Linq;
using SaL.Board;
using Deck;
using SaL.Core;
using SaL.Gameplay.Commands;
using Unity.Netcode;
using UnityEngine;

namespace SaL.Gameplay.Managers
{
    /// <summary>
    /// Manages the core gameplay loop, including turns, rounds, and executing card commands.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Assets")]
        [SerializeField] private SnakePresetHolder _snakePresetHolder;
        [SerializeField] private LadderPreset _ladderPreset;

        public NetworkVariable<int> CurrentPlayerTurn = new();
        private int _playedCardId;
        private int _turnsTakenThisRound;
        
        // Card effect states
        private List<ActiveEffect> _activeEffects = new();
        
        // Game State
        private int _playerCountAtStart;
        private List<ulong> _finishedPlayers = new();
        private List<ulong> _disconnectedPlayers = new();
        private Dictionary<ulong, PlayerStats> _playerStats = new();

        private CommandFactory _commandFactory;
        private Dictionary<int, List<int>> _cellToPresetMap;
        private Dictionary<int, int> _ladderMap;
        private int _currentSnakePresetIndex = -1;

        public override void OnNetworkSpawn()
        {
            if (!IsHost) return;
            Instance = this;
            _commandFactory = new CommandFactory();
            LobbyManager.Instance.OnClientDisconnected += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            if (IsHost && LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnClientDisconnected -= OnClientDisconnected;
            }
            base.OnNetworkDespawn();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!_finishedPlayers.Contains(clientId))
            {
                _disconnectedPlayers.Add(clientId);
                _playerStats[clientId].WasDisconnected = true;
                CheckForGameEnd();
            }
        }

        public void StartGame()
        {
            _playerCountAtStart = NetworkManager.Singleton.ConnectedClientsIds.Count;
            _playerStats.Clear();
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                _playerStats[clientId] = new PlayerStats
                {
                    PlayerId = clientId,
                    PlayerName = LobbyManager.Instance.GetPlayerData(clientId).PlayerName.Value.ToString()
                };
            }
            
            PrecomputeBoard();
            
            // Generate the visual board on all clients first
            GenerateBoardClientRpc();
            
            _currentSnakePresetIndex = Random.Range(0, _snakePresetHolder.Presets.Length);
            var initialPreset = _snakePresetHolder.Presets[_currentSnakePresetIndex];
            UpdateSnakesClientRpc(initialPreset.SnakePositions);

            // Set the first player's turn
            CurrentPlayerTurn.Value = 0;
        }

        private void PrecomputeBoard()
        {
            _cellToPresetMap = new Dictionary<int, List<int>>();
            for (var i = 0; i < _snakePresetHolder.Presets.Length; i++)
            {
                foreach (var snake in _snakePresetHolder.Presets[i].SnakePositions)
                {
                    var headCell = snake.x;
                    if (!_cellToPresetMap.ContainsKey(headCell))
                    {
                        _cellToPresetMap[headCell] = new List<int>();
                    }
                    _cellToPresetMap[headCell].Add(i);
                }
            }
            
            _ladderMap = new Dictionary<int, int>();
            foreach (var ladder in _ladderPreset.LadderPositions)
            {
                _ladderMap[ladder.x] = ladder.y;
            }
        }

        public int GetPlayedCardId() => _playedCardId;

        public void AddEffect(CardSO card, ulong targetId, int effectValue = 0)
        {
            _activeEffects.Add(new ActiveEffect
            {
                EffectType = card.ActionType,
                Duration = card.Duration,
                TargetId = targetId,
                EffectValue = effectValue
            });
        }

        public int GetClosestSnakeHead(int fromCell)
        {
            var activeSnakes = _snakePresetHolder.Presets[_currentSnakePresetIndex].SnakePositions;
            if (activeSnakes.Length == 0) return fromCell;

            var closestHead = activeSnakes[0].x;
            var minDistance = Mathf.Abs(closestHead - fromCell);

            for (var i = 1; i < activeSnakes.Length; i++)
            {
                var head = activeSnakes[i].x;
                var distance = Mathf.Abs(head - fromCell);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestHead = head;
                }
            }
            return closestHead;
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayCardServerRpc(int cardId, ulong targetClientId, int selectedLadderId, ServerRpcParams serverRpcParams = default)
        {
            var attackerId = serverRpcParams.Receive.SenderClientId;
            _playedCardId = cardId;
            
            var card = CardRegistry.Instance.GetCardById(cardId);
            if (card == null) return;

            // --- STATS TRACKING ---
            var attackerStats = _playerStats[attackerId];
            switch (card.BaseCardType)
            {
                case CardSO.CardType.Movement:
                    attackerStats.MovementCardsUsed++;
                    break;
                case CardSO.CardType.Action:
                    attackerStats.OffensiveCardsUsed++;
                    break;
                case CardSO.CardType.Defensive:
                    // This case is handled when a counter occurs.
                    break;
            }
            // --- END STATS TRACKING ---

            bool wasCardCountered = false;
            if (card.BaseCardType == CardSO.CardType.Action)
            {
                wasCardCountered = HandleActionCard(card, attackerId, targetClientId);
            }
            else if (card.BaseCardType == CardSO.CardType.Movement)
            {
                var player = LobbyManager.Instance.GetPlayerData(attackerId);
                var targetCell = player.CurrentCellIndex.Value + card.MoveTileCount;
                player.CurrentCellIndex.Value = ResolveFinalCellIndex(targetCell, attackerId);
                
                if (CheckForWin(attackerId)) return;
            }

            if (!wasCardCountered)
            {
                DeckManager.Instance.RemoveCardAndDrawNew(attackerId, cardId);
            }
            
            AdvanceTurn();
        }

        private bool HandleActionCard(CardSO actionCard, ulong attackerId, ulong targetId)
        {
            if (actionCard.ActionType == CardSO.ActionCardType.Retreat)
            {
                targetId = GetNextPlayerId(attackerId);
            }

            if (DeckManager.Instance.PlayerHasCounter(targetId, actionCard))
            {
                // --- STATS TRACKING ---
                _playerStats[targetId].OffensiveCardsCountered++;
                _playerStats[targetId].DefensiveCardsUsed++;
                _playerStats[attackerId].OffensiveCardsReceived++; // It was received, even if countered
                // --- END STATS TRACKING ---

                var attackerName = LobbyManager.Instance.GetPlayerData(attackerId)?.PlayerName.Value.ToString() ?? $"Player {attackerId}";
                var targetName = LobbyManager.Instance.GetPlayerData(targetId)?.PlayerName.Value.ToString() ?? $"Player {targetId}";
                var counterCard = DeckManager.Instance.GetCounterCard(targetId, actionCard);
                var msg = $"{targetName} countered {attackerName}'s '{actionCard.CardName}' with '{counterCard.CardName}'!";
                SendLogToClientsClientRpc(msg);
                
                DeckManager.Instance.RemoveCardAndDrawNew(targetId, counterCard.CardId);
                return true;
            }
            
            // --- STATS TRACKING ---
            if (actionCard.ActionType != CardSO.ActionCardType.Retreat) // Retreat targets an opponent, but doesn't harm them
            {
                _playerStats[targetId].OffensiveCardsReceived++;
            }
            // --- END STATS TRACKING ---

            var command = _commandFactory.GetCommand(actionCard.ActionType);
            if (command != null)
            {
                var context = new CommandContext
                {
                    GameManager = this,
                    LobbyManager = LobbyManager.Instance,
                    DeckManager = DeckManager.Instance,
                    AttackerId = attackerId,
                    TargetId = targetId
                };
                command.Execute(context);

                if (CheckForWin(context.AttackerId)) return false;
                if (CheckForWin(context.TargetId)) return false;

                var attackerName = LobbyManager.Instance.GetPlayerData(attackerId)?.PlayerName.Value.ToString() ?? $"Player {attackerId}";
                SendLogToClientsClientRpc($"{attackerName} used '{actionCard.CardName}'.");
            }

            return false;
        }

        private void AdvanceTurn()
        {
            var connectedPlayers = NetworkManager.Singleton.ConnectedClientsIds.ToList();
            var currentPlayerIndex = connectedPlayers.IndexOf(NetworkManager.Singleton.ConnectedClientsIds[CurrentPlayerTurn.Value]);
            
            // Loop until we find a player who is not halted
            while (true)
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % connectedPlayers.Count;
                var nextPlayerId = connectedPlayers[currentPlayerIndex];

                var haltedEffect = _activeEffects.FirstOrDefault(e => e.EffectType == CardSO.ActionCardType.FreezeOpponent && e.TargetId == nextPlayerId);
                if (haltedEffect == null)
                {
                    CurrentPlayerTurn.Value = currentPlayerIndex;
                    break;
                }
                
                // If a player was halted, remove the effect after skipping their turn
                _activeEffects.Remove(haltedEffect);
                SendLogToClientsClientRpc($"Player {nextPlayerId} was halted and their turn was skipped.");
            }

            _turnsTakenThisRound++;
            if (_turnsTakenThisRound >= connectedPlayers.Count)
            {
                _turnsTakenThisRound = 0;
                OnRoundEnd();
            }
        }

        private void OnRoundEnd()
        {
            // Clear all temporary, round-long effects
            _activeEffects.RemoveAll(effect => effect.Duration == CardSO.EffectDuration.Round);
            
            var dieRoll = Random.Range(1, 7);
            if (dieRoll == 1 || dieRoll == 6)
            {
                RandomizeSnakes();
            }
        }

        private void RandomizeSnakes()
        {
            var presetScores = new int[_snakePresetHolder.Presets.Length];
            
            var connectedClients = NetworkManager.Singleton.ConnectedClientsIds;
            foreach (var clientId in connectedClients)
            {
                var player = LobbyManager.Instance.GetPlayerData(clientId);
                if (player == null) continue;

                var cellIndex = player.CurrentCellIndex.Value;
                if (_cellToPresetMap.ContainsKey(cellIndex))
                {
                    foreach (var presetIndex in _cellToPresetMap[cellIndex])
                    {
                        presetScores[presetIndex]++;
                    }
                }
            }

            var bestPresetIndex = 0;
            var maxScore = 0;
            for (var i = 0; i < presetScores.Length; i++)
            {
                if (presetScores[i] > maxScore)
                {
                    maxScore = presetScores[i];
                    bestPresetIndex = i;
                }
            }
            
            if (maxScore == 0)
            {
                bestPresetIndex = Random.Range(0, _snakePresetHolder.Presets.Length);
            }

            _currentSnakePresetIndex = bestPresetIndex;
            var newPreset = _snakePresetHolder.Presets[_currentSnakePresetIndex];
            
            SendLogToClientsClientRpc("The ground rumbles... Snakes have moved to new locations!");
            UpdateSnakesClientRpc(newPreset.SnakePositions);
        }
        
        public int ResolveFinalCellIndex(int targetCell, ulong playerId)
        {
            var finalCell = targetCell;
            var activeSnakes = _snakePresetHolder.Presets[_currentSnakePresetIndex].SnakePositions;
            var snakeMap = activeSnakes.ToDictionary(s => s.x, s => s.y);

            while (true)
            {
                if (snakeMap.TryGetValue(finalCell, out var snakeTail))
                {
                    // Check for SnakeTamer card
                    if (DeckManager.Instance.TryConsumeDefensiveCard(playerId, CardSO.DefensiveCardType.SnakeTamer))
                    {
                        var playerName = LobbyManager.Instance.GetPlayerData(playerId)?.PlayerName.Value.ToString() ?? $"Player {playerId}";
                        SendLogToClientsClientRpc($"{playerName} used Snake Tamer to avoid the snake!");
                        break; // Avoid the snake
                    }
                    
                    finalCell = snakeTail;
                    continue;
                }

                var isLadderBlocked = _activeEffects.Any(e => e.EffectType == CardSO.ActionCardType.LadderVandalism && e.EffectValue == finalCell);
                var isPlayerLockedOut = _activeEffects.Any(e => e.EffectType == CardSO.ActionCardType.LadderLockout && e.TargetId == playerId);

                if (_ladderMap.TryGetValue(finalCell, out var ladderTop) && !isLadderBlocked && !isPlayerLockedOut)
                {
                    finalCell = ladderTop;
                    continue; // Re-run checks in case a ladder leads to a snake
                }
                
                break;
            }
            return finalCell;
        }

        private ulong GetNextPlayerId(ulong currentPlayerId)
        {
            var clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();
            var currentIndex = clientIds.IndexOf(currentPlayerId);
            if (currentIndex == -1) return 0; // Should not happen
            
            var nextIndex = (currentIndex + 1) % clientIds.Count;
            return clientIds[nextIndex];
        }

        [ClientRpc]
        private void GenerateBoardClientRpc()
        {
            if (ClientController.Instance != null && ClientController.Instance.BoardController != null)
            {
                ClientController.Instance.BoardController.GenerateBoard();
            }
        }

        [ClientRpc]
        private void UpdateSnakesClientRpc(Vector2Int[] newPositions)
        {
            if (ClientController.Instance != null && ClientController.Instance.BoardController != null)
            {
                ClientController.Instance.BoardController.UpdateSnakeVisuals(newPositions);
            }
        }

        [ClientRpc]
        private void SendLogToClientsClientRpc(string message)
        {
            if (UI.GameFeedManager.Instance != null)
            {
                UI.GameFeedManager.Instance.AddLogEntry(message);
            }
        }

        private bool CheckForWin(ulong playerId)
        {
            var player = LobbyManager.Instance.GetPlayerData(playerId);
            if (player == null || player.CurrentCellIndex.Value < 100 || _finishedPlayers.Contains(playerId))
            {
                return false;
            }
            
            // A player has finished!
            _finishedPlayers.Add(playerId);
            _playerStats[playerId].FinishPosition = _finishedPlayers.Count;
            
            var winnerName = player.PlayerName.Value.ToString();
            SendLogToClientsClientRpc($"{winnerName} has finished in position #{_finishedPlayers.Count}!");

            return CheckForGameEnd();
        }

        private bool CheckForGameEnd()
        {
            var totalPlayers = _playerCountAtStart;
            var finishedOrDisconnected = _finishedPlayers.Count + _disconnectedPlayers.Count;

            if (totalPlayers - finishedOrDisconnected <= 1)
            {
                // Game is over!
                EndGameClientRpc("The game has concluded!"); // Generic message, stats will follow
                
                // Find the last player if they exist
                if (_finishedPlayers.Count + _disconnectedPlayers.Count == totalPlayers - 1)
                {
                    var allPlayers = _playerStats.Keys;
                    var lastPlayerId = allPlayers.Except(_finishedPlayers).Except(_disconnectedPlayers).First();
                    _playerStats[lastPlayerId].FinishPosition = totalPlayers;
                }

                // TODO: Send stats to clients
                
                enabled = false;
                return true;
            }

            return false;
        }

        [ClientRpc]
        private void EndGameClientRpc(string message)
        {
            UI.PopupManager.ShowPopup("Game Over", message);
            // Optionally, disable client-side controls here
        }
    }
}
