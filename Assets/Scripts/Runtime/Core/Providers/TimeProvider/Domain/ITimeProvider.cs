namespace NewFrogger.Core.Domain
{
    public interface ITimeProvider
    {
        float Time { get; }
        float DeltaTime { get; }
    }
}
