// File: D:\Wagr\WagrGames\WagrGames\Assets\Games\ColorSwitch\Scripts\ColorSwitcher.cs
using System.Collections.Generic;
using UnityEngine;
namespace ColorSwitch {
    [ExecuteAlways]
    public class ColorSwitcher : MonoBehaviour {
        [SerializeField] private List<SpriteRenderer> sections;
        [SerializeField] public bool isRandom = true;
        [SerializeField] public List<ColorVariants> TargetVariants;
        private void Start() {
            if (Application.isPlaying) {
                UpdateColors();
            }
        }
        private void OnValidate() {
            UpdateColors();
        }
        public void UpdateColors() {
            if (sections == null || sections.Count == 0)
                return;
            for (int i = 0; i < sections.Count; i++) {
                if (sections[i] == null)
                    continue;
                if (isRandom) {
                    sections[i].color = Colors.Palette[(ColorVariants)(i % System.Enum.GetValues(typeof(ColorVariants)).Length)];
                }
                else if (TargetVariants != null && TargetVariants.Count > 0) {
                    ColorVariants variant = TargetVariants[i % TargetVariants.Count];
                    sections[i].color = Colors.Palette[variant];
                }
            }
        }
        private void OnTriggerEnter2D(Collider2D collision) {
            if (!Application.isPlaying) return;
            if (collision.gameObject.CompareTag("Player")) {
                var playerManager = collision.gameObject.GetComponent<PlayerManager>();
                if (isRandom) {
                    playerManager.ChangeColor(Colors.GetRandom());
                }
                else if (TargetVariants != null && TargetVariants.Count > 0) {
                    ColorVariants chosenVariant = TargetVariants[Random.Range(0, TargetVariants.Count)];
                    playerManager.ChangeColor(chosenVariant);
                }
                gameObject.SetActive(false);
            }
        }
    }
}