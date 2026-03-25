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

        public void Initialize(PlayerModel player)
        {
            gameObject.SetActive(player.Active);

            player.OnPositionChanged += HandleOnMovement;
            player.OnActiveChanged += HandleOnActiveChanged;
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
            OnVehicleHit = null;
            OnPlayerFinishedMovement = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_vehicleTag))
            {
                OnVehicleHit?.Invoke();
            }
        }
    }
}
