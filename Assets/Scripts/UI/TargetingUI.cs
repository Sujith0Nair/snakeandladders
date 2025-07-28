using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SaL.UI
{
    /// <summary>
    /// Manages the UI for selecting a target player for an offensive card.
    /// </summary>
    public class TargetingUI : MonoBehaviour
    {
        // This class will be responsible for displaying a list of other players
        // and returning the ClientId of the selected player.

        public Action<ulong> OnTargetSelected;

        public void Show(List<ulong> potentialTargetIds)
        {
            // 1. Clear any existing target buttons.
            // 2. For each target ID, instantiate a button.
            // 3. The button's text would be the player's name (fetched from a player data cache).
            // 4. The button's onClick event would call OnTargetSelected(clientId) and then hide the panel.
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
