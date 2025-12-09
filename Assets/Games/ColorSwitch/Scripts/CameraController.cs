using UnityEngine;

namespace ColorSwitch {
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Rigidbody2D playerRb;
        [SerializeField] private Scroller scroller; // Used to get all content for the world reset
        [SerializeField] private BoxCollider2D leftBorder;
        [SerializeField] private BoxCollider2D rightBorder;
        [SerializeField] private BoxCollider2D deathZone; // 👈 New death collider

        [Header("Camera Sizing")]
        [SerializeField] private float MinCameraWidth = 5f;
        [SerializeField] private float BottomY = -5f;

        [Header("Scrolling")]
        [Tooltip("The constant speed at which the camera scrolls up to catch up with the player.")]
        [SerializeField] private float scrollSpeed = 5f;
        [Tooltip("The Y position at which the world will reset to avoid floating point issues.")]
        [SerializeField] private float worldResetThresholdY = 100f;

        private Camera cam;
        private float distanceToMove = 0f;

        private void Start() {
            cam = GetComponent<Camera>();
            if (!cam.orthographic) {
                return;
            }

            // Set camera size based on aspect ratio to ensure minimum width is visible
            float minOrthoSize = MinCameraWidth / (2f * cam.aspect);
            if (cam.orthographicSize < minOrthoSize) {
                cam.orthographicSize = minOrthoSize;
            }

            // Set initial camera position
            Vector3 pos = transform.position;
            pos.y = BottomY + cam.orthographicSize;
            transform.position = pos;

            // Setup death zone collider
            if (deathZone != null) {
                deathZone.isTrigger = true;
                deathZone.tag = "Death";
            }

            UpdateBorderColliders();
            UpdateDeathCollider();
        }

        private void LateUpdate() {
            if (playerTransform == null) return;

            HandleCameraScroll();
            HandleWorldReset();
            UpdateBorderColliders();
            UpdateDeathCollider();
        }

        private void HandleCameraScroll() {
            float camMidY = transform.position.y;
            float playerY = playerTransform.position.y;

            // Only move camera if player is above its center
            if (playerY > camMidY) {
                Vector3 targetPos = new Vector3(transform.position.x, playerY, transform.position.z);

                // Smoothly move camera toward player using Lerp
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    scrollSpeed * Time.deltaTime
                );
            }
        }

        private void HandleWorldReset() {
            if (transform.position.y >= worldResetThresholdY) {

                // Shift Camera
                transform.position -= new Vector3(0, worldResetThresholdY, 0);

                // Shift Player (both transform and rigidbody)
                playerTransform.position -= new Vector3(0, worldResetThresholdY, 0);
                playerRb.position -= new Vector2(0, worldResetThresholdY);

                // Shift all scrollable content
                if (scroller != null && scroller.scrollContent != null) {
                    foreach (var item in scroller.scrollContent) {
                        if (item.transform != null) {
                            item.transform.position -= new Vector3(0, worldResetThresholdY, 0);
                        }
                    }
                }
            }
        }

        private void UpdateBorderColliders() {
            if (leftBorder == null || rightBorder == null) return;

            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;

            // Stretch vertically to match camera height
            Vector2 colliderSize = new Vector2(1f, camHeight); // thickness = 1, can adjust
            leftBorder.size = colliderSize;
            rightBorder.size = colliderSize;

            // Position colliders just outside screen
            float offsetX = camWidth / 2f + leftBorder.size.x / 2f;

            Vector3 camPos = transform.position;
            leftBorder.transform.position = new Vector3(camPos.x - offsetX, camPos.y, 0f);
            rightBorder.transform.position = new Vector3(camPos.x + offsetX, camPos.y, 0f);
        }

        private void UpdateDeathCollider() {
            if (deathZone == null) return;

            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;

            // Stretch horizontally to cover entire screen
            deathZone.size = new Vector2(camWidth, 1f); // height = 1, can adjust

            // Position just below bottom of camera
            Vector3 camPos = transform.position;
            float offsetY = -(camHeight / 2f + deathZone.size.y / 2f);
            deathZone.transform.position = new Vector3(camPos.x, camPos.y + offsetY, 0f);
        }
    }
}
