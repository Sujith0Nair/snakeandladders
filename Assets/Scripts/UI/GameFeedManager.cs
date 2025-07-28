using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace SaL.UI
{
    /// <summary>
    /// Manages a scrolling log of game events for all players to see.
    /// </summary>
    public class GameFeedManager : MonoBehaviour
    {
        public static GameFeedManager Instance { get; private set; }

        [SerializeField] private GameObject _logEntryPrefab;
        [SerializeField] private Transform _logContainer;
        [SerializeField] private int _maxLogEntries = 20;

        private Queue<GameObject> _logEntries = new();

        private void Awake()
        {
            Instance = this;
        }

        public void AddLogEntry(string message)
        {
            if (_logEntries.Count >= _maxLogEntries)
            {
                var oldEntry = _logEntries.Dequeue();
                Destroy(oldEntry);
            }

            var newEntry = Instantiate(_logEntryPrefab, _logContainer);
            newEntry.GetComponent<TextMeshProUGUI>().text = message;
            _logEntries.Enqueue(newEntry);
        }
    }
}
