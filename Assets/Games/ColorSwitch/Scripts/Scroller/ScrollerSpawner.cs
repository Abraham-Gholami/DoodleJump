// File: D:\Wagr\WagrGames\WagrGames\Assets\Games\ColorSwitch\Scripts\Scroller\ScrollerSpawner.cs
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ColorSwitch {
    public class ScrollerSpawner : MonoBehaviour {
        [SerializeField] private Scroller scroller;
        [SerializeField] private BlockerPool pool;
        [SerializeField] private BlockerPool switcherPool; // Pool for switcher prefabs
        [SerializeField] private Camera cam;
        [SerializeField] private float TopBuffer = 10;
        [SerializeField] private float BottomBufferSize = 10;
        [SerializeField] private float ElementMargin = 2f;

        private int spawnCount = 0;

        private void Start() {
            HandleTop();
        }

        private void LateUpdate() {
            HandleOffscreen();
            HandleTop();
        }

        private void HandleTop() {
            if (scroller.scrollContent.Count <= 0) {
                SpawnNewTop(null);
                return;
            }

            int topMostIndex = -1;
            float max_y = float.MinValue;
            for (int i = 0; i < scroller.scrollContent.Count; i++) {
                float current_y = scroller.scrollContent[i].transform.position.y;
                if (current_y > max_y) {
                    max_y = current_y;
                    topMostIndex = i;
                }
            }

            if (topMostIndex != -1) {
                ScrollItem topMostItem = scroller.scrollContent[topMostIndex];
                float cameraTop = cam.transform.position.y + cam.orthographicSize;
                if (topMostItem.transform.position.y < cameraTop + TopBuffer) {
                    SpawnNewTop(topMostItem);
                    // Recursively call to fill the screen quickly
                    HandleTop();
                }
            }
        }

        private void SpawnNewTop(ScrollItem topMostItem) {
            // --- Step 1: Calculate the final position for the NEW BLOCKER ---
            float newBlockerY;
            float previousItemTop = 0; // The Y-coordinate of the top edge of the last item

            if (topMostItem != null) {
                previousItemTop = topMostItem.transform.position.y + topMostItem.transform.GetComponent<IBlocker>().Height;
                newBlockerY = previousItemTop + ElementMargin;
            }
            else {
                // This is the very first item being spawned
                newBlockerY = cam.transform.position.y;
            }

            // --- Step 2: Get the new blocker from the pool ---
            GameObject newBlocker;
            int newBlockerIndex;

            if (spawnCount < 2) {
                newBlocker = pool.Get(0);
                newBlockerIndex = 0;
            }
            else {
                (newBlocker, newBlockerIndex) = pool.GetRandom();
            }
            spawnCount++;

            var blockerComponent = newBlocker.GetComponent<BaseBlocker>();

            // --- Step 3: Check if a switcher is needed and spawn it if so ---
            bool needsSwitcher = switcherPool != null && blockerComponent.BlockedColors != null && blockerComponent.BlockedColors.Count > 0;

            // We can only spawn a switcher if there's a previous item to create a margin with.
            if (needsSwitcher && topMostItem != null) {
                (GameObject switcherItem, int switcherIndex) = switcherPool.GetRandom();

                // Configure the switcher's colors to provide a valid path
                ColorSwitcher colorSwitcher = switcherItem.transform.GetChild(0).GetComponent<ColorSwitcher>();
                if (colorSwitcher != null) {
                    colorSwitcher.isRandom = false;
                    var allVariants = System.Enum.GetValues(typeof(ColorVariants)).Cast<ColorVariants>();
                    colorSwitcher.TargetVariants = allVariants.Except(blockerComponent.BlockedColors).ToList();
                    //foreach (var item in blockerComponent.BlockedColors) {
                    //    GameLogger.Log($"[ScrollerSpawner] blocked color: {item}");
                    //}
                    //foreach (var item in allVariants.Except(blockerComponent.BlockedColors).ToList()) {
                    //    GameLogger.Log($"[ScrollerSpawner] selection color: {item}");
                    //}
                    colorSwitcher.UpdateColors();
                }
                else {
                }

                // Position the switcher exactly in the middle of the margin
                float switcherY = previousItemTop + (ElementMargin / 2f) - (switcherItem.GetComponent<IBlocker>().Height / 2);
                switcherItem.transform.position = new Vector3(0, switcherY, 0);

                switcherItem.GetComponent<IBlocker>().Activate();
                switcherItem.transform.SetParent(transform);
                scroller.scrollContent.Add(new ScrollItem(switcherItem, ScrollItemType.SwitcherBlocker, switcherIndex));
            }

            // --- Step 4: Spawn the NEW BLOCKER at its final calculated position ---
            newBlocker.transform.position = new Vector3(0, newBlockerY, 0);
            newBlocker.GetComponent<IBlocker>().Activate();
            newBlocker.transform.SetParent(transform);
            scroller.scrollContent.Add(new ScrollItem(newBlocker, ScrollItemType.Blocker, newBlockerIndex));
        }

        private void HandleOffscreen() {
            float camBottom = cam.transform.position.y - cam.orthographicSize;
            for (int i = scroller.scrollContent.Count - 1; i >= 0; i--) {
                ScrollItem scrollItem = scroller.scrollContent[i];
                if (scrollItem.transform == null) {
                    scroller.scrollContent.RemoveAt(i);
                    continue;
                }
                if (scrollItem.transform.position.y < camBottom - BottomBufferSize) {
                    if (scrollItem.Type == ScrollItemType.Blocker && scrollItem.blockerIndex != -1) {
                        scrollItem.transform.gameObject.GetComponent<IBlocker>().Deactivate();
                        pool.Return(scrollItem.transform.gameObject, scrollItem.blockerIndex);
                    }
                    else if (scrollItem.Type == ScrollItemType.SwitcherBlocker && scrollItem.blockerIndex != -1) {
                        scrollItem.transform.gameObject.GetComponent<IBlocker>().Deactivate();
                        switcherPool.Return(scrollItem.transform.gameObject, scrollItem.blockerIndex);
                    }
                    else {
                        Destroy(scrollItem.transform.gameObject);
                    }
                    scroller.scrollContent.RemoveAt(i);
                }
            }
        }
    }
}