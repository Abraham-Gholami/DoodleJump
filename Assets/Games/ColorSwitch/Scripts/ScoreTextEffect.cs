using TMPro;
using UnityEngine;
using System.Collections;

namespace ColorSwitch {

    public class ScoreTextEffect : MonoBehaviour {
        [Header("Animation Settings")]
        [SerializeField] private float AnimationDuration = 1f;

        [Header("Curves (time normalized 0 → 1)")]
        [SerializeField] private AnimationCurve yCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0, 1, 1, 1);
        [SerializeField] private AnimationCurve opacityCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Header("References")]
        [SerializeField] private TextMeshPro t;

        private Vector3 startPos;
        private Vector3 startScale;

        public void Initialize(int val) {
            if (val > 0) {
                t.text = "+" + val.ToString();
            }
            else if (val < 0) {
                t.text = val.ToString(); // already has the minus sign
            }
            else {
                t.text = "0";
            }

            startPos = transform.position;
            startScale = transform.localScale;

            StartCoroutine(Animate());
        }

        private IEnumerator Animate() {
            float elapsed = 0f;
            Color baseColor = t.color;

            while (elapsed < AnimationDuration) {
                elapsed += Time.deltaTime;
                float tVal = Mathf.Clamp01(elapsed / AnimationDuration);

                // Apply Y curve
                float yDelta = yCurve.Evaluate(tVal);
                transform.position = startPos + Vector3.up * yDelta;

                // Apply Scale curve
                float scaleMul = scaleCurve.Evaluate(tVal);
                transform.localScale = startScale * scaleMul;

                // Apply Opacity curve
                float alpha = opacityCurve.Evaluate(tVal);
                t.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
