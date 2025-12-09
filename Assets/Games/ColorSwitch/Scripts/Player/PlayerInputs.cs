
using UnityEngine;
namespace ColorSwitch {
    public class PlayerInputs : MonoBehaviour {
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
            }
            if (Application.isMobilePlatform) {
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
                }
            }
        }
    }
}