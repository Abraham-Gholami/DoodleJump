using TMPro;
using UnityEngine;
using System.Globalization;

namespace ColorSwitch {
    public class UIManager : MonoBehaviour {
        [SerializeField] UILayout DefaultLayout;
        [SerializeField] UILayout SurgeLayout;

        private void Start() {
            HandleScoreChanged(0); // for initial val

        }

        private void OnDestroy() {
        }

        private void HandleScoreChanged(int score) {
            DefaultLayout.Clear();
            SurgeLayout.Clear();
        }

        private void HandleTimerChanged(int arg0) {
        }
    }
}