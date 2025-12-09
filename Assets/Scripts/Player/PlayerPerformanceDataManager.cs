using System.Linq;
using UnityEngine;

namespace Data
{
    public static class PlayerPerformanceDataManager
    {

        public static void AddCoins(int value)
        {
            var coins = GetCoins();
            SetCoins(coins + 1);
        }
        
        public static void SetCoins(int value)
        {
            PlayerPrefs.SetInt("Coins", value);
        }

        public static int GetCoins()
        {
            return PlayerPrefs.GetInt($"Coins", 0);
        }

        public static bool DoesCoinExist()
        {
            return PlayerPrefs.HasKey("Coins");
        }
    }
}
