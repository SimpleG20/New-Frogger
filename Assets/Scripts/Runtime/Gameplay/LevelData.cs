using NewFrogger.Traffic.Domain.Entities;
using NewFrogger.Vehicle.Domain;
using System.Linq;

public struct LevelData
{
    public int MaxTime { get; private set; }
    public TrafficSettingsPredicted[] TrafficSettings { get; private set; }

    public LevelData(float referencedSpeed, float zLimit, TrafficStatusModel current, params TrafficPredictModel[] predicteds)
    {
        MaxTime = predicteds.Sum(p => p.EstimatedTime);
        TrafficSettings = new TrafficSettingsPredicted[predicteds.Length + 1];
        TrafficSettings[0] = new TrafficSettingsPredicted(
            0, 
            new TrafficSettings(
                current.VehicleDensity, 
                current.AverageSpeed,
                current.Weather,
                referencedSpeed, 
                zLimit
            )
        );

        for (int i = 1; i < predicteds.Length; i++)
        {
            var predicted = predicteds[i - 1];
            var status = predicted.Status;
            TrafficSettings[i] = new TrafficSettingsPredicted(
                predicted.EstimatedTime, 
                new TrafficSettings(
                    status.VehicleDensity, 
                    status.AverageSpeed, 
                    status.Weather,
                    referencedSpeed, 
                    zLimit
                )
            );
        }
    }
}
