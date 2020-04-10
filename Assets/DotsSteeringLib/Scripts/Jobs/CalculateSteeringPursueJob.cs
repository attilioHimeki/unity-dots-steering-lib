using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Himeki.DOTS.UnitySteeringLib
{
    [BurstCompile]
    struct CalculateSteeringPursueJob : IJobChunk
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

                    //Seek
                    float3 distanceVector = targetPos - translation.Value;
                    float3 direction = math.normalize(distanceVector);
                    float3 desiredVelocity = direction * steeringAgentParams.maxSpeed;
                    float3 steering = desiredVelocity - velocity.Value;

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
    }
}