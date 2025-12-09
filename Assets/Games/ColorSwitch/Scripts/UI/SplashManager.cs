using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ColorSwitch {
    public class SplashManager : MonoBehaviour {
        //[SerializeField] VoidEventChannelSO OnPlayerDead;
        [SerializeField] Image render;
        [SerializeField] AnimationCurve curve;
        [SerializeField] float animationDuration;

        private void Start() {
            //OnPlayerDead.OnEventRaised += HandlePlayerDead;
        }
        private void OnDestroy() {
           // OnPlayerDead.OnEventRaised -= HandlePlayerDead;
        }

        private void HandlePlayerDead() {
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut() {
            float time = 0f;
            Color startColor = render.color;

            while (time < animationDuration) {
                time += Time.deltaTime;
                float t = time / animationDuration;

                float curveValue = curve.Evaluate(t);

                // Interpolate alpha from 1 → 0
                Color newColor = startColor;
                newColor.a = Mathf.Lerp(1f, 0f, curveValue);
                render.color = newColor;

                yield return null;
            }

            // Ensure fully transparent at the end
            Color finalColor = startColor;
            finalColor.a = 0f;
            render.color = finalColor;
        }

    }
}