using System;
using UnityEngine;

namespace NewFrogger.Vehicle.Presentation
{
    using Vehicle.Domain;

    public class VehicleObject : MonoBehaviour
    {
        public event Action<VehicleModel> OnLimitReached;

        private float m_zLimit;
        private Vector3 m_initialPos;
        private VehicleModel m_model;

        public void Initialize(VehicleModel model, float zLimit)
        {
            m_initialPos = transform.position;
            m_zLimit = zLimit;
            SetModel(model);
        }

        public void SetModel(VehicleModel model)
        {
            if (m_model != null)
            {
                m_model.OnActiveChanged -= HandleOnActiveChanged;
            }

            m_model = model;

            if (m_model != null)
            {
                m_model.OnActiveChanged += HandleOnActiveChanged;
                gameObject.SetActive(m_model.Active);
            }
        }

        private void Update()
        {
            if (m_model == null || !m_model.Active) return;

            transform.Translate(Vector3.forward * m_model.Speed * Time.deltaTime);

            if (transform.position.z > m_zLimit)
            {
                OnLimitReached?.Invoke(m_model);
            }
        }

        private void HandleOnActiveChanged(bool value)
        {
            gameObject.SetActive(value);
            
            if (!value)
            {
                transform.position = m_initialPos;
            }
        }

        private void OnDestroy()
        {
            if (m_model != null)
            {
                m_model.OnActiveChanged -= HandleOnActiveChanged;
            }
        }
    }
}