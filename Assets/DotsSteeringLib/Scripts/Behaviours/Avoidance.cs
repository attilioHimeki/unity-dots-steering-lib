using Unity.Collections;
using Unity.Mathematics;

namespace Himeki.DOTS.UnitySteeringLib
{
    public static class Avoidance
    {
        private readonly static float minObstacleAvoidanceDistance = 3f;
        private readonly static float maxAvoidanceForce = 30f;
        private readonly static float maxAvoidanceBrakingForce = 4f;

        public static float3 steer(float3 agentPos, float agentRadius, NativeArray<float3> obstaclePositions, NativeArray<float> obstacleRadii, float3 agentVelocity)
        {
            if (obstaclePositions.Length > 0)
            {
                float3 closestObstaclePosition = getClosestAvoidanceObstaclePosition(agentPos, agentVelocity, agentRadius, obstaclePositions, obstacleRadii);
                if (math.lengthsq(closestObstaclePosition) > 0.01f)
                {
                    float3 distanceVector = closestObstaclePosition - agentPos;
                    float distance = math.length(distanceVector);

                    float3 agentMovDirection = math.normalize(agentVelocity);

                    float brakingMultiplier = (minObstacleAvoidanceDistance - distance) / minObstacleAvoidanceDistance;
                    float3 braking = agentMovDirection * brakingMultiplier * maxAvoidanceBrakingForce;

                    float3 lateralSteering = (agentMovDirection - math.normalize(distanceVector)) * maxAvoidanceForce;

                    return braking + lateralSteering;
                }
            }

            return float3.zero;
        }

        private static float3 getClosestAvoidanceObstaclePosition(float3 agentPos, float3 agentVelocity, float agentRadius, NativeArray<float3> obstaclePositions, NativeArray<float> obstacleRadii)
        {
            float3 closestPosition = default;
            float closestDistance = float.MaxValue;

            float3 agentMovDirection = math.normalize(agentVelocity);
            for (var i = 0; i < obstaclePositions.Length; i++)
            {
                float3 obstaclePos = obstaclePositions[i];
                float3 distanceVector = obstaclePos - agentPos;
                float3 distanceDirection = math.normalize(distanceVector);
                if (math.dot(distanceDirection, agentMovDirection) > 0.5f)
                {

                    float distance = math.length(distanceVector) - agentRadius - obstacleRadii[i];
                    if (distance <= minObstacleAvoidanceDistance)
                    {
                        if (distance < closestDistance)
                        {
                            closestPosition = obstaclePos;
                            closestDistance = distance;
                        }
                    }
                }

            }

            return closestPosition;
        }

    }
}