using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

using NewFrogger.Vehicle.Domain;
using NewFrogger.Vehicle.Presentation;
using CustomLogger;

public class TrafficSpawner : MonoBehaviour
{
    [SerializeField] private VehicleView _vehiclePrefab;
    [SerializeField] private Transform _vehicleParent;
    [SerializeField] private Transform[] _lanes;

    public event Action<VehicleView> OnSpawn;

    private int _lastLaneChoosen;
    private TrafficSettings _currentSettings;
    private CancellationTokenSource _spawnCTS;

    private ObjectPool<VehicleView> _pool;
    private Dictionary<VehicleModel, VehicleView> _activeVehicles;


    public void Initialize(TrafficSettings settings, int vehiclesAmount)
    {
        _activeVehicles = new();
        _pool = new ObjectPool<VehicleView>(CreateVehicle, defaultCapacity: vehiclesAmount);
        UpdateTrafficSettings(settings);
    }

    private VehicleView CreateVehicle()
    {
        var vehicle = Instantiate(_vehiclePrefab, _vehicleParent);
        var model = new VehicleModel(_currentSettings.AverageSpeed, _currentSettings.ReferencedSpeed);
        vehicle.Initialize(model, _currentSettings.zLimit);
        vehicle.OnLimitReached += HandleOnLimitReached;

        return vehicle;
    }

    private void HandleOnLimitReached(VehicleModel model)
    {
        model.SetActive(false);
        _pool.Release(_activeVehicles[model]);
        _activeVehicles.Remove(model);
    }

    public void StartSpawnning()
    {
        _ = SpawnRoutine();
    }
    private async UniTaskVoid SpawnRoutine() 
    {
        try
        {
            _spawnCTS?.Cancel();
            _spawnCTS = new CancellationTokenSource();

            await UniTask.Delay(TimeSpan.FromSeconds(_currentSettings.SpawnInterval), cancellationToken: _spawnCTS.Token);

            var vehicle = _pool.Get();
            int lane = UnityEngine.Random.Range(0, _lanes.Length);
            int trys = 10;
            while (lane == _lastLaneChoosen || trys > 0)
            {
                lane = UnityEngine.Random.Range(0, _lastLaneChoosen);
                trys--;
            }

            _lastLaneChoosen = lane;
            vehicle.SetPosition(_lanes[lane].position);

            var model = vehicle.GetModel();
            _activeVehicles[model] = vehicle;

            model.SetActive(true);

            OnSpawn?.Invoke(vehicle);
        }
        catch (Exception e)
        {
            Log.log(e);
        }
    }
    
    [ContextMenu("Stop")]
    public void StopSpawnning()
    {
        _spawnCTS?.Cancel();
    }

    public void UpdateTrafficSettings(TrafficSettings settings) 
    {
        _currentSettings = settings;

        foreach(var model in _activeVehicles.Keys)
        {
            model.ChangeSpeed(settings.AverageSpeed);
        }
    }

    public void OnDestroy()
    {
        foreach(var v in _activeVehicles)
        {
            v.Value.OnLimitReached -= HandleOnLimitReached;
        }
    }
}