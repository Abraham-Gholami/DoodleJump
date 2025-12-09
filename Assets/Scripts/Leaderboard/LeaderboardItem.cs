using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class LeaderboardItem : MonoBehaviour
    {
        [SerializeField] private Text index;
        [SerializeField] private Text username;
        [SerializeField] private Text score;

        public void InitializeItem(string idx, string uName, string points)
        {
            index.text = idx;
            username.text = uName;
            score.text = points;
        }
    }
}

namespace UGS
{
}