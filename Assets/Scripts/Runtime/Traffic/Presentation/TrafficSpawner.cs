using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

using CustomLogger;
using Cysharp.Threading.Tasks;

using NewFrogger.Vehicle.Domain;
using NewFrogger.Vehicle.Presentation;
using NewFrogger.Traffic.Domain;

namespace NewFrogger.Traffic.Presentation
{
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

        private VehicleView CreateVehicle()
        {
            var vehicle = Instantiate(_vehiclePrefab, _vehicleParent);
            var model = new VehicleModel(_currentSettings.AverageSpeed, _currentSettings.ReferencedSpeed);
            vehicle.Initialize(model, _currentSettings.zLimit);
            vehicle.OnLimitReached += HandleOnLimitReached;

            return vehicle;
        }

        private void OnGetVehicle(VehicleView vehicle)
        {
            int lane = UnityEngine.Random.Range(0, _lanes.Length);
            int trys = 10;
            while (lane == _lastLaneChoosen && trys > 0)
            {
                lane = UnityEngine.Random.Range(0, _lanes.Length);
                trys--;
            }

            _lastLaneChoosen = lane;
            vehicle.SetPosition(_lanes[lane].position);

            var model = vehicle.GetModel();
            model.SetActive(true);
            _activeVehicles[model] = vehicle;
        }

        private void OnReleaseVehicle(VehicleView vehicle)
        {
            var model = vehicle.GetModel();
            model.SetActive(false);
            _activeVehicles.Remove(model);
        }

        private void OnDestroyVehicle(VehicleView vehicle)
        {
            Destroy(vehicle.gameObject);
        }

        private void HandleOnLimitReached(VehicleModel model)
        {
            _pool.Release(_activeVehicles[model]);
        }

        public void HandleStart(TrafficSettings settings, int vehiclesAmount)
        {
            _spawnCTS = new();
            _activeVehicles = new();

            _lastLaneChoosen = -1;

            _pool = new ObjectPool<VehicleView>(
                CreateVehicle,
                OnGetVehicle,
                OnReleaseVehicle,
                OnDestroyVehicle,
                defaultCapacity: vehiclesAmount
            );
            UpdateTrafficSettings(settings);

            _ = SpawnRoutine();
        }

        private async UniTask SpawnRoutine()
        {
            try
            {
                while (_spawnCTS != null && !_spawnCTS.Token.IsCancellationRequested)
                {
                    _spawnCTS?.Cancel();
                    _spawnCTS = new CancellationTokenSource();

                    await UniTask.Delay(TimeSpan.FromSeconds(_currentSettings.SpawnInterval), cancellationToken: _spawnCTS.Token);

                    var vehicle = _pool.Get();
                    OnSpawn?.Invoke(vehicle);
                }
            }
            catch (OperationCanceledException)
            {
                Log.log("SpawnRoutine cancelled");
            }
            catch (Exception e)
            {
                Log.log(e);
            }
        }

        public void StopSpawning()
        {
            _spawnCTS?.Cancel();
        }

        public void HideVehicles()
        {
            if (_activeVehicles == null || _activeVehicles.Count == 0) return;

            var vehicles = new List<VehicleView>(_activeVehicles.Values);

            foreach(var v in vehicles)
            {
                _pool.Release(v);
            }
        }

        public void UpdateTrafficSettings(TrafficSettings settings)
        {
            _currentSettings = settings;

            foreach (var model in _activeVehicles.Keys)
            {
                model.ChangeSpeed(settings.AverageSpeed);
            }
        }

        public void OnDestroy()
        {
            _spawnCTS?.Cancel();
            _spawnCTS?.Dispose();

            if (_activeVehicles != null)
            {
                foreach (var v in _activeVehicles)
                {
                    v.Value.OnLimitReached -= HandleOnLimitReached;
                }
            }
        }
    }
}