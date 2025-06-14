using Common;
using System;
using Data;
using Helpers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace _Main.Contexts
{
    public class CharacterSelectionContext : Context
    {
        [SerializeField] private string gameSceneName;
        [SerializeField] private Slider maxPlayersSlider;
        [SerializeField] private Button[] playerCharacterChoices;
        [SerializeField] private Image[] tickImages;
        [SerializeField] private Button backButton;
        [SerializeField] private Button startButton;
        [SerializeField] private float customSceneLoadDelay;
        
        private Action goToPreviousPage;
        
        public override void Enter(Action onMoveBack, Action _)
        {
            goToPreviousPage = onMoveBack;
        }

        public override void Exit() { }

        private void Start()
        {
            backButton.AddButtonClickEvent(GoBackToHome);
            startButton.AddButtonClickEvent(StartGame);
            SetupCharacterButtons();
            SetupDataBasedOnPreviousInfo();
            maxPlayersSlider.onValueChanged.AddListener((value) => World.Get.Board.PlayerCountInMatch = (int)value);
        }

        private void GoBackToHome()
        {
            goToPreviousPage();
        }

        private void StartGame()
        {
            var sceneOp = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);
            if (sceneOp == null)
            {
                Debug.LogError("Failed to load game scene");
                return;
            }
            LoadingScreen.ShowLoadingScreen(() => !sceneOp.isDone, () => sceneOp.progress, SetSceneActive, customSceneLoadDelay);
        }

        private static void SetSceneActive()
        {
            var sceneCount = SceneManager.sceneCount;
            var targetScene = SceneManager.GetSceneAt(sceneCount - 1);
            SceneManager.SetActiveScene(targetScene);
        }

        private void SetupCharacterButtons()
        {
            for (var i = 0; i < playerCharacterChoices.Length; i++)
            {
                var choice = playerCharacterChoices[i];
                var localCopy = i;
                choice.AddButtonClickEvent(SelectCharacter);
                continue;

                void SelectCharacter()
                {
                    World.Get.Board.PlayerCharacterIndex = localCopy;
                    SetTick(localCopy);
                }
            }
        }

        private void SetupDataBasedOnPreviousInfo()
        {
            maxPlayersSlider.value = World.Get.Board.PlayerCountInMatch;
            var characterIndex = World.Get.Board.PlayerCharacterIndex;
            playerCharacterChoices[characterIndex].onClick.Invoke();
        }

        private void SetTick(int index)
        {
            for (var i = 0; i < tickImages.Length; i++)
            {
                var status = i == index;
                tickImages[i].gameObject.SetActive(status);
            }
        }
    }
}