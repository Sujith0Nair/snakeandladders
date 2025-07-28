using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace SaL.UI
{
    /// <summary>
    /// A static manager to show modal popups to the user.
    /// It works by loading a prefab from the Resources folder.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        private static PopupManager _instance;

        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private GameObject _popupCanvas;

        public static void ShowPopup(string title, string message, Action onConfirm = null)
        {
            if (_instance == null)
            {
                var prefab = Resources.Load<PopupManager>("UIPopup");
                if (prefab == null)
                {
                    Debug.LogError("PopupManager prefab not found in Resources/UIPopup!");
                    return;
                }
                _instance = Instantiate(prefab);
            }
            _instance.InternalShow(title, message, onConfirm);
        }

        private void InternalShow(string title, string message, Action onConfirm)
        {
            _titleText.text = title;
            _messageText.text = message;
            
            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(() =>
            {
                onConfirm?.Invoke();
                _popupCanvas.SetActive(false);
            });

            _popupCanvas.SetActive(true);
        }
    }
}
