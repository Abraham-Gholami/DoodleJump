// File: D:\Wagr\WagrGames\WagrGames\Assets\Games\ColorSwitch\Scripts\Scroller\Scroller.cs
using UnityEngine;
using System.Collections.Generic;
namespace ColorSwitch {
    [System.Serializable]
    public class ScrollItem {
        public Transform transform;
        public ScrollItemType Type = ScrollItemType.Default;
        public int blockerIndex = -1;
        public ScrollItem() { }
        public ScrollItem(GameObject obj, ScrollItemType type, int index) {
            this.transform = obj.transform;
            this.Type = type;
            this.blockerIndex = index;
        }
    }
    public enum ScrollItemType {
        Default,
        Blocker,
        SwitcherBlocker
    }
    public class Scroller : MonoBehaviour {
        [Tooltip("A list of all objects that are part of the scrolling world.")]
        public List<ScrollItem> scrollContent;
    }
}