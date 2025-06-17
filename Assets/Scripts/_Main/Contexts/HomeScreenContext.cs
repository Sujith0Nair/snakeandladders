using System;
using Helpers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace _Main.Contexts
{
    public class HomeScreenContext : Context
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitGameButton;
        [SerializeField] private CanvasGroup canvasGroup;
        
        private Action goToCharacterSelectionScreen;
        private Scene? loadedScene;
        
        public override void Enter(Action _, Action onMoveForward)
        {
            goToCharacterSelectionScreen = onMoveForward;
            ToggleStatusIfOtherSceneExists();
        }

        public override void Exit()
        {
            goToCharacterSelectionScreen = null;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
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

        private void ToggleStatusIfOtherSceneExists()
        {
            var sceneCount = SceneManager.sceneCount;
            if (sceneCount == 1)
            {
                return;
            }

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            ToggleCanvasGroup(false);

            var currentScene = gameObject.scene;
            var nextSceneIndex = -1;
    
            for (var i = 0; i < sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i) != currentScene) continue;
                nextSceneIndex = i + 1;
                break;
            }

            if (nextSceneIndex >= 0 && nextSceneIndex < sceneCount)
            {
                loadedScene = SceneManager.GetSceneAt(nextSceneIndex);
            }
        }

        private void OnSceneUnloaded(Scene unloadingScene)
        {
            if (loadedScene != unloadingScene) return;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            ToggleCanvasGroup(true);
        }
        
        private void ToggleCanvasGroup(bool status)
        {
            canvasGroup.alpha = status ? 1 : 0;
            canvasGroup.blocksRaycasts = status;
            canvasGroup.interactable = status;
        }
    }
}