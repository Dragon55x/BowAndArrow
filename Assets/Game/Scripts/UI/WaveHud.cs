using UnityEngine;
using UnityEngine.UI;

namespace BAA
{
    public sealed class WaveHud : MonoBehaviour
    {
        [SerializeField] private WaveController waveController;
        [SerializeField] private Text waveText;
        [SerializeField] private Text remainingText;
        [SerializeField] private GameObject banner;
        [SerializeField] private Text bannerText;

        private bool _isSubscribed;

        public void Configure(
            WaveController controller,
            Text targetWaveText,
            Text targetRemainingText,
            GameObject targetBanner,
            Text targetBannerText)
        {
            waveController = controller;
            waveText = targetWaveText;
            remainingText = targetRemainingText;
            banner = targetBanner;
            bannerText = targetBannerText;
        }

        private void Start()
        {
            if (waveController == null || waveText == null || remainingText == null ||
                banner == null || bannerText == null)
            {
                Debug.LogError(
                    "WaveHud requires controller, status Texts, and banner references.",
                    this);
                enabled = false;
                return;
            }

            waveController.WavePreparing += OnWavePreparing;
            waveController.WaveStarted += OnWaveStarted;
            waveController.RemainingEnemiesChanged += OnRemainingChanged;
            waveController.RoomCleared += OnRoomCleared;
            _isSubscribed = true;
            waveText.text = $"第 0/{waveController.TotalWaves} 波";
            remainingText.text = "准备战斗";
            banner.SetActive(false);
        }

        private void OnWavePreparing(int current, int total)
        {
            waveText.text = $"第 {current}/{total} 波";
            remainingText.text = "敌人即将出现";
            bannerText.text = $"第 {current} 波";
            banner.SetActive(true);
        }

        private void OnWaveStarted(int current, int total)
        {
            waveText.text = $"第 {current}/{total} 波";
            banner.SetActive(false);
        }

        private void OnRemainingChanged(int remaining)
        {
            if (waveController.CurrentWaveNumber > 0)
            {
                remainingText.text = $"剩余敌人 {remaining}";
            }
        }

        private void OnRoomCleared()
        {
            waveText.text = "全部波次完成";
            remainingText.text = "剩余敌人 0";
            banner.SetActive(false);
        }

        private void OnDestroy()
        {
            if (!_isSubscribed || waveController == null)
            {
                return;
            }

            waveController.WavePreparing -= OnWavePreparing;
            waveController.WaveStarted -= OnWaveStarted;
            waveController.RemainingEnemiesChanged -= OnRemainingChanged;
            waveController.RoomCleared -= OnRoomCleared;
        }
    }
}
