using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BAA
{
    public sealed class UpgradeChoiceController : MonoBehaviour
    {
        private const int ChoiceCount = 3;

        [SerializeField] private PlayerProgression progression;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private Text[] titleTexts;
        [SerializeField] private Text[] descriptionTexts;

        private readonly SkillId[] _currentChoices = new SkillId[ChoiceCount];
        private readonly UnityAction[] _choiceActions = new UnityAction[ChoiceCount];
        private System.Random _random;
        private bool _isShowing;
        private bool _isApplying;

        public void Configure(
            PlayerProgression source,
            PlayerCombat playerCombat,
            GameObject upgradePanel,
            Button[] buttons,
            Text[] titles,
            Text[] descriptions)
        {
            progression = source;
            combat = playerCombat;
            panel = upgradePanel;
            choiceButtons = buttons;
            titleTexts = titles;
            descriptionTexts = descriptions;
        }

        private void Start()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
                return;
            }

            _random = new System.Random(Environment.TickCount);
            panel.SetActive(false);
            progression.LevelUpQueued += OnLevelUpQueued;
            for (var i = 0; i < ChoiceCount; i++)
            {
                var choiceIndex = i;
                _choiceActions[i] = () => SelectChoice(choiceIndex);
                choiceButtons[i].onClick.AddListener(_choiceActions[i]);
            }
        }

        private void OnLevelUpQueued()
        {
            if (!_isShowing)
            {
                ShowChoices();
            }
        }

        private void ShowChoices()
        {
            var choices = SkillCatalog.CreateThreeUniqueChoices(_random, combat.RuntimeStats);
            for (var i = 0; i < ChoiceCount; i++)
            {
                _currentChoices[i] = choices[i];
                titleTexts[i].text = SkillCatalog.GetTitle(choices[i]);
                descriptionTexts[i].text = SkillCatalog.GetDescription(choices[i]);
                choiceButtons[i].interactable = true;
            }

            _isApplying = false;
            _isShowing = true;
            panel.SetActive(true);
            GamePauseCoordinator.Acquire(this);
        }

        private void SelectChoice(int index)
        {
            if (!_isShowing || _isApplying || index < 0 || index >= ChoiceCount)
            {
                return;
            }

            _isApplying = true;
            for (var i = 0; i < ChoiceCount; i++)
            {
                choiceButtons[i].interactable = false;
            }

            SkillApplier.Apply(_currentChoices[index], combat.RuntimeStats);
            progression.ConsumePendingLevel();
            if (progression.PendingLevels > 0)
            {
                ShowChoices();
                return;
            }

            _isShowing = false;
            panel.SetActive(false);
            GamePauseCoordinator.Release(this);
        }

        private bool ValidateConfiguration()
        {
            var arraysValid = choiceButtons != null && choiceButtons.Length == ChoiceCount &&
                              titleTexts != null && titleTexts.Length == ChoiceCount &&
                              descriptionTexts != null && descriptionTexts.Length == ChoiceCount;
            if (progression != null && combat != null && panel != null && arraysValid &&
                AllElementsAssigned())
            {
                return true;
            }

            Debug.LogError(
                "UpgradeChoiceController requires progression, combat, panel, and three complete choices.",
                this);
            return false;
        }

        private bool AllElementsAssigned()
        {
            for (var i = 0; i < ChoiceCount; i++)
            {
                if (choiceButtons[i] == null || titleTexts[i] == null || descriptionTexts[i] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnDisable()
        {
            if (!_isShowing)
            {
                return;
            }

            _isShowing = false;
            _isApplying = false;
            if (panel != null)
            {
                panel.SetActive(false);
            }

            GamePauseCoordinator.Release(this);
        }

        private void OnDestroy()
        {
            if (progression != null)
            {
                progression.LevelUpQueued -= OnLevelUpQueued;
            }

            if (choiceButtons != null)
            {
                for (var i = 0; i < choiceButtons.Length; i++)
                {
                    if (choiceButtons[i] != null && _choiceActions[i] != null)
                    {
                        choiceButtons[i].onClick.RemoveListener(_choiceActions[i]);
                    }
                }
            }

            GamePauseCoordinator.Release(this);
        }
    }
}
