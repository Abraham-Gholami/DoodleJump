using System.Collections.Generic;
using UnityEngine;

namespace ColorSwitch {
    public class BlockerPool : MonoBehaviour {
        [SerializeField] private List<GameObject> AvailableBlockers;

        private List<Queue<GameObject>> _pools;
        private int _lastRandomIndex = -1; // Track last returned index

        private void Awake() {
            // Initialize pool queues
            _pools = new List<Queue<GameObject>>(AvailableBlockers.Count);

            for (int i = 0; i < AvailableBlockers.Count; i++) {
                _pools.Add(new Queue<GameObject>());

                // Spawn one of each at start
                var obj = Instantiate(AvailableBlockers[i], transform);
                obj.SetActive(false);
                _pools[i].Enqueue(obj);
            }
        }

        public GameObject Get(int index) {
            if (index < 0 || index >= AvailableBlockers.Count) {
                return null;
            }

            if (_pools[index].Count == 0) {
                var newObj = Instantiate(AvailableBlockers[index], transform);
                newObj.SetActive(false);
                _pools[index].Enqueue(newObj);
            }

            var obj = _pools[index].Dequeue();
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject obj, int index) {
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            _pools[index].Enqueue(obj);
        }

        public (GameObject, int) GetRandom() {
            if (AvailableBlockers.Count == 0) return (null, -1);

            int index;
            do {
                index = GameManager.Instance.Rng.Next(AvailableBlockers.Count);
            } while (AvailableBlockers.Count > 1 && index == _lastRandomIndex);

            _lastRandomIndex = index;
            return (Get(index), index);
        }
    }
}
