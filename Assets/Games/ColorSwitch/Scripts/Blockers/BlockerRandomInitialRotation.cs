using System.Collections.Generic;
using UnityEngine;

namespace ColorSwitch {
    public class BlockerRandomInitialRotation : MonoBehaviour, IBlockerResettable {
        [SerializeField] private List<float> AvailableZ;

        public void Reset() {
            Initialize();
        }

        private void Start() {
            Initialize();
        }

        private void Initialize() {
            int index = GameManager.Instance.Rng.Next() % AvailableZ.Count;
            transform.Rotate(transform.rotation.x, transform.rotation.y, AvailableZ[index]);
        }
    }
}