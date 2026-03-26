using System;
using System.Threading;
using CustomLogger;

using NewFrogger.Player.Presentation;
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

            _trafficController.OnLevelTimerEnded += HandleOnLevelTimerEnded;
            _trafficController.OnChangeTrafficSettings += HandleOnChangeTrafficSettings;
            _playerController.OnPlayerHitVehicle += HandleOnPlayerHitVehicle;
            _playerController.OnPlayerReachedVictory += HandleOnPlayerReachedVictory;
        }

        #region Handle Events
        private void HandleOnLevelTimerEnded()
        {
            if (!_playerController.HasPlayerReachedVictory()) EndGameplay();
        }
        private void HandleOnPlayerHitVehicle()
        {
            EndGameplay();
        }
        private async void HandleOnPlayerReachedVictory()
        {
            try
            {
                if (_trafficController.HasNextLevel())
                {
                    _playerController.ResetPlayer();
                    _trafficController.EndLevel();
                    _trafficController.IncreaseLevel();
                    await _view.TimedShowChangeLevelPanel(_generalCTS.Token);
                    await _trafficController.StartLevel(_generalCTS.Token);
                    _playerController.StartGameplay();
                    _playerController.Pause();
                }
                else
                {
                    EndGameplay();
                }
            }
            catch (Exception ex)
            {
                Log.log(ex);
                EndGameplay();
            }
        }
        private async void HandleOnChangeTrafficSettings()
        {
            try
            {
                PauseGameplay();
                var newSettings = await _trafficController.ChangeTrafficSettings(_generalCTS.Token);
                if (newSettings.Equals(default)) return;

                _playerController.HandleTrafficSettingsUpdate(newSettings);
                ResumeGameplay();
            }
            catch (Exception ex)
            {
                Log.log(ex);
            }
        }
        #endregion

        #region Gameplay
        public void StartGameplay()
        {
            _view.StartGameplay();

            _trafficController.StartGameplay(_generalCTS.Token);
            _playerController.StartGameplay();
            _playerController.Pause();

            IsRunning = true;
        }
        public void PauseGameplay()
        {
            _playerController.Pause();
            _trafficController.PauseGameplay();
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
        #endregion

        public void Dispose()
        {
            _view = null;
            _playerController = null;
            _trafficController = null;
            _generalCTS = null;
        }
    }
}