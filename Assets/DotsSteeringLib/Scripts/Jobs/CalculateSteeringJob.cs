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
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Velocity> velocityType;
        [ReadOnly] public ArchetypeChunkComponentType<SteeringAgentParameters> steeringAgentParametersType;
        [ReadOnly] public ArchetypeChunkComponentType<TargetEntity> targetType;
        [ReadOnly] public ComponentDataFromEntity<LocalToWorld> localToWorldFromEntity;
        [ReadOnly] [NativeDisableContainerSafetyRestriction] public ComponentDataFromEntity<Velocity> velocityFromEntity;

        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Translation> obstaclesTranslations;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Obstacle> obstacles;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> chunkTranslations = chunk.GetNativeArray(translationType);
            NativeArray<SteeringAgentParameters> chunkSteeringAgentParameters = chunk.GetNativeArray(steeringAgentParametersType);
            NativeArray<Velocity> chunkVelocities = chunk.GetNativeArray(velocityType);
            NativeArray<TargetEntity> chunkTargets = chunk.GetNativeArray(targetType);

            int obstaclesAmount = obstaclesTranslations.Length;
            NativeArray<float3> obstaclesPositions = new NativeArray<float3>(obstaclesAmount, Allocator.Temp);
            NativeArray<float> obstaclesRadii = new NativeArray<float>(obstaclesAmount, Allocator.Temp);

            for (int i = 0; i < obstaclesAmount; i++)
            {
                obstaclesPositions[i] = obstaclesTranslations[i].Value;
                obstaclesRadii[i] = obstacles[i].radius;
            }

            for (var i = 0; i < chunk.Count; i++)
            {
                Translation translation = chunkTranslations[i];
                SteeringAgentParameters steeringAgentParams = chunkSteeringAgentParameters[i];
                TargetEntity target = chunkTargets[i];
                Velocity velocity = chunkVelocities[i];

                if (target.entity != Entity.Null && localToWorldFromEntity.Exists(target.entity))
                {
                    LocalToWorld targetLocalToWorld = localToWorldFromEntity[target.entity];
                    float3 targetPos = targetLocalToWorld.Value.c3.xyz;
                    float3 targetForward = targetLocalToWorld.Forward;
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
                        case SteeringBehaviourId.Evade:
                            steering = Evade.steer(translation.Value, targetPos, steeringAgentParams.maxSpeed, velocity.Value, targetVelocity);
                            break;
                        case SteeringBehaviourId.Follow:
                            steering = Follow.steer(translation.Value, targetPos, targetForward, steeringAgentParams.maxSpeed, velocity.Value);
                            break;
                    }

                    if(steeringAgentParams.avoidObstacles)
                    {
                        steering += Avoidance.steer(translation.Value, steeringAgentParams.radius, obstaclesPositions, obstaclesRadii, velocity.Value); 
                    }

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