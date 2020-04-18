using Unity.Mathematics;

namespace Himeki.DOTS.UnitySteeringLib
{
    public static class Arrival
    {

        public static float3 steer(float3 agentPos, float3 targetPos, float agentMaxSpeed, float3 agentVelocity)
        {
            float decelerationFactor = 0.3f; //Todo: Figure out where to set this

            float3 distanceVector = targetPos - agentPos;
            float distance = math.length(distanceVector);

            if (distance > 0.01f)
            {
                float speed = math.min(agentMaxSpeed, distance / decelerationFactor);
                float3 desiredVelocity = distanceVector * speed / distance;
                var steering = desiredVelocity - agentVelocity;
                return steering;
            }

            return float3.zero;
        }
    }
}
