using Unity.Netcode;
using UnityEngine;

namespace SaL.Core
{
    /// <summary>
    /// This is a bootstrap script responsible for spawning the host-only 
    /// managers prefab ([MANAGERS_SERVER]) when the host successfully starts.
    /// It should be placed in the bootstrap scene on the same object as the NetworkManager.
    /// </summary>
    public class HostManagerSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [Tooltip("The prefab containing all host-only manager scripts (LobbyManager, etc.)")]
        [SerializeField] private GameObject _hostManagersPrefab;

        public void SpawnHostManagers()
        {
            Debug.LogWarning("Spawning host-managers");
            // This method only runs on the host because of the OnServerStarted event.
            if (_hostManagersPrefab == null)
            {
                Debug.LogError("HostManagersPrefab is not assigned in the HostManagerSpawner!");
                return;
            }

            // Instantiate the prefab and spawn it on the network.
            var instance = Instantiate(_hostManagersPrefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
}
