using System.Threading;
using Cysharp.Threading.Tasks;

namespace NewFrogger.Traffic.Data.Datasources
{
    public interface ITrafficDataSource
    {
        UniTask<DTO.TrafficStatsDTO> GetStats(CancellationToken ct = default);
    }
}
