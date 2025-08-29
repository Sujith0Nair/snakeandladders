using Data;
using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Authentication;

namespace _Main
{
    public class World : MonoBehaviour
    {
        public static World Get;
        public bool IsSignedIn { get; private set; }
        public DataBoard Board { get; private set; }
        public ISession ActiveSession { get; set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            var _ = new GameObject("[WORLD]", typeof(World));
        }

        private void Awake()
        {
            Get = this;
            Board = new DataBoard();
            DontDestroyOnLoad(this);
        }

        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                var message = $"Signed in anonymously. Player id: {AuthenticationService.Instance.PlayerId}";
                IsSignedIn = true;
                Debug.Log(message);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private void OnDestroy()
        {
            DataBoard.ClearData();
            Get = null;
        }
    }
}