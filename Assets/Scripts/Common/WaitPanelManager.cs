using UnityEngine;

namespace Common
{
    public class WaitPanelManager : MonoBehaviour
    {
        [SerializeField] private WaitingPanel waitingPanel;
        
        public static WaitPanelManager Instance { get; private set; }

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

        public static void DisplayWaitingPanel(string message, float duration = 2f)
        {
            if (Instance != null && Instance.waitingPanel != null)
            {
                Instance.waitingPanel.Show(message, duration);
            }
        }

        public static void HideWaitingPanel()
        {
            if (Instance != null && Instance.waitingPanel != null)
            {
                Instance.waitingPanel.Hide();
            }
        }
    }
}