namespace NewFrogger.Player.Domain
{
    public interface IPlayerSettings
    {
        float PlayerDelayFactor { get; }
        float GridMovementFactor { get; }
        float MinX { get; }
        float MaxX { get; }
        float MinZ { get; }
        float MaxZ { get; }
        float xVictory { get; }
    }
}
