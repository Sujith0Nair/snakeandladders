using UnityEngine;
using Unity.Netcode;

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
        [SerializeField] private GameObject clientHelpersPrefab;

        private void Start()
        {
            NetworkManager.Singleton.OnClientStarted += SpawnClientHelpers;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStarted -= SpawnClientHelpers;
            }
        }

        private void SpawnClientHelpers()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (clientHelpersPrefab == null)
            {
                Debug.LogError("ClientHelpersPrefab is not assigned in the ClientHelperSpawner!");
                return;
            }

            Instantiate(clientHelpersPrefab);
        }
    }
}
