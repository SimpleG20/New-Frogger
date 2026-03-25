using NewFrogger.Core.Domain;

namespace NewFrogger.Core.Data
{
    public class UnityTimeProvider : ITimeProvider
    {
        public float Time => UnityEngine.Time.time;
        public float DeltaTime => UnityEngine.Time.deltaTime;
    }
}
