// File: D:\Wagr\WagrGames\WagrGames\Assets\Games\ColorSwitch\Scripts\Blockers\BaseBlocker.cs
using System.Collections.Generic;
using UnityEngine;
namespace ColorSwitch {
    public interface IBlockerResettable {
        public void Reset();
    }

    public interface IBlocker {
        public float Height { get; }
        public void Activate();
        public void Deactivate();
        public void Reset();
    }
    public class BaseBlocker : MonoBehaviour, IBlocker {
        [SerializeField] private float _height;
        [SerializeField] private List<GameObject> stars;
        [SerializeField, Tooltip("Must have IBlockerResettable")] private List<GameObject> resettable;
        [SerializeField] private List<GameObject> colorSwitches;
        [Tooltip("A list of colors that cannot pass through this blocker.")]
        public List<ColorVariants> BlockedColors;
        public float Height => _height;
        public void Activate() {
            foreach (var item in stars) {
                item.SetActive(true);
            }
            foreach (var item in colorSwitches) {
                item.SetActive(true);
            }

            foreach (var item in resettable) {
                item.GetComponent<IBlockerResettable>()?.Reset();
            }
        }

        private void Update() {
#if UNITY_EDITOR
            // Center position of the blocker
            Vector3 pos = transform.position;

            float halfWidth = 2.5f;
            float height = _height;

            // Define rectangle corners
            Vector3 bottomLeft = new Vector3(pos.x - halfWidth, pos.y, pos.z);
            Vector3 bottomRight = new Vector3(pos.x + halfWidth, pos.y, pos.z);
            Vector3 topLeft = new Vector3(pos.x - halfWidth, pos.y + height, pos.z);
            Vector3 topRight = new Vector3(pos.x + halfWidth, pos.y + height, pos.z);

            // Draw rectangle (scene view only, not in game)
            Debug.DrawLine(bottomLeft, bottomRight, Color.red);
            Debug.DrawLine(bottomRight, topRight, Color.red);
            Debug.DrawLine(topRight, topLeft, Color.red);
            Debug.DrawLine(topLeft, bottomLeft, Color.red);
#endif
        }


        public void Deactivate() {
        }
        public void Reset() {
        }
    }
}