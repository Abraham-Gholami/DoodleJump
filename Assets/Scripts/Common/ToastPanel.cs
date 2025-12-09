using System.Collections;
using TMPro;
using UnityEngine;

namespace Common
{
    public class ToastPanel : MonoBehaviour
    {
        [SerializeField] private GameObject toastGameObject;
        [SerializeField] private TMP_Text toastText;
        
        private Coroutine toastCoroutine;

        public void Show(string txt, float duration)
        {
            if(toastCoroutine != null) StopCoroutine(toastCoroutine);
            
            toastCoroutine = StartCoroutine(ShowToastCoroutine(txt, duration));
        }

        private IEnumerator ShowToastCoroutine(string txt, float duration)
        {
            toastGameObject.SetActive(true);
            toastText.text = txt;
            yield return new WaitForSecondsRealtime(duration);
            toastGameObject.SetActive(false);
        }
    }
}
