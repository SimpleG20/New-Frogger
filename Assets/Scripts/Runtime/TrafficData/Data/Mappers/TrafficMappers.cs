using NewFrogger.Traffic.Domain.Entities;
using NewFrogger.Traffic.Data.DTO;

namespace NewFrogger.Traffic.Data.Mappers
{
    public static class TrafficMappers
    {
        public static TrafficStatusModel ToDomain(this TrafficStatusDTO dto)
        {
            return new TrafficStatusModel(
                vehicleDensity: dto.vehicleDensity,
                averageSpeed: dto.averageSpeed,
                weather: dto.weather
            );
        }

        public static TrafficPredictModel ToDomain(this TrafficPredictDTO dto)
        {
            return new TrafficPredictModel(
                estimatedTime: dto.estimated_time,
                status: dto.status.ToDomain()
            );
        }

        public static TrafficStatsModel ToDomain(this TrafficStatsDTO dto)
        {
            var current = dto.current_status.ToDomain();
            var predicted = new TrafficPredictModel[dto.predicted_status.Length];
            
            for (int i = 0; i < dto.predicted_status.Length; i++)
            {
                predicted[i] = dto.predicted_status[i].ToDomain();
            }
            
            return new TrafficStatsModel(current, predicted);
        }
    }
}