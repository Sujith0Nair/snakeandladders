using System;
using Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace _Main.Contexts
{
    public class CharacterSelectionContext : Context
    {
        [SerializeField] private Slider maxPlayersSlider;
        [SerializeField] private Button[] playerCharacterChoices;
        [SerializeField] private Image[] tickImages;
        [SerializeField] private Button backButton;
        
        private Action goToPreviousPage;
        
        public override void Enter(Action onMoveBack, Action _)
        {
            goToPreviousPage = onMoveBack;
        }

        public override void Exit() { }

        private void Start()
        {
            backButton.AddButtonClickEvent(GoBackToHome);
            SetupCharacterButtons();
            SetupDataBasedOnPreviousInfo();
            maxPlayersSlider.onValueChanged.AddListener((value) => World.Get.Board.PlayerCountInMatch = (int)value);
        }

        public void GoBackToHome()
        {
            goToPreviousPage();
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