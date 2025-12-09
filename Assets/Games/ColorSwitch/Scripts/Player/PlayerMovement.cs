
using UnityEngine;
namespace ColorSwitch {
    public class PlayerMovement : MonoBehaviour {
        [Header("Physics")]
        [SerializeField] private float forceY;
        [SerializeField] private float maxVelocityY;
        [SerializeField] private Rigidbody2D rb;
        private void Start() {
        }
        private void OnDestroy() {
        }
        private void FixedUpdate() {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, maxVelocityY));
        }
        private void HandlePlayerClick() {
            rb.linearVelocityY = 0;
            rb.AddForce(new Vector2(
                0,
                forceY
            ), ForceMode2D.Impulse);
        }
    }
}