using NewFrogger.Traffic.Domain.Enums;
using UnityEngine;

namespace NewFrogger.Vehicle.Domain
{
    public struct TrafficSettings
    {
        public float VehicleDensity;
        public float AverageSpeed;
        public float ReferencedSpeed;
        public float SpawnInterval;
        public float zLimit;
        public ETrafficWeather Weather;

        public TrafficSettings(float vehicleDensity, float averageSpeed, ETrafficWeather weather, float referencedSpeed, float zLimit)
        {
            this.zLimit = zLimit;

            Weather = weather;
            AverageSpeed = averageSpeed;
            ReferencedSpeed = referencedSpeed;
            VehicleDensity = vehicleDensity;
            
            SpawnInterval = 1f / Mathf.Max(VehicleDensity, 0.05f);
        }
    }
}