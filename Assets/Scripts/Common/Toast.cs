using System;
using UnityEngine;

namespace Common
{
    public class Toast : MonoBehaviour
    {
        [SerializeField] private ToastPanel toastPanel;
        
        public static Toast Instance { get; private set; }

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

        public static void ShowToast(string message, float duration = 2f)
        {
            if (Instance != null && Instance.toastPanel != null)
            {
                Instance.toastPanel.Show(message, duration);
            }
        }
    }
}
