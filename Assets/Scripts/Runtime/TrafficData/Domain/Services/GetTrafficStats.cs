using System;
using Cysharp.Threading.Tasks;
using NewFrogger.Traffic.Domain.Entities;
using NewFrogger.Traffic.Domain.Repositories;

namespace NewFrogger.Traffic.Domain.Services
{
    public class GetTrafficStatsService : BaseServiceAsync<TrafficStatsModel, NoArg>
    {
        private readonly ITrafficRepository _trafficRepo;

        public GetTrafficStatsService(ITrafficRepository trafficRepo)
        {
            _trafficRepo = trafficRepo ?? throw new ArgumentNullException(nameof(trafficRepo));
        }

        public override async UniTask<TrafficStatsModel> call(NoArg arg = default)
        {
            return await _trafficRepo.GetStats();
        }
    }
}