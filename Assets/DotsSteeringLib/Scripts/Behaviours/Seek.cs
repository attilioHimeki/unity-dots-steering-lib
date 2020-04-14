using Unity.Mathematics;

public static class Seek
{
    public static float3 steer(float3 agentPos, float3 targetPos, float agentMaxSpeed, float3 agentVelocity)
    {
        float3 distanceVector = targetPos - agentPos;
        float3 direction = math.normalize(distanceVector);
        float3 desiredVelocity = direction * agentMaxSpeed;
        float3 steering = desiredVelocity - agentVelocity;

        return steering;
    }
}
