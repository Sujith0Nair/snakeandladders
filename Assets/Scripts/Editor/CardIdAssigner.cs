using System.Collections.Generic;
using System.Linq;
using Deck;
using UnityEditor;
using UnityEngine;

namespace SaL.Editor
{
    /// <summary>
    /// An editor utility to automatically find all CardSO assets in the project
    /// and assign them unique, sequential IDs.
    /// </summary>
    public static class CardIdAssigner
    {
        [MenuItem("SaL/Assign Unique Card IDs")]
        public static void AssignCardIds()
        {
            var allCardGuids = AssetDatabase.FindAssets($"t:{nameof(CardSO)}");
            var allCards = new List<CardSO>();

            foreach (var guid in allCardGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var card = AssetDatabase.LoadAssetAtPath<CardSO>(path);
                if (card != null)
                {
                    allCards.Add(card);
                }
            }

            // Sort by name to ensure a consistent order
            allCards = allCards.OrderBy(c => c.name).ToList();

            var currentId = 1; // Start IDs from 1
            foreach (var card in allCards)
            {
                var so = new SerializedObject(card);
                var idProperty = so.FindProperty("cardId");
                idProperty.intValue = currentId++;
                so.ApplyModifiedProperties();
                
                EditorUtility.SetDirty(card);
                Debug.Log($"Assigned ID {idProperty.intValue} to {card.name}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Finished assigning IDs to {allCards.Count} cards.");
        }
    }
}
