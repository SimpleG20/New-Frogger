using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

using CustomLogger;
using Cysharp.Threading.Tasks;

using NewFrogger.Traffic.Domain;
using NewFrogger.Vehicle.Domain;
using NewFrogger.Vehicle.Presentation;

namespace NewFrogger.Traffic.Presentation
{
    [Serializable]
    public class TrafficSpawner
    {
        [SerializeField] private VehicleView _vehiclePrefab;
        [SerializeField] private Transform _vehicleParent;
        [SerializeField] private Transform[] _lanes;

        public event Action<VehicleView> OnSpawn;
        public event Action<VehicleView> OnRelease;

        private int _lastLaneChoosen;
        private bool _paused;

        private TrafficSettings _currentSettings;
        private CancellationTokenSource _spawnCTS;

        private ObjectPool<VehicleView> _pool;

        public void Initialize(int vehiclesAmount)
        {
            _pool = new ObjectPool<VehicleView>(
                CreateVehicle,
                OnGetVehicle,
                OnReleaseVehicle,
                OnDestroyVehicle,
                defaultCapacity: vehiclesAmount
            );
        }
        private VehicleView CreateVehicle()
        {
            var vehicle = UnityEngine.Object.Instantiate(_vehiclePrefab, _vehicleParent);
            var model = new VehicleModel(_currentSettings.AverageSpeed, _currentSettings.ReferencedSpeed);
            vehicle.Initialize(model, _currentSettings.zLimit);

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

            OnSpawn?.Invoke(vehicle);
        }
        private void OnReleaseVehicle(VehicleView vehicle)
        {
            OnRelease?.Invoke(vehicle);
        }
        private void OnDestroyVehicle(VehicleView vehicle)
        {
        }

        public void UpdateTrafficSettings(TrafficSettings settings) => _currentSettings = settings;

        public void StartSpawning(TrafficSettings settings)
        {
            _spawnCTS = new();
            _lastLaneChoosen = -1;
            _currentSettings = settings;
            _ = SpawnRoutine();
        }
        private async UniTask SpawnRoutine()
        {
            try
            {
                while (_spawnCTS != null && !_spawnCTS.Token.IsCancellationRequested)
                {
                    if (_paused)
                    {
                        await UniTask.Yield();
                        continue;
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(_currentSettings.SpawnInterval), cancellationToken: _spawnCTS.Token);

                    _pool.Get();
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

        public void PauseSpawning() => _paused = true;
        public void ResumeSpawning() => _paused = false;
        public void StopSpawning() => _spawnCTS?.Cancel();

        public void Release(VehicleView vehicle) => _pool.Release(vehicle);

        public void Dispose()
        {
            OnSpawn = null;

            _spawnCTS?.Cancel();
            _spawnCTS?.Dispose();

            _pool?.Dispose();
        }
    }
}