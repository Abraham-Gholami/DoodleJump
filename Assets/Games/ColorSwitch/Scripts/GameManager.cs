

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorSwitch {
    public enum ColorVariants {
        Violet,
        Cyan,
        Pink,
        Yellow,
    }
    public class Colors {
        public static Dictionary<ColorVariants, Color> Palette = new Dictionary<ColorVariants, Color> {
            {ColorVariants.Violet, new Color(0.698f, 0, 1) },
            {ColorVariants.Cyan, new Color(0, 0.98f, 0.98f) },
            {ColorVariants.Pink, new Color(1, 0.152f, 0.545f) },
            {ColorVariants.Yellow, new Color(1, 0.83f, 0.21f) },
        };

        public static ColorVariants GetRandom() {
            int index = GameManager.Instance.Rng.Next() % 4;
            return (ColorVariants)(index);
        }
    }

    public class GameManager : MonoBehaviour {

        public static GameManager Instance;
        public System.Random Rng => _rng;
        private System.Random _rng;

        private bool isGameFinished = false;


        private void Awake() {
            int seed = 986524;
#if !UNITY_EDITOR
            seed = MessageReceiver.UserData.Seed;
#endif
            _rng = new System.Random(seed);
            if (Instance != null) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start() {
        }

        public void OnTimerComplete() {
            PlayerCollisions player = FindFirstObjectByType<PlayerCollisions>();
            player?.Die();
        }

        private void HandlePlayerDead() {
            FinishGame();
        }

        public void FinishGame() {
            if (isGameFinished)
                return;
            StartCoroutine(FinishGameCor());
        }

        private IEnumerator FinishGameCor() {
            if (isGameFinished)
                yield break;
            isGameFinished = true;
            yield return new WaitForSeconds(3f);
        }
    }
}