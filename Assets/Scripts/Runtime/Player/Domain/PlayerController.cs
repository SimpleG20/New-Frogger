using System;
using UnityEngine;

using NewFrogger.Core.Domain;
using NewFrogger.Gameplay.Data;
using NewFrogger.Player.Domain;
using NewFrogger.Traffic.Domain;

namespace NewFrogger.Player.Presentation
{
    public class PlayerController : MonoBehaviour
    {
        public event Action OnPlayerReachedVictory;
        public event Action OnPlayerHitVehicle;

        [SerializeField] private PlayerView _view;

        private PlayerModel _model;

        public void Initialize(PlayerModel model)
        {
            Vector3 initialPos = _view.transform.position;

            _model = model;
            _model.SetInitialPosition(initialPos);
            _model.SetPosition(initialPos);

            _view.Initialize(_model);
            _view.OnVehicleHit += HandleOnPlayerHitVehicle;
            _view.OnPlayerFinishedMovement += HandleOnPlayerFinishedMovement;
        }

        private void HandleOnPlayerFinishedMovement()
        {
            if (_model.IsInVictoryPos()) OnPlayerReachedVictory?.Invoke();
        }
        private void HandleOnPlayerHitVehicle()
        {
            OnPlayerHitVehicle?.Invoke();
        }
        public void HandleTrafficSettingsUpdate(TrafficSettings newSettings)
        {
            _model.ChangeSpeedAccordingToWeather(newSettings.Weather);
        }

        public void StartGameplay()
        {
            _model.SetActive(true);
            _model.SetCanMove(true);
        }
        public void Pause()
        {
            _model.SetCanMove(false);
        }
        public void Resume()
        {
            _model.SetCanMove(true);
        }
        public void EndGameplay()
        {
            ResetPlayer();
        }
        public void ResetPlayer()
        {
            _model.Reset();
        }

        private void OnDestroy()
        {
            OnPlayerHitVehicle = null;
            OnPlayerReachedVictory = null;
        }
    }
}
