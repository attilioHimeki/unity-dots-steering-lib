using Unity.Entities;

namespace Himeki.DOTS.UnitySteeringLib
{
    public struct SteeringAgentParameters : IComponentData
    {
        public SteeringBehaviourId behaviour;
        public bool avoidObstacles;
        public float maxSpeed;
        public float maxForce;
        public float mass;
        public float radius;
    }
}