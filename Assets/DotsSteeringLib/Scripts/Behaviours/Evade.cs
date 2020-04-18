using Unity.Mathematics;

namespace Himeki.DOTS.UnitySteeringLib
{
    public static class Evade
    {

        public static float3 steer(float3 agentPos, float3 targetPos, float agentMaxSpeed, float3 agentVelocity, float3 targetVelocity)
        {
            float safeDistance = 10f;

            float3 distanceVector = targetPos - agentPos;
            float distance = math.length(distanceVector);

            if (distance < safeDistance)
            {
                return -Pursue.steer(agentPos, targetPos, agentMaxSpeed, agentVelocity, targetVelocity);
            }
            else
            {
                return -agentVelocity;
            }

        }
    }
}
