using Unity.Mathematics;

public static class Pursue
{

    public static float3 steer(float3 agentPos, float3 targetPos, float agentMaxSpeed, float3 agentVelocity, float3 targetVelocity)
    {
        float anticipationFactor = 2f;

        float3 pursueTargetPos = targetPos + targetVelocity * anticipationFactor;

        float3 pursueTargetDistanceVector = pursueTargetPos - agentPos;
        float3 desiredVelocity = math.normalize(pursueTargetDistanceVector) * agentMaxSpeed;

        float3 steering = desiredVelocity - agentVelocity;

        return steering;
    }

}
