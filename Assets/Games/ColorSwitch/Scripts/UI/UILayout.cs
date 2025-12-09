using System.Globalization;
using TMPro;
using UnityEngine;

namespace ColorSwitch {
    public class UILayout : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI ScoreValue;
        [SerializeField] private TextMeshProUGUI MoneyValue;
        [SerializeField] private TextMeshProUGUI TimerValue;

        [Header("Config")]
        [SerializeField] private string scorePrefix = "Score: ";
        [SerializeField] private string moneyPrefix = "";

        private static readonly CultureInfo usCulture = new CultureInfo("en-US");

        public void Clear() {
            if (ScoreValue)
                ScoreValue.text = "";
            if (MoneyValue)
                MoneyValue.text = "";
        }

        public void SetTimer (int arg0) {
            TimerValue.text = $"{arg0 / 60}:{(arg0 % 60).ToString("D2")}";
        }

        public void SetSurge(int moneyCents, int score) {
            SetScore(score);
            SetMoney(moneyCents);
        }

        public void SetScore(int score) {
            if (ScoreValue != null)
                ScoreValue.text = scorePrefix + score.ToString();
        }

        public void SetMoney(int moneyCents) {
            if (MoneyValue != null) {
                float dollars = moneyCents / 100f;
                MoneyValue.text = moneyPrefix + dollars.ToString("C2", usCulture);
            }
        }
    }
}
