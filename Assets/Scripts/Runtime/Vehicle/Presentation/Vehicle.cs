using UnityEngine;

public class Vehicle : MonoBehaviour
{
    private VehicleModel _model;
    private float _zLmimit;
    private Vector3 _initialPos;

    public void Initialize(VehicleModel model)
    {
        _initialPos = transform.position;
        _zLmimit = GameplaySettings.Instance.ZLimit;
        SetModel(model);
    }

    public void SetModel(VehicleModel model)
    {
        if (model == null) return;
        _model = model;
        _model.OnActiveChanged += HandleOnActiveChanged;

        gameObject.SetActive(_model.Active);
    }

    public void Update()
    {
        if (_model == null) return;
        if (!_model.Active) return;
        
        transform.Translate(Vector3.forward * _model.Speed * Time.deltaTime);

        if (transform.position.z > _zLmimit) TestVehicle.OnVehicleLimitZ(_model);
    }

    private void HandleOnActiveChanged(bool value)
    {
        gameObject.SetActive(value);
        if (value == false) transform.position = _initialPos;
    }
}