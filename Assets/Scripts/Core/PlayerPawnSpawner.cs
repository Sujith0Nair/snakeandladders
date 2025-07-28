using SaL.Player;
using UnityEngine;
using System.Collections.Generic;

namespace SaL.Core
{
    /// <summary>
    /// A client-side script responsible for spawning and managing the visual player pawns.
    /// It is controlled by the Player object itself, which calls SpawnPawnFor on its own spawn.
    /// </summary>
    public class PlayerPawnSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPawnPrefab;
        
        // A client-side cache mapping a player's ID to their visual pawn GameObject.
        public static readonly Dictionary<ulong, PlayerPawn> PlayerPawns = new();

        /// <summary>
        /// Spawns a visual pawn for the given Player data object.
        /// </summary>
        public void SpawnPawnFor(Player.Player player)
        {
            if (player == null || PlayerPawns.ContainsKey(player.OwnerClientId))
            {
                return;
            }
            
            var pawnInstance = Instantiate(_playerPawnPrefab, player.transform);
            var playerPawn = pawnInstance.GetComponent<PlayerPawn>();
            
            // Link the data object to its visual pawn
            player.SetPawn(playerPawn);
            
            // Cache the pawn for easy lookup by other systems
            PlayerPawns[player.OwnerClientId] = playerPawn;
        }
        
        /// <summary>
        /// Destroys the visual pawn associated with the given Player data object.
        /// </summary>
        public void DespawnPawnFor(Player.Player player)
        {
            if (player == null) return;

            if (!PlayerPawns.TryGetValue(player.OwnerClientId, out var pawn)) return;
            if (pawn != null && pawn.gameObject != null)
            {
                Destroy(pawn.gameObject);
            }
            PlayerPawns.Remove(player.OwnerClientId);
        }
    }
}