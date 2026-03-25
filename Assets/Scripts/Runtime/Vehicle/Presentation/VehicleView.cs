using System;
using NewFrogger.Vehicle.Domain;
using UnityEngine;

namespace NewFrogger.Vehicle.Presentation
{
    public class VehicleView : MonoBehaviour
    {
        public event Action<VehicleView> OnLimitReached;

        private float _zLimit;
        private Vector3 _initialPos;
        private VehicleModel _model;

        public void Initialize(VehicleModel model, float zLimit)
        {
            _initialPos = transform.position;
            _zLimit = zLimit;
            SetModel(model);
        }

        public VehicleModel GetModel() => _model;

        public void SetModel(VehicleModel model)
        {
            if (_model != null)
            {
                _model.OnActiveChanged -= HandleOnActiveChanged;
            }

            _model = model;

            if (_model != null)
            {
                _model.OnActiveChanged += HandleOnActiveChanged;
                gameObject.SetActive(_model.Active);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_model == null 
                || !_model.Active 
                || !_model.CanMove
            ) return;

            transform.Translate(Vector3.forward * _model.Speed * Time.deltaTime);

            if (transform.position.z > _zLimit) OnLimitReached?.Invoke(this);
        }

        private void HandleOnActiveChanged(bool value)
        {
            gameObject.SetActive(value);
            
            if (!value)
            {
                transform.position = _initialPos;
            }
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.OnActiveChanged -= HandleOnActiveChanged;
            }
        }
    }
}