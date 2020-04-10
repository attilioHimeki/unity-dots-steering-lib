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
        private EntityQuery agentsGroup;

        protected override void OnCreate()
        {
            agentsGroup = GetEntityQuery(ComponentType.ReadOnly<SteeringAgentParameters>(), ComponentType.ReadOnly<TargetEntity>());
        }

        protected override void OnDestroy()
        {
        }

        protected override unsafe JobHandle OnUpdate(JobHandle handle)
        {
            ArchetypeChunkComponentType<Translation> translationType = GetArchetypeChunkComponentType<Translation>();
            ArchetypeChunkComponentType<SteeringAgentParameters> steeringAgentParametersType = GetArchetypeChunkComponentType<SteeringAgentParameters>(true);
            ArchetypeChunkComponentType<Velocity> velocityType = GetArchetypeChunkComponentType<Velocity>();
            ArchetypeChunkComponentType<TargetEntity> targetType = GetArchetypeChunkComponentType<TargetEntity>(true);

            CalculateSteeringJob steeringJob = new CalculateSteeringJob()
            {
                deltaTime = Time.DeltaTime,
                translationType = translationType,
                steeringAgentParametersType = steeringAgentParametersType,
                velocityType = velocityType,
                targetType = targetType,
                localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>()
            };

            UpdateEntityTranslationJob translJob = new UpdateEntityTranslationJob()
            {
                deltaTime = Time.DeltaTime,
                translationType = translationType,
                velocityType = velocityType,
            };
            JobHandle calculateSteeringJobHandle = steeringJob.Schedule(agentsGroup);

            JobHandle updateTranslationJobHandle = translJob.Schedule(agentsGroup, calculateSteeringJobHandle);

            return updateTranslationJobHandle;
        }

    }

}