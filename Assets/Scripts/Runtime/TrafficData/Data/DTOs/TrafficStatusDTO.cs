using System;

namespace NewFrogger.Traffic.Data.DTO
{
    [Serializable]
    public class TrafficStatusDTO
    {
        public float vehicleDensity;
        public float averageSpeed;
        public string weather;
    }
}
