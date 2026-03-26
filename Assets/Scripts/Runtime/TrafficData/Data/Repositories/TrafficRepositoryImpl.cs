using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using CustomLogger;

using NewFrogger.Traffic.Domain.Entities;
using NewFrogger.Traffic.Domain.Repositories;
using NewFrogger.Traffic.Data.Datasources;
using NewFrogger.Traffic.Data.Mappers;

namespace NewFrogger.Traffic.Data.Repositories
{
    
    public class TrafficRepositoryImpl : ITrafficRepository
    {
        private readonly ITrafficDataSource _trafficDataSource;

        public TrafficRepositoryImpl(ITrafficDataSource trafficDataSource)
        {
            _trafficDataSource = trafficDataSource ?? throw new ArgumentNullException(nameof(trafficDataSource));
        }

        public async UniTask<TrafficStatsModel> GetStats(int level, CancellationToken ct)
        {
            try
            {
                var dto = await _trafficDataSource.GetStats(level, ct);
                return dto.ToDomain();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.log($"Repository failed to fetch traffic stats: {e}");
                throw;
            }
        }
    }
}