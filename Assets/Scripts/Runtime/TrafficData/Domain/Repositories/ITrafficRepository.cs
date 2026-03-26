using System.Threading;
using Cysharp.Threading.Tasks;
using NewFrogger.Traffic.Domain.Entities;

namespace NewFrogger.Traffic.Domain.Repositories
{
    public interface ITrafficRepository
    {
        UniTask<TrafficStatsModel> GetStats(int level, CancellationToken ct);
    }
}