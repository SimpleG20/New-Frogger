
using System;

public class VehicleModel
{
    public event Action<bool> OnActiveChanged;
    public float Speed { get; private set; }
    public bool Active { get; private set; }
    
    public VehicleModel(float speed)
    {
        if (speed < 0) throw new ArgumentException("Vehicle cannot have a negative speed");
        ChangeSpeed(speed);
        Active = false;
    }

    public void ChangeSpeed(float speed)
    {
        if (speed < 0) return;
        Speed = speed / 100 * GameplaySettings.Instance.ReferenceSpeed;
    }

    public void SetActive(bool value)
    {
        Active = value;
        OnActiveChanged?.Invoke(value);
    }
}