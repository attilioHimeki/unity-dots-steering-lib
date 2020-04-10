using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Himeki.DOTS.UnitySteeringLib
{
    [BurstCompile]
    struct UpdateEntityTranslationJob : IJobChunk
    {
        [ReadOnly] public float deltaTime;
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Velocity> velocityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkVelocities = chunk.GetNativeArray(velocityType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var translation = chunkTranslations[i];
                var velocity = chunkVelocities[i];
            
                float3 newPos = translation.Value + velocity.Value * deltaTime;
                chunkTranslations[i] = new Translation { Value = newPos };
            }
        }
    }
}