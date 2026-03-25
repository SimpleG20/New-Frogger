namespace NewFrogger.Gameplay.Domain
{
    public interface ITrafficLevelSettings
    {
        int MaxLevels { get; }
        float ReferenceSpeed { get; }
        float zVehicleLimit { get; }
    }
}
