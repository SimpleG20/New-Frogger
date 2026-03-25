using System;

namespace NewFrogger.Traffic.Domain.Entities
{
    public class TrafficStatsModel
    {
        public TrafficStatusModel CurrentStatus { get; private set; }
        public TrafficPredictModel[] PredictedStatus { get; private set; }

        public TrafficStatsModel(TrafficStatusModel current, params TrafficPredictModel[] predicteds)
        {
            CurrentStatus = current ?? throw new ArgumentNullException(nameof(current));
            PredictedStatus = predicteds;
        }

        public static TrafficStatsModel Default()
        {
            return new TrafficStatsModel
            (
                current: TrafficStatusModel.Default(),
                predicteds: Array.Empty<TrafficPredictModel>()
            );
        }
    }
}
