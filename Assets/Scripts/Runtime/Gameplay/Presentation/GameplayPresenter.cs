using System;
using System.Threading;
using System.Threading.Tasks;
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
using NewFrogger.Traffic.Presentation;
using NewFrogger.Traffic.Domain;

namespace NewFrogger.Gameplay.Presentation
{
    public class GameplayPresenter : MonoBehaviour
    {
        [SerializeField] private string _apiBaseUrl = "https://f98dcad7-664e-44fb-8c6f-79c727a98caf.mock.pstmn.io";
        [SerializeField] private GameplaySettingsSO _gameplaySettingsSO;
        [SerializeField] private GameplayView _gameplayView;

        [Header("Traffic")]
        [SerializeField] private TrafficSpawner _trafficSpawner;
        [SerializeField] private TrafficView _trafficView;

        [Header("Player")]
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private PlayerInput _input;

        private PlayerModel _player;
        private TrafficModel _traffic;
        private GameplayInputHandler _gameplayInput;
        private GameplayCompositionRoot _compositionRoot;

        private CancellationTokenSource _generalCTS;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            _generalCTS = new CancellationTokenSource();
            _compositionRoot = new GameplayCompositionRoot(_apiBaseUrl);

            InitializeTraffic();
            InitializePlayer();

            _gameplayInput = new GameplayInputHandler(_player, _input);

            _gameplayView.Initialize();
            _gameplayView.OnStart += StartGameplay;
            _gameplayView.OnStop += StopGameplay;

            Log.log("Gameplay Presenter Initialized");
        }
        private void InitializeTraffic()
        {
            _traffic = new TrafficModel(_gameplaySettingsSO, _compositionRoot.GetTrafficStatsService());
            _traffic.OnTrafficSettingsChanged += HandleOnTrafficSettingsChanged;
            _traffic.OnStartNewLevel += HandleOnTrafficStartNewLevel;
            _traffic.OnStopLevel += HandleOnTrafficStopLevel;
            _traffic.OnGameEnded += HandleOnGameEnded;

            _trafficView.Initialize(_traffic);
            _trafficView.OnPanelHid += HandleOnPanelHid;
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
        private void HandleOnPanelHid()
        {
            _player.SetCanMove(true);
        }
        private void HandleOnTrafficStopLevel()
        {
            _trafficSpawner.StopSpawning();
            _trafficSpawner.HideVehicles();
        }
        private void HandleOnTrafficStartNewLevel(int level, int time)
        {
            _trafficSpawner.HandleStart(_traffic.CurrentTrafficSettings, 10);
        }
        private void HandleOnTrafficSettingsChanged(TrafficSettings newSettings)
        {
            _trafficSpawner.UpdateTrafficSettings(newSettings);
            _player.ChangeSpeedAccordingToWeather(newSettings.Weather);
            _player.SetCanMove(false);
        }
        private void HandleOnPlayerFinishedMovement()
        {
            if (_player.Position.x == _gameplaySettingsSO.xVictory) Victory();
            else Log.log("Not yet");
        }
        private void HandleOnPlayerHitVehicle()
        {
            EndLevel();
        }
        private void HandleOnGameEnded()
        {
            _trafficSpawner.StopSpawning();
            _trafficSpawner.HideVehicles();
            _player.SetCanMove(false);
            StopGameplay();
        }
        #endregion

        private void StartGameplay()
        {
            _generalCTS = new CancellationTokenSource();
            _gameplayView.HideStartMenu();
            _gameplayView.ShowGameplayPanel();
            StartNewLevel();
        }
        private void StartNewLevel()
        {
            try
            {
                var start = _traffic.StartNewLevel(_generalCTS.Token);
                _player.SetActive(true);
                _player.SetCanMove(true);
            }
            catch(Exception ex)
            {
                Log.log(ex);
                StopGameplay();
            }
        }
        private void StopGameplay()
        {
            _generalCTS?.Cancel();

            _player.SetActive(false);
            _player.SetCanMove(false);

            _gameplayView.HideGameplayPanel();
            _gameplayView.ShowStartMenu();
        }
        private async UniTaskVoid Victory()
        {
            EndLevel();

            if (_traffic.CanNextLevel())
            {
                await _gameplayView.ShowChangeLevelPanel(_generalCTS.Token);
                StartNewLevel();
            }
            else
            {
                StopGameplay();
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
                _traffic.OnGameEnded -= HandleOnGameEnded;
                _traffic.Dispose();
            }

            if (_trafficView != null)
            {
                _trafficView.OnPanelHid -= HandleOnPanelHid;
            }

            if (_playerView != null)
            {
                _playerView.OnPlayerFinishedMovement -= HandleOnPlayerFinishedMovement;
                _playerView.OnVehicleHit -= HandleOnPlayerHitVehicle;
            }

            _gameplayInput?.Dispose();
        }
    }
}