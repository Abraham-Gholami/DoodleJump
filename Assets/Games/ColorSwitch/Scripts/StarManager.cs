using UnityEngine;

namespace ColorSwitch {
    public class StarManager : MonoBehaviour {
        [SerializeField] private GameObject effect;
        [SerializeField] private GameObject TextEffect;
        [SerializeField] private byte Reward = 2;

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Player")) {
                Instantiate(effect, transform.position, Quaternion.identity);
                Instantiate(TextEffect, transform.position, Quaternion.identity).GetComponent<ScoreTextEffect>().Initialize(Reward);
                gameObject.SetActive(false);
            }
        }
    }
}