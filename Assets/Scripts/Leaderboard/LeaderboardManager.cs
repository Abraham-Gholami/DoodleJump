using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace UGS
{
    public class LeaderboardManager
    {
        private const string LeaderboardId = "MaxHeight";

        private static DateTime savedTime;
        private static List<LeaderboardEntry> result;
        
        public static async void UpdateScore(int score)
        {
            try
            {
                var playerEntry = await LeaderboardsService.Instance
                    .AddPlayerScoreAsync(LeaderboardId, score);
                Debug.Log(JsonConvert.SerializeObject(playerEntry));
            }
            catch (Exception e)
            {
                Debug.LogError("Could not save the score. Error: " + e.Message);
            }
        }

        public async Task<List<LeaderboardEntry>> GetLeaderboardEntries()
        {
            if (IsTenMinutesPast() || result == null)
            {
                result = await GetPaginatedScores();
                savedTime = DateTime.Now;
            }
            return result;
        }

        private static bool IsTenMinutesPast()
        {
            var currentTime = DateTime.Now;
            var timeDifference = currentTime - savedTime;
            return timeDifference.TotalMinutes >= 1;
        }

        private static async Task<List<LeaderboardEntry>> GetPaginatedScores()
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                LeaderboardId,
                new GetScoresOptions{ Offset = 0, Limit = 50 }
            );

            return scoresResponse.Results;
        }
    }
}