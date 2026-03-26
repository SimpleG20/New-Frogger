using NewFrogger.Core.Data;
using NewFrogger.Core.Domain;
using NewFrogger.Traffic.Data.Datasources;
using NewFrogger.Traffic.Data.Repositories;
using NewFrogger.Traffic.Domain.Repositories;
using NewFrogger.Traffic.Domain.Services;

namespace NewFrogger.Gameplay.Composition
{
    public class GameplayCompositionRoot
    {
        private readonly string _apiBaseUrl;
        private ITrafficDataSource _dataSource;
        private ITrafficRepository _repository;
        private IGetTrafficStatsService _trafficStatsService;
        private ITimeProvider _timeProvider;

        public GameplayCompositionRoot(string apiBaseUrl)
        {
            _apiBaseUrl = apiBaseUrl ?? throw new System.ArgumentNullException(nameof(apiBaseUrl));
        }

        public ITrafficDataSource GetTrafficDataSource()
        {
            _dataSource ??= new ApiTrafficDataSource(_apiBaseUrl);
            return _dataSource;
        }

        public ITrafficRepository GetTrafficRepository()
        {
            _repository ??= new TrafficRepositoryImpl(GetTrafficDataSource());
            return _repository;
        }

        public IGetTrafficStatsService GetTrafficStatsService()
        {
            _trafficStatsService ??= new GetTrafficStatsService(GetTrafficRepository());
            return _trafficStatsService;
        }

        public ITimeProvider GetTimeProvider()
        {
            _timeProvider ??= new UnityTimeProvider();
            return _timeProvider;
        }

        public void Dispose()
        {
            _dataSource = null;
            _repository = null;
            _trafficStatsService = null;
            _timeProvider = null;
        }
    }
}