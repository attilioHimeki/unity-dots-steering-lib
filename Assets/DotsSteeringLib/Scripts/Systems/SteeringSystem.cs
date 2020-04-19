using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine.Jobs;

namespace Himeki.DOTS.UnitySteeringLib
{
    [AlwaysSynchronizeSystem]
    public class SteeringSystem : JobComponentSystem
    {
        private EntityQuery agentsQuery;
        private EntityQuery obstaclesQuery;

        protected override void OnCreate()
        {
            agentsQuery = GetEntityQuery(ComponentType.ReadOnly<SteeringAgentParameters>(), ComponentType.ReadOnly<TargetEntity>());
            obstaclesQuery = GetEntityQuery(ComponentType.ReadOnly<Obstacle>(), ComponentType.ReadOnly<Translation>());
        }

        protected override void OnDestroy()
        {
        }

        protected override unsafe JobHandle OnUpdate(JobHandle handle)
        {
            ArchetypeChunkComponentType<Translation> translationType = GetArchetypeChunkComponentType<Translation>();
            ArchetypeChunkComponentType<Rotation> rotationType = GetArchetypeChunkComponentType<Rotation>();
            ArchetypeChunkComponentType<SteeringAgentParameters> steeringAgentParametersType = GetArchetypeChunkComponentType<SteeringAgentParameters>(true);
            ArchetypeChunkComponentType<Velocity> velocityType = GetArchetypeChunkComponentType<Velocity>();
            ArchetypeChunkComponentType<TargetEntity> targetType = GetArchetypeChunkComponentType<TargetEntity>(true);
            ArchetypeChunkComponentType<LocalToWorld> localToWorldType = GetArchetypeChunkComponentType<LocalToWorld>(true);

            ComponentDataFromEntity<LocalToWorld> localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            ComponentDataFromEntity<Velocity> velocityFromEntity = GetComponentDataFromEntity<Velocity>(true);

            NativeArray<Translation> obstaclesTranslations = obstaclesQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            NativeArray<Obstacle> obstacles = obstaclesQuery.ToComponentDataArray<Obstacle>(Allocator.TempJob);

            CalculateSteeringJob steeringJob = new CalculateSteeringJob()
            {
                deltaTime = Time.DeltaTime,
                translationType = translationType,
                steeringAgentParametersType = steeringAgentParametersType,
                velocityType = velocityType,
                targetType = targetType,
                localToWorldFromEntity = localToWorldFromEntity,
                velocityFromEntity = velocityFromEntity,
                obstaclesTranslations = obstaclesTranslations,
                obstacles = obstacles
            };

            UpdateEntityTranslationJob translJob = new UpdateEntityTranslationJob()
            {
                deltaTime = Time.DeltaTime,
                translationType = translationType,
                localToWorldType = localToWorldType,
                rotationType = rotationType,
                velocityType = velocityType,
            };

            JobHandle calculateSteeringJobHandle = steeringJob.Schedule(agentsQuery);
            JobHandle updateTranslationJobHandle = translJob.Schedule(agentsQuery, calculateSteeringJobHandle);

            return updateTranslationJobHandle;
        }

    }

}