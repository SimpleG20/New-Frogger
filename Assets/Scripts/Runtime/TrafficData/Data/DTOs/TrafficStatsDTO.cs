using System;

namespace NewFrogger.Traffic.Data.DTO
{
    [Serializable]
    public class TrafficStatsDTO
    {
        public TrafficStatusDTO current_status;
        public TrafficPredictDTO[] predicted_status;
    }
}
