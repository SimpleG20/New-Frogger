using NewFrogger.Traffic.Domain.Enums;

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
            
            float clampedDensity = vehicleDensity < 0.05f ? 0.05f : vehicleDensity;
            SpawnInterval = 1f / clampedDensity;
        }
    }
}