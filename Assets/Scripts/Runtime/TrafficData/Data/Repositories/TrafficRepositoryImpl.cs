using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using CustomLogger;

namespace NewFrogger.Traffic.Data.Repositories
{
    using Domain.Entities;
    using Domain.Repositories;
    using Data.Datasources;
    
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
                return TrafficStatsModel.FromDTO(dto);
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