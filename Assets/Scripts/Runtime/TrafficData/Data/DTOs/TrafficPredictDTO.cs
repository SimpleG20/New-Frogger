using System;

namespace NewFrogger.Traffic.Data.DTO
{
    [Serializable]
    public class TrafficPredictDTO
    {
        public int estimated_time;
        public TrafficStatusDTO status;
    }
}
