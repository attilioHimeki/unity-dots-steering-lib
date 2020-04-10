using Unity.Entities;
using Unity.Mathematics;

namespace Himeki.DOTS.UnitySteeringLib
{
    public struct Velocity : IComponentData
    {
        public float3 Value;
    }
}