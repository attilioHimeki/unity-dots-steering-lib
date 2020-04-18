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
        [ReadOnly] public ArchetypeChunkComponentType<Velocity> velocityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> chunkTranslations = chunk.GetNativeArray(translationType);
            NativeArray<Velocity> chunkVelocities = chunk.GetNativeArray(velocityType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation translation = chunkTranslations[i];
                Velocity velocity = chunkVelocities[i];
            
                float3 newPos = translation.Value + velocity.Value * deltaTime;
                chunkTranslations[i] = new Translation { Value = newPos };
            }
        }
    }
}