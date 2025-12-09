using UnityEngine;

namespace ColorSwitch {
    public class Cheats : MonoBehaviour {
        [SerializeField] private bool _DisableDeath = false;

        public static bool DisableDeath = false;

        private void Update() {
#if UNITY_EDITOR
            DisableDeath = _DisableDeath;

            if (Input.GetKeyUp(KeyCode.A)) {
                Time.timeScale = 0.5f;
            }
            if (Input.GetKeyUp(KeyCode.D)) {
                Time.timeScale = 1f;
            }
#endif
        }
    }
}