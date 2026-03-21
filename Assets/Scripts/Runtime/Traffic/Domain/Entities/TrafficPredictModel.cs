using System;

namespace NewFrogger.Traffic.Domain.Entities
{
    public class TrafficPredictModel
    {
        public int EstimatedTime { get; private set; }
        public TrafficStatusModel Status { get; private set;}

        public TrafficPredictModel(int estimatedTime, TrafficStatusModel status)
        {
            if (estimatedTime < 0) throw new ArgumentException("Estimated time cannot by less than zero");

            EstimatedTime = estimatedTime;
            Status = status;
        }

        public static TrafficPredictModel FromDTO(Data.DTO.TrafficPredictDTO dto)
        {
            return new TrafficPredictModel(
                estimatedTime: dto.estimated_time,
                status: TrafficStatusModel.FromDTO(dto.status)
            );
        }
    }
}
