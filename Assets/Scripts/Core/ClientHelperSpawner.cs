using Unity.Netcode;
using UnityEngine;

namespace SaL.Core
{
    /// <summary>
    /// This is a bootstrap script responsible for spawning the client-only 
    /// helpers prefab ([HELPERS_CLIENT]) when a client successfully starts.
    /// It should be placed in the bootstrap scene on the same object as the NetworkManager.
    /// </summary>
    public class ClientHelperSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [Tooltip("The prefab containing all client-only helper scripts.")]
        [SerializeField] private GameObject _clientHelpersPrefab;

        private void Start()
        {
            // Subscribe to the event that fires when the NetworkManager starts a client.
            NetworkManager.Singleton.OnClientStarted += SpawnClientHelpers;
        }

        private void OnDestroy()
        {
            // Always unsubscribe from events when the object is destroyed.
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStarted -= SpawnClientHelpers;
            }
        }

        private void SpawnClientHelpers()
        {
            // This event fires on the Host as well, so we must ensure we are NOT the host.
            if (NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (_clientHelpersPrefab == null)
            {
                Debug.LogError("ClientHelpersPrefab is not assigned in the ClientHelperSpawner!");
                return;
            }

            // Instantiate the prefab locally. It is NOT a network object.
            Instantiate(_clientHelpersPrefab);
        }
    }
}
