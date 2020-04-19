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
        public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkComponentType<Velocity> velocityType;
        [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> localToWorldType;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> chunkTranslations = chunk.GetNativeArray(translationType);
            NativeArray<Velocity> chunkVelocities = chunk.GetNativeArray(velocityType);
            NativeArray<Rotation> chunkRotations = chunk.GetNativeArray(rotationType);
            NativeArray<LocalToWorld> localToWorlds = chunk.GetNativeArray(localToWorldType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Velocity velocity = chunkVelocities[i];
                float speedSq = math.lengthsq(velocity.Value);

                if(speedSq > 0.01f)
                {
                    Translation translation = chunkTranslations[i];
                    LocalToWorld localToWorld = localToWorlds[i];
                    float3 newPos = translation.Value + velocity.Value * deltaTime;
                    chunkTranslations[i] = new Translation { Value = newPos };

                    quaternion newRotation = quaternion.LookRotation(velocity.Value, localToWorld.Up);
                    chunkRotations[i] = new Rotation { Value = newRotation };
                }
            }
        }
    }
}