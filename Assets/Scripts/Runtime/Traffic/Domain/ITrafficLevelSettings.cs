namespace NewFrogger.Traffic.Domain
{
    public interface ITrafficLevelSettings
    {
        int MaxLevels { get; }
        float ReferenceSpeed { get; }
        float zVehicleLimit { get; }
    }
}
