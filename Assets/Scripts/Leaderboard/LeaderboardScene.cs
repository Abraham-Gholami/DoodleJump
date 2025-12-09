using System.Collections.Generic;
using System.Linq;
using TMPro;
using UGS;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class LeaderboardScene : MonoBehaviour
    {
        [SerializeField] private Text yourUsername;
        [SerializeField] private Button editUsernameButton;

        [SerializeField] private Transform content;
        [SerializeField] private GameObject leaderboardItemPrefab;
        
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private EditUsernamePanel editUsernamePanel;

        private readonly List<GameObject> items = new();

        private void Start()
        {
            ClearAndDestroyGameObjects(items);

            yourUsername.text = GamingServices.UsernameManager.GetUsername();
            
            // Set up edit button
            if (editUsernameButton != null)
            {
                editUsernameButton.onClick.AddListener(OnEditUsernameClicked);
            }

            // Subscribe to username change events
            if (editUsernamePanel != null)
            {
                editUsernamePanel.OnUsernameChanged += OnUsernameUpdated;
            }

            PopulateLeaderboardList();
        }

        private void OnEditUsernameClicked()
        {
            if (editUsernamePanel != null)
            {
                editUsernamePanel.OpenPanel();
            }
        }

        private void OnUsernameUpdated(string newUsername)
        {
            // Update the displayed username
            yourUsername.text = newUsername;
            
            // Refresh the leaderboard to show the new username
            RefreshLeaderboard();
        }

        private void RefreshLeaderboard()
        {
            ClearAndDestroyGameObjects(items);
            PopulateLeaderboardList();
        }

        private async void PopulateLeaderboardList()
        {
            DisplayWaitingPanel();

            var list = await GamingServices.LeaderboardManager.GetLeaderboardEntries();

            for (int i = 0; i < list.Count; i++)
            {
                var newItem = Instantiate(leaderboardItemPrefab, content);
                newItem.GetComponent<LeaderboardItem>()
                    .InitializeItem((i + 1).ToString(), list[i].PlayerName, list[i].Score.ToString());

                items.Add(newItem);
            }
            
            HideWaitingPanel();
        }

        private static void ClearAndDestroyGameObjects(List<GameObject> gameObjects)
        {
            foreach (var obj in gameObjects.Where(obj => obj != null))
            {
                Destroy(obj);
            }

            gameObjects.Clear();
        }

        private void DisplayWaitingPanel()
        {
            waitingPanel.SetActive(true);
        }

        private void HideWaitingPanel()
        {
            waitingPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (editUsernameButton != null)
            {
                editUsernameButton.onClick.RemoveListener(OnEditUsernameClicked);
            }

            if (editUsernamePanel != null)
            {
                editUsernamePanel.OnUsernameChanged -= OnUsernameUpdated;
            }
        }
    }
}