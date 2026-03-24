namespace NewFrogger.Gameplay.Domain
{
    public interface ITrafficLevelSettings
    {
        float ReferenceSpeed { get; }
        float ZLimit { get; }
    }
}
