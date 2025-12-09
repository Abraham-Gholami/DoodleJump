using System;
using System.Threading.Tasks;
using Common;
using Data;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace UGS
{
    public class GamingServices : MonoBehaviour
    {
        public string environment = "production";

        private static LeaderboardManager leaderboardManager;
        private static UsernameManager usernameManager;
        
        public static GamingServices Instance { get; private set; }

        public static LeaderboardManager LeaderboardManager => leaderboardManager;
        public static UsernameManager UsernameManager => usernameManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            try
            {
                WaitPanelManager.DisplayWaitingPanel("Loading User Data.");

                leaderboardManager = new LeaderboardManager();
                usernameManager = new UsernameManager();

                var options = new InitializationOptions()
                    .SetEnvironmentName(environment);
                await UnityServices.InitializeAsync(options);

                Debug.Log("Unity Services initialized successfully.");

                await SignInAnonymouslyAsync();
                await usernameManager.LoadPlayerData();

                if (PlayerPerformanceDataManager.DoesCoinExist())
                {
                    UpdateLeaderboard();
                }
            }
            catch (Exception e)
            {
                Toast.ShowToast("Error: " + e.Message);
                Debug.LogError("Unity Services Initializer Error: " + e.Message);
            }
            finally
            {
                WaitPanelManager.HideWaitingPanel();
            }
        }
        
        private void UpdateLeaderboard()
        {
            LeaderboardManager.UpdateScore(PlayerPerformanceDataManager.GetCoins());
        }

        private static async Task SignInAnonymouslyAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Sign in anonymously succeeded!");

                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"Failed to sign in: {ex.Message}");
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"Request Failed Exception: {ex.Message}");
            }
        }
    }
}
