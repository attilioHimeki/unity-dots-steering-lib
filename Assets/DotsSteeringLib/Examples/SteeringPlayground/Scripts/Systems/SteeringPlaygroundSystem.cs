using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Himeki.DOTS.UnitySteeringLib;

[AlwaysSynchronizeSystem]
public class SteeringPlaygroundSystem : JobComponentSystem
{

    private EntityArchetype playerArchetype;
    private EntityArchetype agentArchetype;

    protected override void OnCreate()
    {
        playerArchetype = EntityManager.CreateArchetype(
            typeof(PlayerControl),
            typeof(Velocity),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderBounds),
            typeof(ChunkWorldRenderBounds)
            );

        agentArchetype = EntityManager.CreateArchetype(
            typeof(SteeringAgentParameters),
            typeof(TargetEntity),
            typeof(Velocity),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderBounds),
            typeof(ChunkWorldRenderBounds)
        );

        CreateAgents(2000);
    }

    public void CreateAgents(int amount)
    {
        float randomSpreadRadius = 40f;

        var playerMat = Resources.Load("PlayerMat", typeof(Material)) as Material;
        var agentsMat = Resources.Load("AgentsMat", typeof(Material)) as Material;
        var entityMesh = Resources.Load("Cube", typeof(Mesh)) as Mesh;

        Entity playerEntity = EntityManager.CreateEntity(playerArchetype);

        EntityManager.SetComponentData(playerEntity, new Scale { Value = 1f });
        EntityManager.SetSharedComponentData(playerEntity, new RenderMesh
        {
            mesh = entityMesh,
            material = playerMat,
            subMesh = 0,
            layer = 0,
            castShadows = ShadowCastingMode.On,
            receiveShadows = true
        });


        NativeArray<Entity> entities = new NativeArray<Entity>(amount, Allocator.Temp);
        EntityManager.CreateEntity(agentArchetype, entities);

        for (int i = 0; i < amount; i++)
        {
            var e = entities[i];
            EntityManager.SetComponentData(e, new Scale { Value = 1f });
            EntityManager.SetComponentData(e, new Translation { Value = new float3(UnityEngine.Random.Range(-randomSpreadRadius, randomSpreadRadius),
                                                                        0f,
                                                                        UnityEngine.Random.Range(-randomSpreadRadius, randomSpreadRadius)) });
            EntityManager.SetComponentData(e, new TargetEntity { entity = playerEntity });
            EntityManager.SetComponentData(e, new SteeringAgentParameters { mass = 1f,
                                                                            radius = 1f,
                                                                            maxForce = 5f,
                                                                            maxSpeed = 5f,
                                                                            behaviour = SteeringBehaviourId.Seek });
            EntityManager.SetSharedComponentData(e, new RenderMesh { mesh = entityMesh,
                                                                    material = agentsMat,
                                                                    subMesh = 0,
                                                                    layer = 0,
                                                                    castShadows = ShadowCastingMode.On,
                                                                    receiveShadows = true });
        }

        entities.Dispose();
    }

    protected override void OnDestroy()
    {
    }

    protected override unsafe JobHandle OnUpdate(JobHandle handle)
    {
        float deltaTime = Time.DeltaTime;
        float inputH = Input.GetAxis("Horizontal");
        float inputV = Input.GetAxis("Vertical");
        float3 inputVector = new float3(inputH, 0f, inputV);

        if (math.length(inputVector) > 0.01f)
        {
            Entities.
                WithAll<PlayerControl>().
                ForEach((Entity e, ref Translation translation) =>
            {
                float3 movementDirection = math.normalize(inputVector);

                float3 newPos = translation.Value + movementDirection * 35f * deltaTime;
                translation = new Translation { Value = newPos };
            }).Run(); //Not worth running on worker thread
        }

        return default;
        
    }

        

}