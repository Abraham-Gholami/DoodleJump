using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Unity.Services.Authentication;
using UnityEngine;

namespace UGS
{
    public class UsernameManager
    {
        private Player player;

        public async Task LoadPlayerData()
        {
            string savedUsername = await AuthenticationService.Instance.GetPlayerNameAsync();
            long temp = Convert.ToInt64(PlayerPrefs.GetString("lastUsernameChangeTime", DateTime.MinValue.Ticks.ToString()));
            DateTime savedChangeTime = new DateTime(temp);
        
            player = new Player(savedUsername)
            {
                LastUsernameChangeTime = savedChangeTime
            };
        }

        public bool CanChangeUsername()
        {
            return DateTime.Now.Subtract(player.LastUsernameChangeTime).TotalHours >= 24;
        }

        public TimeSpan GetTimeUntilNextChange()
        {
            var timeSinceLastChange = DateTime.Now.Subtract(player.LastUsernameChangeTime);
            var timeRemaining = TimeSpan.FromHours(24) - timeSinceLastChange;
            return timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero;
        }

        public async Task<ValidationResult> ChangeUsername(string newUsername)
        {
            // First validate the username
            var validation = UsernameValidator.ValidateUsername(newUsername);
            if (!validation.IsValid)
            {
                return validation;
            }

            // Check cooldown
            if (!CanChangeUsername())
            {
                var timeRemaining = GetTimeUntilNextChange();
                return new ValidationResult(false, 
                    $"You can only change your username once every 24 hours. Time remaining: {timeRemaining.Hours}h {timeRemaining.Minutes}m");
            }

            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(newUsername);
                player.Username = newUsername;
                player.LastUsernameChangeTime = DateTime.Now;
            
                SavePlayerData();
                Debug.Log("Username changed successfully!");
                return new ValidationResult(true, "Username changed successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError("Error changing username: " + e.Message);
                Toast.ShowToast("Error: " + e.Message);
                return new ValidationResult(false, "Failed to update username. Please try again.");
            }
        }

        public string GetUsername()
        {
            return RemoveUsernameNumber(player.Username);
        }

        public string GetFullUsername()
        {
            return player.Username;
        }
        
        private void SavePlayerData()
        {
            PlayerPrefs.SetString("Username", player.Username);
            PlayerPrefs.SetString("lastUsernameChangeTime", player.LastUsernameChangeTime.Ticks.ToString());
            PlayerPrefs.Save();
        }
        
        public static string RemoveUsernameNumber(string input)
        {
            string pattern = @"#\d+$";
            return Regex.Replace(input, pattern, "");
        }
    }
}