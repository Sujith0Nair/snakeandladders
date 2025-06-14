using UnityEngine.UI;
using UnityEngine.Events;

namespace Helpers
{
    public static class UIHelpers
    {
        public static void AddButtonClickEvent(this Button button, UnityAction action, bool removeExistingEvents = true)
        {
            if (removeExistingEvents)
            {
                button.onClick.RemoveAllListeners();
            }
            button.onClick.AddListener(action);
        }
    }
}