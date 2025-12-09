using TMPro;
using UGS;
using UnityEngine;

namespace MainMenu
{
    public class ProfilePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text usernameText;
        [SerializeField] private TMP_Text coinText;

        private void OnEnable()
        {
            usernameText.text = GamingServices.UsernameManager.GetUsername();
        }
    }
}