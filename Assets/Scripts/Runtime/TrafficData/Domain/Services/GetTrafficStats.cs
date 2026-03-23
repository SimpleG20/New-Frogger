using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NewFrogger.Traffic.Domain.Entities;
using NewFrogger.Traffic.Domain.Repositories;

namespace NewFrogger.Traffic.Domain.Services
{
    public class GetTrafficStatsService : BaseServiceAsync<TrafficStatsModel, StatsArg>
    {
        private readonly ITrafficRepository _trafficRepo;

        public GetTrafficStatsService(ITrafficRepository trafficRepo)
        {
            _trafficRepo = trafficRepo ?? throw new ArgumentNullException(nameof(trafficRepo));
        }

        public override async UniTask<TrafficStatsModel> call(StatsArg arg)
        {
            return await _trafficRepo.GetStats(arg.level, arg.ct);
        }
    }

    public struct StatsArg
    {
        public int level;
        public CancellationToken ct;

        public StatsArg(int level, CancellationToken ct)
        {
            this.level = level;
            this.ct = ct;
        }
    }
}