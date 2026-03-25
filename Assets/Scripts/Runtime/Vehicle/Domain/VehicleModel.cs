using System;

namespace NewFrogger.Vehicle.Domain
{
    public class VehicleModel
    {
        public event Action<bool> OnActiveChanged;
        
        public float Speed { get; private set; }
        public bool Active { get; private set; }
        public bool CanMove { get; private set; }
        
        private readonly float m_referenceSpeed;

        public VehicleModel(float rawSpeed, float referenceSpeed)
        {
            if (rawSpeed < 0) throw new ArgumentException("Vehicle cannot have a negative speed.");
            if (referenceSpeed <= 0) throw new ArgumentException("Reference speed must be greater than zero.");
            
            m_referenceSpeed = referenceSpeed;
            Active = false;
            
            ChangeSpeed(rawSpeed);
        }

        public void ChangeSpeed(float rawSpeed)
        {
            if (rawSpeed < 0) return;
            
            Speed = (rawSpeed / 100f) * m_referenceSpeed;
        }

        public void SetActive(bool value)
        {
            Active = value;
            OnActiveChanged?.Invoke(value);
        }

        public void SetCanMove(bool v)
        {
            CanMove = v;
        }
    }
}