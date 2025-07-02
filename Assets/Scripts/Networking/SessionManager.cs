using _Main;
using System;
using Common;
using Helpers;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using _Main.Contexts;
using System.Collections;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Random = UnityEngine.Random;

namespace Networking
{
    public class SessionManager : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_InputField joinRoomId;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private TMPro.TextMeshProUGUI messageLabel;
        [SerializeField] private GameObject[] objectsToToggle;
        [SerializeField] private GameObject overlayPanel;
        [SerializeField] private string gameSceneName;
        [SerializeField] private CharacterSelectionContext context;

        private string enteredRoomId;
        private float sceneLoadProgress;
        private bool isSceneLoaded;

        private static ISession ActiveSession
        {
            get => World.Get.ActiveSession;
            set
            {
                World.Get.ActiveSession = value;
                Debug.Log($"Session set to {value}");
            }
        }

        private void Start()
        {
            createRoomButton.AddButtonClickEvent(() =>
            {
                enteredRoomId = Random.Range(1000, 9999).ToString();
                OnRoomJoinClicked();
            });
            joinRoomButton.AddButtonClickEvent(() =>
            {
                enteredRoomId = joinRoomId.text;
                OnRoomJoinClicked();
            });
        }

        private async void OnRoomJoinClicked()
        {
            try
            {
                if (string.IsNullOrEmpty(enteredRoomId) || string.IsNullOrWhiteSpace(enteredRoomId))
                {
                    Debug.LogError("Room Id is empty");
                    return;
                }
                
                ToggleObjects(false);
                
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                var message = $"Signed in anonymously. Player id: {AuthenticationService.Instance.PlayerId}";
                messageLabel.text = message;
                Debug.Log(message);

                StartSession();
            }
            catch (Exception e)
            {
                ToggleObjects(true);
                Debug.LogException(e);
            }
        }

        private async void StartSession()
        {
            try
            {
                var options = new SessionOptions
                {
                    MaxPlayers = World.Get.Board.PlayerCountInMatch,
                    IsPrivate = true
                }
                .WithDistributedAuthorityNetwork();
                ActiveSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(enteredRoomId, options);
                var message = $"Session created or joined. Session id: {ActiveSession.Id}. Join code: {enteredRoomId}";
                messageLabel.text = message;
                Debug.Log(message);

                StartCoroutine(WaitUntilAllRequiredPlayersJoined());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ToggleObjects(true);
            }
        }

        private IEnumerator WaitUntilAllRequiredPlayersJoined()
        {   
            var neededPlayerCount = World.Get.Board.PlayerCountInMatch;

            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
            NetworkManager.Singleton.SceneManager.VerifySceneBeforeUnloading += VerifySceneBeforeUnloading;
            
            Debug.LogError($"Waiting for players to come in: {neededPlayerCount}");

            while (ActiveSession.PlayerCount < neededPlayerCount)
            {
                messageLabel.text = $"Waiting for players. Joined {ActiveSession.PlayerCount} out of {neededPlayerCount}. Join code: {enteredRoomId}";
                yield return null;
            }
            
            messageLabel.text = $"All players joined. Starting the game. Session id: {ActiveSession.Id}. Join code: {enteredRoomId}";
            LoadingScreen.ShowLoadingScreen(() => isSceneLoaded, () => sceneLoadProgress, null);
            if (!ActiveSession.IsHost) yield break;
            
            yield return new WaitForSeconds(5f);
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Additive);
        }

        private static bool VerifySceneBeforeUnloading(Scene scene)
        {
            var result = scene.name != "Bootstrap";
            Debug.LogError($"Verifying scene before unloading: {result} for scene name: {scene.name}");
            return result; 
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            sceneLoadProgress = sceneEvent.AsyncOperation?.progress ?? 50f;
            if (sceneEvent.SceneEventType != SceneEventType.LoadEventCompleted) return;
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            isSceneLoaded = true;
            var sceneCount = SceneManager.sceneCount;
            var targetScene = SceneManager.GetSceneAt(sceneCount - 1);
            SceneManager.SetActiveScene(targetScene);
            context.GoBackToHome();
        }

        private async void KickPlayer(string playerId)
        {
            try
            {
                if (!ActiveSession.IsHost) return;
                await ActiveSession.AsHost().RemovePlayerAsync(playerId);
                Debug.Log($"Player kicked. Player id: {playerId}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private async void LeaveSession()
        {
            try
            {
                await ActiveSession.LeaveAsync();
                Debug.Log($"Session left. Session id: {ActiveSession.Id}. Join code: {ActiveSession.Code}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                ActiveSession = null;
            }
        }
        
        private void ToggleObjects(bool isActive)
        {
            foreach (var obj in objectsToToggle)
            {
                obj.SetActive(isActive);
            }
            overlayPanel.SetActive(!isActive);
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton == null) return;
            if (NetworkManager.Singleton.SceneManager == null) return;
            NetworkManager.Singleton.SceneManager.VerifySceneBeforeUnloading -= VerifySceneBeforeUnloading;
        }
    }
}