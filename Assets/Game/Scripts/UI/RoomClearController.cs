using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BAA
{
    public sealed class RoomClearController : MonoBehaviour
    {
        [SerializeField] private WaveController waveController;
        [SerializeField] private CharacterHealth playerHealth;
        [SerializeField] private PlayerProgression playerProgression;
        [SerializeField] private GameObject roomClearPanel;
        [SerializeField] private Button replayButton;
        [SerializeField, Min(0f)] private float clearHealAmount = 20f;

        private bool _isCleared;
        private bool _isRestarting;

        public void Configure(
            WaveController controller,
            CharacterHealth health,
            PlayerProgression progression,
            GameObject panel,
            Button button,
            float healAmount)
        {
            waveController = controller;
            playerHealth = health;
            playerProgression = progression;
            roomClearPanel = panel;
            replayButton = button;
            clearHealAmount = Mathf.Max(0f, healAmount);
        }

        private void Start()
        {
            if (waveController == null || playerHealth == null || playerProgression == null ||
                roomClearPanel == null || replayButton == null)
            {
                Debug.LogError(
                    "RoomClearController requires wave, health, progression, panel, and button references.",
                    this);
                enabled = false;
                return;
            }

            roomClearPanel.SetActive(false);
            replayButton.interactable = true;
            waveController.RoomCleared += OnRoomCleared;
            replayButton.onClick.AddListener(Replay);
        }

        private void OnRoomCleared()
        {
            if (_isCleared)
            {
                return;
            }

            _isCleared = true;
            StartCoroutine(CompleteRoomAfterDelay());
        }

        private IEnumerator CompleteRoomAfterDelay()
        {
            while (ExperienceOrb.PendingCount > 0 || playerProgression.PendingLevels > 0)
            {
                yield return null;
            }

            if (playerHealth.IsDead)
            {
                yield break;
            }

            playerHealth.Heal(clearHealAmount);
            roomClearPanel.SetActive(true);
            GamePauseCoordinator.Acquire(this);
        }

        private void Replay()
        {
            if (_isRestarting)
            {
                return;
            }

            _isRestarting = true;
            replayButton.interactable = false;
            GamePauseCoordinator.Release(this);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            GamePauseCoordinator.Release(this);
            if (waveController != null)
            {
                waveController.RoomCleared -= OnRoomCleared;
            }

            if (replayButton != null)
            {
                replayButton.onClick.RemoveListener(Replay);
            }
        }
    }
}
