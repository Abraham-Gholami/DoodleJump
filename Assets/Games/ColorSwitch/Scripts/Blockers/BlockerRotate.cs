using UnityEngine;

namespace ColorSwitch {
    public class BlockerRotate : MonoBehaviour {
        [SerializeField] private float RotationSpeed = 0f;

        private void Update() {
            // Rotate around Z-axis every frame
            transform.Rotate(0f, 0f, RotationSpeed * Time.deltaTime);
        }
    }
}