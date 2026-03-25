using System;
using System.Threading;
using System.Threading.Tasks;
using CustomLogger;

using NewFrogger.Player.Presentation;
using NewFrogger.Traffic.Domain;
using NewFrogger.Traffic.Presentation;

namespace NewFrogger.Gameplay.Presentation
{
    public class GameplayLifecycle
    {
        public bool IsRunning { get; private set; }

        private GameplayView _view;

        private PlayerController _playerController;
        private TrafficController _trafficController;

        private CancellationTokenSource _generalCTS;

        public GameplayLifecycle(GameplayView view, PlayerController player, TrafficController traffic, CancellationTokenSource generalCts)
        {
            _view = view;
            _playerController = player;
            _trafficController = traffic;
            _generalCTS = generalCts;
        }

        public void StartGameplay()
        {
            _playerController.OnPlayerHitVehicle += HandleOnPlayerHitVehicle;
            _playerController.OnPlayerReachedVictory += HandleOnPlayerReachedVictory;

            _view.StartGameplay();

            _trafficController.OnTrafficSettingsChanged += ChangeTrafficSettings;
            _trafficController.StartGameplay(_generalCTS.Token);
            _playerController.StartGameplay();

            IsRunning = true;
        }

        private async void HandleOnPlayerReachedVictory()
        {
            if (_trafficController.HasNextLevel())
            {
                _playerController.ResetPlayer();
                _trafficController.EndLevel();
                _trafficController.IncreaseLevel();
                await _view.ShowChangeLevelPanel(_generalCTS.Token);
                await _trafficController.StartLevel(_generalCTS.Token);
                _playerController.StartGameplay();
            }
        }

        private void HandleOnPlayerHitVehicle()
        {
            EndGameplay();
        }

        public void PauseGameplay()
        {
            _playerController.Pause();
            _trafficController.PauseGameplay();
        }

        public async void ChangeTrafficSettings(TrafficSettings newSettings)
        {
            try
            {
                PauseGameplay();
                await _trafficController.ShowChangeTrafficSettings(_generalCTS.Token);
                _playerController.HandleTrafficSettingsUpdate(newSettings);
                ResumeGameplay();
            }
            catch (Exception ex)
            {
                Log.log(ex);
            }
        }

        public void ResumeGameplay()
        {
            _playerController.Resume();
            _trafficController.ResumeGameplay();
        }

        public void EndGameplay()
        {
            IsRunning = false;

            _playerController.EndGameplay();
            _trafficController.EndGameplay();

            _view.HideGameplayPanel();
            _view.ShowStartMenu();
        }
    }
}