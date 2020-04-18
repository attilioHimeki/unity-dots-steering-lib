using Unity.Mathematics;

namespace Himeki.DOTS.UnitySteeringLib
{
    public static class Flee
    {
        public static float3 steer(float3 agentPos, float3 targetPos, float agentMaxSpeed, float3 agentVelocity)
        {
            float safeFleeDistance = 20f; //Todo: Figure out where to set this
            float3 distanceVector = targetPos - agentPos;
            float distance = math.length(distanceVector);
            if (distance < safeFleeDistance)
            {
                float3 desiredVelocity = math.normalize(distanceVector) * agentMaxSpeed;
                float3 steering = desiredVelocity - agentVelocity;
                return -steering;
            }

            return -agentVelocity;
        }
    }
}
