using NewFrogger.Vehicle.Domain;

public struct TrafficSettingsPredicted
{
    public int EstimatedTime { get; private set; }
    public TrafficSettings Settings { get; private set; }
    public TrafficSettingsPredicted(int estimatedTime, TrafficSettings settings)
    {
        EstimatedTime = estimatedTime;
        Settings = settings;
    }
}
