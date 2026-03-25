using System;

namespace NewFrogger.Traffic.Domain.Entities
{
    public class TrafficStatsModel
    {
        public TrafficStatusModel CurrentStatus { get; private set; }
        public TrafficPredictModel[] PredictedStatus { get; private set; }

        public TrafficStatsModel(TrafficStatusModel current, params TrafficPredictModel[] predicteds)
        {
            CurrentStatus = current ?? throw new ArgumentNullException("Current Status cannot be null");
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

        public static TrafficStatsModel FromDTO(Data.DTO.TrafficStatsDTO dto)
        {
            var current = TrafficStatusModel.FromDTO(dto.current_status);
            var predicteds = dto.predicted_status.Length > 0 
                                ? new TrafficPredictModel[dto.predicted_status.Length] 
                                : Array.Empty<TrafficPredictModel>();
            
            for (int i = 0; i < dto.predicted_status.Length; i++)
            {
                predicteds[i] = TrafficPredictModel.FromDTO(dto.predicted_status[i]);
            }
            
            return new TrafficStatsModel(
                current,
                predicteds
            );
        }
    }
}
