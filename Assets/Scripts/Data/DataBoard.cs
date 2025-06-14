using UnityEngine;
using System.Collections.Generic;

namespace Data
{
    public class DataBoard
    {
        private static readonly Dictionary<string, string> KeyToPrefName = new()
        {
            { "PlayerCount", "PlayerCountInMatch" },
            { "CharIndex", "PlayerCharacterIndex" }
        };
        
        public int PlayerCountInMatch
        {
            get => GetPlayerCountInMatch(); 
            set => SetPlayerCountInMatch(value);
        }
        
        private static int GetPlayerCountInMatch()
        {
            return PlayerPrefs.GetInt(KeyToPrefName["PlayerCount"], 1);
        }
        
        private static void SetPlayerCountInMatch(int playerCountInMatch)
        {
            PlayerPrefs.SetInt(KeyToPrefName["PlayerCount"], playerCountInMatch);
        }

        public int PlayerCharacterIndex
        {
            get => GetPlayerCharacterIndex(); 
            set => SetPlayerCharacterIndex(value);
        }
        
        private static int GetPlayerCharacterIndex()
        {
            return PlayerPrefs.GetInt(KeyToPrefName["CharIndex"], 0);
        }
        
        private static void SetPlayerCharacterIndex(int playerCharacterIndex)
        {
            PlayerPrefs.SetInt(KeyToPrefName["CharIndex"], playerCharacterIndex);
        }
        
        public static void ClearData()
        {
            foreach (var (_, prefName) in KeyToPrefName)
            {
                PlayerPrefs.DeleteKey(prefName);
            }
        }
    }
}