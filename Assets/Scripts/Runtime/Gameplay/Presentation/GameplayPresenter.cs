using System;
using UnityEngine;
using UnityEngine.InputSystem;

using CustomLogger;
using Cysharp.Threading.Tasks;
using NewFrogger.Gameplay.Composition;
using NewFrogger.Gameplay.Data;
using NewFrogger.Gameplay.Domain;
using NewFrogger.Player.Domain;
using NewFrogger.Player.Presentation;
using NewFrogger.Vehicle.Domain;
using NewFrogger.Vehicle.Presentation;

namespace NewFrogger.Gameplay.Presentation
{
    public class GameplayPresenter : MonoBehaviour
    {
        [SerializeField] private GameplaySettingsSO _gameplaySettingsSO;
        [SerializeField] private TrafficSpawner _trafficSpawner;
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private PlayerInput _input;
        [SerializeField] private string _apiBaseUrl = "https://f98dcad7-664e-44fb-8c6f-79c727a98caf.mock.pstmn.io";

        private PlayerModel _player;
        private TrafficModel _traffic;
        private GameplayInputHandler _gameplayInput;
        private GameplayCompositionRoot _compositionRoot;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            Log.SetLogger(new EditorLogger());

            _compositionRoot = new GameplayCompositionRoot(_apiBaseUrl);

            InitializeTraffic();
            InitializePlayer();

            _gameplayInput = new GameplayInputHandler(_player, _input);

            Log.log("Gameplay Presenter Initialized");
        }
        private void InitializeTraffic()
        {
            _traffic = new TrafficModel(_gameplaySettingsSO, _compositionRoot.GetTrafficStatsService());
            _traffic.OnTrafficSettingsChanged += HandleOnTrafficSettingsChanged;
            _traffic.OnStartNewLevel += HandleOnTrafficStartNewLevel;
            _traffic.OnStopLevel += HandleOnTrafficStopLevel;
        }
        private void InitializePlayer()
        {
            Vector3 initialPos = _playerView.transform.position;
            _player = new PlayerModel(initialPos, _gameplaySettingsSO, _compositionRoot.GetTimeProvider());
            _playerView.Initialize(_player);
            _playerView.OnVehicleHit += HandleOnPlayerHitVehicle;
            _playerView.OnPlayerFinishedMovement += HandleOnPlayerFinishedMovement;
        }

        #region Handle Events
        private void HandleOnTrafficStopLevel()
        {
            _trafficSpawner.StopSpawning();
            _trafficSpawner.HideVehicles();
        }
        private void HandleOnTrafficStartNewLevel()
        {
            _trafficSpawner.HandleStart(_traffic.CurrentTrafficSettings, 10);
        }
        private void HandleOnTrafficSettingsChanged(TrafficSettings newSettings)
        {
            _trafficSpawner.UpdateTrafficSettings(newSettings);
            _player.ChangeSpeedAccordingToWeather(newSettings.Weather);
        }
        private void HandleOnPlayerFinishedMovement()
        {
            if (_player.Position.x == _gameplaySettingsSO.xVictory) Victory();
            else Log.log("Not yet");
        }
        private void HandleOnPlayerHitVehicle()
        {
            EndLevel();

            // TODO: view shows the init button again
        }
        #endregion

        [ContextMenu("Start")]
        private void StartGameplay()
        {
            _ = _traffic.StartNewLevel();
            _player.SetActive(true);
        }
        private void Victory()
        {
            EndLevel();

            if (_traffic.CanNextLevel())
            {
                _ = UniTask.Delay(TimeSpan.FromSeconds(2)).ContinueWith(StartGameplay);
            }
            else
            {
                Log.log("All levels done");
            }
        }
        private void EndLevel()
        {
            _traffic?.EndLevel();
            _player?.Reset();
        }


        private void OnDestroy()
        {
            if (_traffic != null)
            {
                _traffic.OnTrafficSettingsChanged -= HandleOnTrafficSettingsChanged;
                _traffic.OnStartNewLevel -= HandleOnTrafficStartNewLevel;
                _traffic.OnStopLevel -= HandleOnTrafficStopLevel;
                _traffic.Dispose();
            }
            
            _gameplayInput?.Dispose();
        }
    }
}