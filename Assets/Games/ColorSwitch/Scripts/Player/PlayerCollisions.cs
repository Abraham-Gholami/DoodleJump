using UnityEngine;
using System.Collections.Generic;

namespace ColorSwitch {
    public class PlayerCollisions : MonoBehaviour {
        [SerializeField] private PlayerManager manager;
        [SerializeField] private GameObject DeathParticles;
        [SerializeField] private string WallTag = "Wall";
        [SerializeField] private string DeathZone = "Death";

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.gameObject.CompareTag(WallTag)) {
                if (manager.CurrentColor != collision.gameObject.GetComponent<Wall>().CurrentColor) {
                    Die();
                }
            }
            if (collision.gameObject.CompareTag(DeathZone)) {
                Die();
            }
        }

        public void Die() {
#if UNITY_EDITOR
            if (Cheats.DisableDeath) {
                return;
            }
#endif
            SpawnParticle();
        }

        private void SpawnParticle() {
            GameObject particle = Instantiate(DeathParticles, transform.position, Quaternion.identity);
            var mainModule = particle.GetComponent<ParticleSystem>().main;

            Gradient myGradient = new Gradient();
            myGradient.mode = GradientMode.Fixed;
            // Define your gradient's color keys and alpha keys
            GradientColorKey[] colors = new GradientColorKey[Colors.Palette.Count];

            int i = 0;
            foreach (KeyValuePair<ColorVariants, Color> item in Colors.Palette) {
                colors[i] = new GradientColorKey(item.Value, (float)(i + 1) / (Colors.Palette.Count));
                i++;
            }

            myGradient.SetKeys(
                colors,
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );

            ParticleSystem.MinMaxGradient randomColors = new ParticleSystem.MinMaxGradient(myGradient);
            randomColors.mode = ParticleSystemGradientMode.RandomColor;
            mainModule.startColor = randomColors;
        }
    }
}