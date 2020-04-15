using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

namespace Himeki.DOTS.UnitySteeringLib
{
    [BurstCompile]
    struct CalculateSteeringJob : IJobChunk
    {
        [ReadOnly] public float deltaTime;
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Velocity> velocityType;
        [ReadOnly] public ArchetypeChunkComponentType<SteeringAgentParameters> steeringAgentParametersType;
        [ReadOnly] public ArchetypeChunkComponentType<TargetEntity> targetType;
        [ReadOnly] public ComponentDataFromEntity<LocalToWorld> localToWorldFromEntity;
        [NativeDisableContainerSafetyRestriction] [ReadOnly] public ComponentDataFromEntity<Velocity> velocityFromEntity;

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
                var target = chunkTargets[i];
                var velocity = chunkVelocities[i];

                if (target.entity != Entity.Null && localToWorldFromEntity.Exists(target.entity))
                {
                    float3 targetPos = localToWorldFromEntity[target.entity].Value.c3.xyz;
                    float3 targetVelocity = velocityFromEntity[target.entity].Value;

                    float3 steering = float3.zero;
                    switch(steeringAgentParams.behaviour)
                    {
                        case SteeringBehaviourId.Idle:
                            steering = Idle.steer();
                            break;
                        case SteeringBehaviourId.Seek:
                            steering = Seek.steer(translation.Value, targetPos, steeringAgentParams.maxSpeed, velocity.Value);
                            break;
                        case SteeringBehaviourId.Arrival:
                            steering = Arrival.steer(translation.Value, targetPos, steeringAgentParams.maxSpeed, velocity.Value);
                            break;
                        case SteeringBehaviourId.Pursue:
                            steering = Pursue.steer(translation.Value, targetPos, steeringAgentParams.maxSpeed, velocity.Value, targetVelocity);
                            break;
                        case SteeringBehaviourId.Flee:
                            steering = Flee.steer(translation.Value, targetPos, steeringAgentParams.maxSpeed, velocity.Value);
                            break;
                    }
                    //Apply steering
                    if (math.length(steering) > steeringAgentParams.maxForce)
                    {
                        steering = math.normalize(steering) * steeringAgentParams.maxForce;
                    }

                    float3 acceleration = steering / steeringAgentParams.mass;

                    float3 newVelocity = velocity.Value + acceleration * deltaTime;
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