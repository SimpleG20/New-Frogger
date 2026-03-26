using System;

namespace NewFrogger.Traffic.Domain.Entities
{
    using Domain.Enums;

    public class TrafficStatusModel
    {
        public float VehicleDensity { get; private set; }
        public float AverageSpeed { get; private set; }
        public ETrafficWeather Weather { get; private set; }

        public TrafficStatusModel(float vehicleDensity, float averageSpeed, string weather)
        {
            if (vehicleDensity < 0 || vehicleDensity > 1) throw new ArgumentOutOfRangeException(nameof(vehicleDensity));
            if (averageSpeed < 0 || averageSpeed > 100) throw new ArgumentOutOfRangeException(nameof(averageSpeed));

            VehicleDensity = vehicleDensity;
            AverageSpeed = averageSpeed;
            Weather = Enum.TryParse(weather, true, out ETrafficWeather value) ? value : ETrafficWeather.sunny;
        }

        public static TrafficStatusModel Default()
        {
            return new TrafficStatusModel
            (
                vehicleDensity: default,
                averageSpeed: default,
                weather: "sunny"
            );
        }
    }
}
