using UnityEngine;

namespace ColorSwitch {

    public class PlayerManager : MonoBehaviour {
        [SerializeField] private SpriteRenderer render;
        public ColorVariants CurrentColor => _currentColor;

        private ColorVariants _currentColor;

        private void Start() {
            ChangeColor(Colors.GetRandom());
        }
        private void OnDestroy() {
        }

        public void ChangeColor(ColorVariants newColor) {
            _currentColor = newColor;
            render.color = Colors.Palette[_currentColor];
        }

        private void HandlePlayerDied() {
            Destroy(gameObject);
        }
    }
}