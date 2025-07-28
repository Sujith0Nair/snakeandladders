using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SaL.UI
{
    /// <summary>
    /// A generic UI handler that can be used to select any GameObject on the screen.
    /// It works by creating screen-space UI overlays that match the bounds of the target objects.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        [SerializeField] private GameObject _selectionBoxPrefab; // A prefab with a RectTransform and a Button
        [SerializeField] private Transform _canvasTransform; // The parent for the selection boxes
        [SerializeField] private Camera _mainCamera;

        private class SelectableTarget
        {
            public GameObject TargetObject;
            public RectTransform UIPanel;
            public Button UIButton;
        }

        private readonly List<SelectableTarget> _activeTargets = new();
        private Action<GameObject> _onSelectedCallback;

        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false); // Start disabled
        }

        /// <summary>
        /// Begins a selection process.
        /// </summary>
        /// <param name="targets">The list of GameObjects to make selectable.</param>
        /// <param name="onSelected">The callback to invoke when a target is chosen.</param>
        public void StartSelection(List<GameObject> targets, Action<GameObject> onSelected)
        {
            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning("StartSelection called with no targets.");
                return;
            }

            EndSelection(); // Clear any previous state

            _onSelectedCallback = onSelected;
            gameObject.SetActive(true);

            foreach (var target in targets)
            {
                var selectionBoxInstance = Instantiate(_selectionBoxPrefab, _canvasTransform);
                var button = selectionBoxInstance.GetComponent<Button>();
                
                var newTarget = new SelectableTarget
                {
                    TargetObject = target,
                    UIPanel = selectionBoxInstance.GetComponent<RectTransform>(),
                    UIButton = button
                };

                button.onClick.AddListener(() => OnTargetClicked(newTarget));
                _activeTargets.Add(newTarget);
            }
        }

        /// <summary>
        /// Ends the current selection process and hides the UI.
        /// </summary>
        public void EndSelection()
        {
            foreach (var target in _activeTargets)
            {
                if (target.UIPanel != null)
                {
                    Destroy(target.UIPanel.gameObject);
                }
            }
            _activeTargets.Clear();
            _onSelectedCallback = null;
            gameObject.SetActive(false);
        }

        private void OnTargetClicked(SelectableTarget target)
        {
            _onSelectedCallback?.Invoke(target.TargetObject);
            EndSelection();
        }

        private void LateUpdate()
        {
            // We use LateUpdate to ensure all game logic and camera movement for the frame is complete.
            foreach (var target in _activeTargets)
            {
                var renderer = target.TargetObject.GetComponentInChildren<Renderer>();
                if (renderer == null) continue;

                Rect screenRect = GetScreenRectFromBounds(renderer.bounds);
                target.UIPanel.position = new Vector2(screenRect.x, screenRect.y);
                target.UIPanel.sizeDelta = new Vector2(screenRect.width, screenRect.height);
            }
        }

        private Rect GetScreenRectFromBounds(Bounds bounds)
        {
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            corners[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            corners[3] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            corners[4] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            corners[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            corners[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int i = 0; i < 8; i++)
            {
                Vector3 screenPoint = _mainCamera.WorldToScreenPoint(corners[i]);
                minX = Mathf.Min(minX, screenPoint.x);
                maxX = Mathf.Max(maxX, screenPoint.x);
                minY = Mathf.Min(minY, screenPoint.y);
                maxY = Mathf.Max(maxY, screenPoint.y);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
