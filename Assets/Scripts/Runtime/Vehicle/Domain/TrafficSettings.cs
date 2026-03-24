using NewFrogger.Traffic.Domain.Enums;
using UnityEngine;

namespace NewFrogger.Vehicle.Domain
{
    public readonly struct TrafficSettings
    {
        public float VehicleDensity { get; }
        public float AverageSpeed { get; }
        public float ReferencedSpeed { get; }
        public float SpawnInterval { get; }
        public float zLimit { get; }
        public ETrafficWeather Weather { get; }

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