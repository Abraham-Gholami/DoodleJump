using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public class WaitingPanel : MonoBehaviour
    {
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private Text waitingText;
        
        public void Show(string txt, float duration)
        {
            waitingPanel.SetActive(true);
            waitingText.text = txt;
        }

        public void Hide()
        {
            waitingPanel.SetActive(false);
        }
    }
    
    
}