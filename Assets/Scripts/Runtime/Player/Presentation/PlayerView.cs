using System;
using UnityEngine;
using NewFrogger.Player.Domain;

namespace NewFrogger.Player.Presentation
{
    public class PlayerView : MonoBehaviour
    {
        public event Action OnVehicleHit;
        public event Action OnPlayerFinishedMovement;

        [SerializeField] private string _vehicleTag = "Vehicle";

        private PlayerModel _player;

        public void Initialize(PlayerModel player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            gameObject.SetActive(player.Active);

            _player.OnPositionChanged += HandleOnMovement;
            _player.OnActiveChanged += HandleOnActiveChanged;
        }

        private void HandleOnActiveChanged(bool value)
        {
            gameObject.SetActive(value);
        }
        
        private void HandleOnMovement(float time, Vector3 newPos)
        {
            LeanTween.cancel(gameObject);
            LeanTween.move(gameObject, newPos, time)
                .setOnComplete(() => {
                    LeanTween.cancel(gameObject);
                    OnPlayerFinishedMovement?.Invoke();
                });
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnPositionChanged -= HandleOnMovement;
                _player.OnActiveChanged -= HandleOnActiveChanged;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(_vehicleTag))
            {
                OnVehicleHit?.Invoke();
            }
        }
    }
}
