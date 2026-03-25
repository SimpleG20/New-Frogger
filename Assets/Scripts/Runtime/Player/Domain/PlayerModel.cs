using System;
using UnityEngine;

using NewFrogger.Core.Domain;
using NewFrogger.Traffic.Domain.Enums;

namespace NewFrogger.Player.Domain
{
    public class PlayerModel
    {
        public event Action<bool> OnActiveChanged;
        public event Action<float, Vector3> OnPositionChanged;

        public Vector3 Position => _position;
        public bool Active { get; private set; }

        private float _delayFactor;
        private float _speedFactorRelatedToWeather;
        private float _gridMovementFactor;
        private float _timeGap;
        private float _lastTimeMoved;

        private bool _canMove;

        private Tuple<float, float> _limitsX;
        private Tuple<float, float> _limitsZ;
        
        private Vector3 _position;
        private Vector3 _initialPos;

        private readonly float _xVictory;
        private readonly ITimeProvider _timeProvider;

        public PlayerModel(IPlayerSettings settings, ITimeProvider timeProvider)
        {
            _initialPos = Vector3.zero;
            _position = _initialPos;
            _xVictory = settings.xVictory;

            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _speedFactorRelatedToWeather = 1;
            _delayFactor = settings.PlayerDelayFactor;
            _gridMovementFactor = settings.GridMovementFactor;
            _lastTimeMoved = _timeProvider.Time;
            _limitsX = new Tuple<float, float>(settings.MinX, settings.MaxX);
            _limitsZ = new Tuple<float, float>(settings.MinZ, settings.MaxZ);

            _timeGap = 0;
            _canMove = false;

            SetActive(false);
        }

        public void SetInitialPosition(Vector3 position) => _initialPos = position;
        public void SetPosition(Vector3 newPosition) => _position = newPosition;
        public void SetCanMove(bool value) => _canMove = value;
        public void SetActive(bool value)
        {
            if (value == Active) return;
            Active = value;
            OnActiveChanged?.Invoke(value);
        }

        public bool IsInVictoryPos() => _position.x >= _xVictory;

        public void Move(Vector2 direction)
        {
            if (!IsMovementValid(direction)) return;

            const float BaseTimeGap = 0.5f;

            _timeGap = BaseTimeGap + (_delayFactor * (1 - _speedFactorRelatedToWeather));
            _lastTimeMoved = _timeProvider.Time;

            float newX = _position.x + (direction.y * _gridMovementFactor);
            float newZ = _position.z + (-direction.x * _gridMovementFactor);
            _position = new Vector3(
                Math.Clamp(newX, _limitsX.Item1, _limitsX.Item2),
                _position.y,
                Math.Clamp(newZ, _limitsZ.Item1, _limitsZ.Item2)
            );

            OnPositionChanged?.Invoke(_timeGap, _position);
        }
        private bool IsMovementValid(Vector2 direction)
        {
            if (!Active) return false;
            if (!_canMove) return false;
            if (_timeProvider.Time - _lastTimeMoved < _timeGap) return false;
            if (direction.y < 0) return false;

            var newX = _position.x + (direction.y * _gridMovementFactor);
            if (newX < _limitsX.Item1 || newX > _limitsX.Item2 ) return false;

            var newZ = _position.z + (-direction.x * _gridMovementFactor);
            if (newZ < _limitsZ.Item1 || newZ > _limitsZ.Item2) return false;
            
            return true;
        }
        public void ChangeSpeedAccordingToWeather(ETrafficWeather weather)
        {
            _speedFactorRelatedToWeather = weather switch
            {
                ETrafficWeather.clouded or ETrafficWeather.foggy => 0.8f,
                ETrafficWeather.light_rain => 0.6f,
                ETrafficWeather.heavy_rain => 0.4f,
                _ => 1,
            };
        }

        public void Reset()
        {
            _canMove = false;
            _position = _initialPos;
            OnPositionChanged?.Invoke(0, _position);
            SetActive(false);
        }
        public void Dispose()
        {
            OnActiveChanged = null;
            OnPositionChanged = null;
        }
    }
}
