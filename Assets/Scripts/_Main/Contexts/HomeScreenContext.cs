using System;
using Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace _Main.Contexts
{
    public class HomeScreenContext : Context
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitGameButton;
        
        private Action goToCharacterSelectionScreen;
        
        public override void Enter(Action _, Action onMoveForward)
        {
            goToCharacterSelectionScreen = onMoveForward;
        }

        public override void Exit()
        {
            goToCharacterSelectionScreen = null;
        }

        private void Start()
        {
            startGameButton.AddButtonClickEvent(ShowGameOptionsScreen);
            quitGameButton.AddButtonClickEvent(EndGame);
        }

        private void ShowGameOptionsScreen()
        {
            goToCharacterSelectionScreen();
        }

        private static void EndGame()
        {
            Application.Quit();
        }
    }
}