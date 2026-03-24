using NewFrogger.Vehicle.Domain;

namespace NewFrogger.Gameplay.Domain
{
    public readonly struct TrafficSettingsPredicted
    {
        public int EstimatedTime { get; }
        public TrafficSettings Settings { get; }
        
        public TrafficSettingsPredicted(int estimatedTime, TrafficSettings settings)
        {
            EstimatedTime = estimatedTime;
            Settings = settings;
        }
    }
}
