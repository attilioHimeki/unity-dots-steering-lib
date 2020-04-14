using Unity.Mathematics;

public static class Pursue
{

    public static float3 steer(float3 agentPos, float3 targetPos, float agentMaxSpeed, float3 agentVelocity, float3 targetVelocity)
    {
        float3 distanceVector = targetPos - agentPos;
        float distance = math.length(distanceVector);

        float3 agentMovDirection = math.normalize(agentVelocity);
        float3 targetMovDirection = math.normalize(targetVelocity);

        float3 relativeHeading = math.dot(agentMovDirection, targetMovDirection);
        //Todo: Handle case where target and agent are facing each other

        float3 anticipationMultiplier = distance / agentMaxSpeed;
        float3 pursueTargetPos = targetPos + targetVelocity * anticipationMultiplier;

        float3 pursueTargetDistanceVector = pursueTargetPos - agentPos;
        float3 desiredVelocity = math.normalize(pursueTargetDistanceVector) * agentMaxSpeed;

        float3 steering = desiredVelocity - agentVelocity;

        return steering;
    }

}
