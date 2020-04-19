using Unity.Mathematics;

namespace Himeki.DOTS.UnitySteeringLib
{
    public static class Follow
    {
        public static float3 steer(float3 agentPos, float3 targetPos, float3 targetForward, float agentMaxSpeed, float3 agentVelocity)
        {
            float followDistance = 3f;
            float3 offset = -targetForward * followDistance;
            float3 targetFollowPos = targetPos + offset;
            float3 targetFollowPosDistance = targetFollowPos - agentPos;

            if (math.lengthsq(targetFollowPosDistance) > 0.01f)
            {
                float3 desiredVelocity = math.normalize(targetFollowPosDistance) * agentMaxSpeed;
                float3 steering = desiredVelocity - agentVelocity;
                return steering;
            }

            return -agentVelocity;
        }
    }
}
