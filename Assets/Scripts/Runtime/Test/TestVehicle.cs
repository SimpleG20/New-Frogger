using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

using NewFrogger.Traffic.Domain.Entities;
using NewFrogger.Traffic.Domain.Services;
using NewFrogger.Traffic.Data.Repositories;
using NewFrogger.Traffic.Data.Datasources;
using NewFrogger.Gameplay.Data;
using NewFrogger.Vehicle.Domain;
using NewFrogger.Gameplay.Domain;
using NewFrogger.Vehicle.Presentation;

namespace NewFrogger.Test
{
    public class TestVehicle : MonoBehaviour
    {
        [Header("Settings")]
        public GameplaySettingsSO settingsSO;
        
        [Header("Scene References")]
        public VehicleObject vehicleView;
        
        private VehicleModel m_vehicleModel;
        private GameplaySettings m_gameplaySettings;

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

        private async UniTask InitializeAsync()
        {
            m_gameplaySettings = new GameplaySettings(settingsSO);

            var stats = await GetAPIStats();

            m_vehicleModel = new VehicleModel(
                stats.CurrentStatus.AverageSpeed, 
                m_gameplaySettings.ReferenceSpeed
            );

            vehicleView.Initialize(m_vehicleModel, m_gameplaySettings.ZLimit);
            vehicleView.OnLimitReached += HandleOnVehicleLimitReached;

            await UniTask.Delay(3000);
            Debug.Log("[TestVehicle] Simulation Started");
            m_vehicleModel.SetActive(true);
        }

        private async UniTask<TrafficStatsModel> GetAPIStats()
        {
            var datasource = new ApiTrafficDataSource("https://f98dcad7-664e-44fb-8c6f-79c727a98caf.mock.pstmn.io");
            var repo = new TrafficRepositoryImpl(datasource);
            var useCase = new GetTrafficStatsService(repo);

            return await useCase.call();
        }

        private void HandleOnVehicleLimitReached(VehicleModel model)
        {
            model.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (vehicleView != null)
            {
                vehicleView.OnLimitReached -= HandleOnVehicleLimitReached;
            }
        }
    }
}