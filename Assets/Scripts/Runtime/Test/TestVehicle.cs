using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

using NewFrogger.Traffic.Domain.Entities;
using NewFrogger.Traffic.Domain.Services;
using NewFrogger.Traffic.Data.Repositories;
using NewFrogger.Traffic.Data.Datasources;
using NewFrogger.Gameplay.Data;
using NewFrogger.Vehicle.Domain;
using NewFrogger.Vehicle.Presentation;
using CustomLogger;

namespace NewFrogger.Test
{
    public class TestVehicle : MonoBehaviour
    {
        [Header("Settings")]
        public GameplaySettingsSO settingsSO;
        
        [Header("Scene References")]
        public TrafficSpawner spawner;

        public async void Start()
        {
            try 
            {
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TestVehicle] Initialization failed: {ex.Message}");
            }
        }

        private UniTask InitializeAsync()
        {
            return UniTask.CompletedTask;
            //var stats = await GetAPIStats();
            //var currentStats = stats.CurrentStatus;

            //var trafficSettings = new TrafficSettings(currentStats.VehicleDensity, currentStats.AverageSpeed, settingsSO.ReferenceSpeed, settingsSO.ZLimit);
            //spawner.Initialize(trafficSettings, 10);

            //await UniTask.Delay(3000);
            //Log.log("[TestVehicle] Simulation Started");
            //spawner.StartSpawning();
        }

        private async UniTask<TrafficStatsModel> GetAPIStats()
        {
            var datasource = new ApiTrafficDataSource("https://f98dcad7-664e-44fb-8c6f-79c727a98caf.mock.pstmn.io");
            var repo = new TrafficRepositoryImpl(datasource);
            var useCase = new GetTrafficStatsService(repo);

            return await useCase.call();
        }
    }
}