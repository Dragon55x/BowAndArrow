using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BAA
{
    public sealed class GameOverController : MonoBehaviour
    {
        [SerializeField] private CharacterHealth playerHealth;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;

        private bool _isGameOver;
        private bool _isRestarting;

        public void Configure(CharacterHealth health, GameObject panel, Button button)
        {
            playerHealth = health;
            gameOverPanel = panel;
            restartButton = button;
        }

        private void Awake()
        {
            GamePauseCoordinator.Reset();
        }

        private void Start()
        {
            if (playerHealth == null || gameOverPanel == null || restartButton == null)
            {
                Debug.LogError(
                    "GameOverController requires CharacterHealth, panel, and restart button references.",
                    this);
                enabled = false;
                return;
            }

            gameOverPanel.SetActive(false);
            restartButton.interactable = true;
            playerHealth.Died += OnPlayerDied;
            restartButton.onClick.AddListener(Restart);
        }

        private void OnPlayerDied(CharacterHealth _)
        {
            if (_isGameOver)
            {
                return;
            }

            _isGameOver = true;
            gameOverPanel.SetActive(true);
            GamePauseCoordinator.Acquire(this);
        }

        private void Restart()
        {
            if (_isRestarting)
            {
                return;
            }

            _isRestarting = true;
            restartButton.interactable = false;
            GamePauseCoordinator.Release(this);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            GamePauseCoordinator.Release(this);
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(Restart);
            }
        }
    }
}
