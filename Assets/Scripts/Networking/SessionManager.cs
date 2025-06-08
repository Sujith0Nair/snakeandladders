using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Authentication;

namespace Networking
{
    public class SessionManager : MonoBehaviour
    {
        [SerializeField] private string sessionId = Guid.NewGuid().ToString("N");
        
        private ISession session;

        private ISession ActiveSession
        {
            get => session;
            set
            {
                session = value;
                Debug.Log($"Session set to {session}");
            }
        }

        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in anonymously. Player id: {AuthenticationService.Instance.PlayerId}");
                
                StartSession();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void StartSession()
        {
            try
            {
                var options = new SessionOptions
                {
                    MaxPlayers = 4
                }
                .WithDistributedAuthorityNetwork();
                ActiveSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, options);
                Debug.Log($"Session created or joined. Session id: {ActiveSession.Id}. Join code: {ActiveSession.Code}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private async void JoinSessionById()
        {
            try
            {
                ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
                Debug.Log($"Session joined. Session id: {ActiveSession.Id}. Join code: {ActiveSession.Code}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private async void JoinSessionByCode()
        {
            try
            {
                ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionId);
                Debug.Log($"Session joined. Session id: {ActiveSession.Id}. Join code: {ActiveSession.Code}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
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
    }
}