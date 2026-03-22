using UnityEngine;

public struct TrafficSettings
{
    public float VehicleDensity;
    public float AverageSpeed;
    public float ReferencedSpeed;
    public float SpawnInterval;
    public float zLimit;

    public TrafficSettings(float vehicleDensity, float averageSpeed, float referencedSpeed, float zLimit)
    {
        this.zLimit = zLimit;

        VehicleDensity = vehicleDensity;
        AverageSpeed = averageSpeed;
        ReferencedSpeed = referencedSpeed;
        
        SpawnInterval = 1f / Mathf.Max(VehicleDensity, 0.05f);
    }
}