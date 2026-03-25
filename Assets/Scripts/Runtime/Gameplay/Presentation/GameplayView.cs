using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Threading;

namespace NewFrogger.Gameplay.Presentation
{
    public class GameplayView : MonoBehaviour
    {
        public event Action OnStart;
        public event Action OnStop;

        [SerializeField] private GameObject _startMenuPanel;
        [SerializeField] private Button _startBt;
        [SerializeField] private Button _stopBt;

        [SerializeField] private GameObject _gameplayPanel;
        [SerializeField] private GameObject _changeLevelPanel;

        public void Initialize()
        {
            _startBt.onClick.AddListener(PublishOnStart);
            _stopBt.onClick.AddListener(PublishOnStop);

            _startMenuPanel.SetActive(true);
            _gameplayPanel.SetActive(false);
            _changeLevelPanel.SetActive(false);
        }

        public void StartGameplay()
        {
            HideStartMenu();
            ShowGameplayPanel();
        }

        private void PublishOnStart() => OnStart?.Invoke();
        private void PublishOnStop() => OnStop?.Invoke();

        public void ShowStartMenu() => _startMenuPanel.SetActive(true);
        public void HideStartMenu() => _startMenuPanel.SetActive(false);

        public void ShowGameplayPanel() => _gameplayPanel.SetActive(true);
        public void HideGameplayPanel() => _gameplayPanel.SetActive(false);

        public async UniTask TimedShowChangeLevelPanel(CancellationToken ct)
        {
            try
            {
                _changeLevelPanel.SetActive(true);
                await UniTask.Delay(TimeSpan.FromSeconds(2)).AttachExternalCancellation(ct);
                _changeLevelPanel.SetActive(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OnDestroy()
        {
            _startBt.onClick.RemoveAllListeners();
            _stopBt.onClick.RemoveAllListeners();
        }

    }
}