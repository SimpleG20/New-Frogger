using CustomLogger;
using NewFrogger.Core.Data;
using NewFrogger.Gameplay.Data;
using NewFrogger.Gameplay.Domain;
using NewFrogger.Player.Domain;
using NewFrogger.Player.Presentation;
using NewFrogger.Traffic.Data.Datasources;
using NewFrogger.Traffic.Data.Repositories;
using NewFrogger.Traffic.Domain.Services;
using NewFrogger.Vehicle.Domain;
using NewFrogger.Vehicle.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NewFrogger.Gameplay.Presentation
{
    public class GameplayPresenter : MonoBehaviour
    {
        [SerializeField] private GameplaySettingsSO _gameplaySettingsSO;
        [SerializeField] private TrafficSpawner _trafficSpawner;
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private PlayerInput _input;

        private PlayerModel _player;
        private TrafficModel _traffic;
        private GameplayInputHandler _gameplayInput;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            Log.SetLogger(new EditorLogger());

            // TODO: Move Composition Root conceptually
            var datasource = new ApiTrafficDataSource("https://f98dcad7-664e-44fb-8c6f-79c727a98caf.mock.pstmn.io");
            var repo = new TrafficRepositoryImpl(datasource);
            var useCase = new GetTrafficStatsService(repo);
            var timeProvider = new UnityTimeProvider();

            _traffic = new TrafficModel(_gameplaySettingsSO, useCase);
            _traffic.OnTrafficSettingsChanged += HandleOnTrafficSettingsChanged;
            _traffic.OnStartNewLevel += HandleOnStartNewLevel;
            _traffic.OnStopLevel += HandleOnStopLevel;

            _player = new PlayerModel(_playerView.transform.position, _gameplaySettingsSO, timeProvider);
            _playerView.Initialize(_player);
            _playerView.OnVehicleHit += HandleOnVehicleHit;

            _gameplayInput = new GameplayInputHandler(_player, _input);

            Log.log("Gameplay Presenter Initialized");
        }

        private void HandleOnVehicleHit()
        {
           print("Hit"); 
        }

        private void HandleOnStopLevel()
        {
            _trafficSpawner.StopSpawning();
            _trafficSpawner.HideVehicles();
        }

        private void HandleOnStartNewLevel()
        {
            _trafficSpawner.HandleStart(_traffic.CurrentTrafficSettings, 10);
        }

        private void HandleOnTrafficSettingsChanged(TrafficSettings newSettings)
        {
            Log.log("New traffic settings set");
            _trafficSpawner.UpdateTrafficSettings(newSettings);
            _player.ChangeSpeedAccordingToWeather(newSettings.Weather);
        }

        [ContextMenu("Start")]
        private void StartGameplay()
        {
            _ = _traffic.StartNewLevel();
            _player.SetActive(true);
        }

        [ContextMenu("End")]
        private void EndGameplay()
        {
            _traffic?.EndLevel();
            _gameplayInput?.Dispose();
        }

        private void OnDestroy()
        {
            if (_traffic != null)
            {
                _traffic.OnTrafficSettingsChanged -= HandleOnTrafficSettingsChanged;
                _traffic.OnStartNewLevel -= HandleOnStartNewLevel;
                _traffic.OnStopLevel -= HandleOnStopLevel;
                _traffic.Dispose();
            }
            
            _gameplayInput?.Dispose();
        }
    }
}