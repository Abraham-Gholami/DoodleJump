using System;
using TMPro;
using UGS;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class EditUsernamePanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text errorMessageText;
        [SerializeField] private Text loadingText;

        public event Action<string> OnUsernameChanged;

        private void Start()
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
            cancelButton.onClick.AddListener(ClosePanel);
            
            // Hide error and loading messages initially
            if (errorMessageText != null) errorMessageText.gameObject.SetActive(false);
            if (loadingText != null) loadingText.gameObject.SetActive(false);
        }

        public void OpenPanel()
        {
            panel.SetActive(true);
            
            // Pre-fill with current username
            usernameInputField.text = GamingServices.UsernameManager.GetUsername();
            usernameInputField.Select();
            usernameInputField.ActivateInputField();
            
            // Clear any previous error messages
            if (errorMessageText != null) errorMessageText.gameObject.SetActive(false);
        }

        public void ClosePanel()
        {
            panel.SetActive(false);
            usernameInputField.text = "";
            if (errorMessageText != null) errorMessageText.gameObject.SetActive(false);
        }

        private async void OnSaveButtonClicked()
        {
            string newUsername = usernameInputField.text.Trim();

            // Show loading state
            SetLoadingState(true);

            // Try to save the username (includes validation and cooldown check)
            var result = await GamingServices.UsernameManager.ChangeUsername(newUsername);

            SetLoadingState(false);

            if (result.IsValid)
            {
                // Notify listeners that username changed
                OnUsernameChanged?.Invoke(newUsername);
                ClosePanel();
            }
            else
            {
                ShowError(result.Message);
            }
        }

        private void ShowError(string message)
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = message;
                errorMessageText.gameObject.SetActive(true);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            saveButton.interactable = !isLoading;
            cancelButton.interactable = !isLoading;
            usernameInputField.interactable = !isLoading;

            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(isLoading);
            }
        }

        private void OnDestroy()
        {
            saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            cancelButton.onClick.RemoveListener(ClosePanel);
        }
    }
}