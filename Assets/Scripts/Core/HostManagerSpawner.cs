using _Main;
using UnityEngine;
using Unity.Netcode;

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
        [SerializeField] private GameObject hostManagersPrefab;

        public void SpawnHostManagers()
        {
            Debug.Log("Spawning host-managers");
            if (!World.Get.ActiveSession.IsHost)
            {
                return;
            }
            
            if (hostManagersPrefab == null)
            {
                Debug.LogError("HostManagersPrefab is not assigned in the HostManagerSpawner!");
                return;
            }

            var instance = Instantiate(hostManagersPrefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
}
