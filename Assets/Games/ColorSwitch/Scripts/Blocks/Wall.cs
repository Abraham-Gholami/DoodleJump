using UnityEngine;

namespace ColorSwitch {
    [ExecuteAlways] // Ensures it runs in Edit mode too
    public class Wall : MonoBehaviour {
        [SerializeField] private ColorVariants _currentColor;
        public ColorVariants CurrentColor => _currentColor;

        private SpriteRenderer _renderer;

        private void Awake() {
            _renderer = GetComponent<SpriteRenderer>();
            gameObject.tag = "Wall";
        }

        private void Start() {
            Initialize(_currentColor);
        }

        public void Initialize(ColorVariants newColor) {
            _currentColor = newColor;
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();

            if (_renderer != null)
                _renderer.color = Colors.Palette[_currentColor];
        }

        // Called when a value is changed in the Inspector
        private void OnValidate() {
            Initialize(_currentColor);
        }
    }
}
