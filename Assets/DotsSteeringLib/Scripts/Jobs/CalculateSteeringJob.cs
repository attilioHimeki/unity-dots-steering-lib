using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Himeki.DOTS.UnitySteeringLib
{
    [BurstCompile]
    struct CalculateSteeringJob : IJobChunk
    {
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Velocity> velocityType;
        [ReadOnly] public ArchetypeChunkComponentType<SteeringAgentParameters> steeringAgentParametersType;
        [ReadOnly] public ArchetypeChunkComponentType<TargetEntity> targetType;
        [ReadOnly] public ComponentDataFromEntity<LocalToWorld> localToWorldFromEntity;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> chunkTranslations = chunk.GetNativeArray(translationType);
            NativeArray<SteeringAgentParameters> chunkSteeringAgentParameters = chunk.GetNativeArray(steeringAgentParametersType);
            NativeArray<Velocity> chunkVelocities = chunk.GetNativeArray(velocityType);
            NativeArray<TargetEntity> chunkTargets = chunk.GetNativeArray(targetType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var translation = chunkTranslations[i];
                var steeringAgentParams = chunkSteeringAgentParameters[i];
                var velocity = chunkVelocities[i];
                var target = chunkTargets[i];

                if (target.entity != Entity.Null && localToWorldFromEntity.Exists(target.entity))
                {
                    var targetPos = localToWorldFromEntity[target.entity].Value.c3.xyz;

                    float3 steering = steerSeek(translation.Value, targetPos, steeringAgentParams.maxSpeed, velocity.Value);
                    //Apply steering
                    if (math.length(steering) > steeringAgentParams.maxForce)
                    {
                        steering = math.normalize(steering) * steeringAgentParams.maxForce;
                    }

                    steering /= steeringAgentParams.mass;

                    float3 newVelocity = velocity.Value + steering;
                    if (math.length(newVelocity) > steeringAgentParams.maxSpeed)
                    {
                        newVelocity = math.normalize(newVelocity) * steeringAgentParams.maxSpeed;
                    }

                    chunkVelocities[i] = new Velocity { Value = newVelocity };
                }
            }
        }

        private float3 steerSeek(float3 agentPos, float3 targetPos, float agentMaxSpeed, float3 agentVelocity)
        { 
            float3 distanceVector = targetPos - agentPos;
            float3 direction = math.normalize(distanceVector);
            float3 desiredVelocity = direction * agentMaxSpeed;
            float3 steering = desiredVelocity - agentVelocity;

            return steering;
        }

    }
}