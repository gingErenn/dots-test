using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

public class TriggerWarningAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    private Entity entityParticle;
    [SerializeField] GameObject particlePrefab;
    private BlobAssetStore blobAsset;
    private World defaultWorld;
    private EntityManager entityManager;
    private Translation objTransform;

    bool _isCompelete = false;

    [SerializeField] string _message = "How dare you!";
    void IConvertGameObjectToEntity.Convert(Entity e, EntityManager dst, GameObjectConversionSystem cs)
    {
        if (!enabled) return;

        dst.AddComponentData(e, new TriggerWarning
        {
            ObjTransform = objTransform,
            EntityManager = entityManager,
            EntityParticle = entityParticle,
            Message = _message
    });
    }

    private void Awake()
    {
        blobAsset = new BlobAssetStore();
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = defaultWorld.EntityManager;
        Debug.Log("c");
        if (particlePrefab != null && entityParticle != null)
        {
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, blobAsset);
            entityParticle = GameObjectConversionUtility.ConvertGameObjectHierarchy(particlePrefab, settings);
            Debug.Log("s");
            _isCompelete = true;
            // spawn x by y grid of Entities
            // InstantiateEntityGrid(xSize, ySize, spacing);
        }
    }
}

public struct TriggerWarning : IComponentData
{
    public FixedString4096Bytes Message;
    public Entity EntityParticle;
    public EntityManager EntityManager;
    public Translation ObjTransform;
    
}

[BurstCompile]
public struct TriggerWarningJob : ICollisionEventsJob
{
    public EntityCommandBuffer ecb;
    [ReadOnly] public ComponentDataFromEntity<TriggerWarning> TriggerWarningData;
    // public ComponentDataFromEntity<RotationSpeed> BallWarningData;
    public BoolComp boolComp;
    public void Execute(CollisionEvent collisionEvent)
    {
        Entity entityA = collisionEvent.EntityA;
        Entity entityB = collisionEvent.EntityB;
        if (entityA != Entity.Null && entityB != Entity.Null)
        {
            //bool isBodyACube = TriggerWarningData.HasComponent(entityA);
            bool isBodyBCube = TriggerWarningData.HasComponent(entityB);

            if (isBodyBCube)
            {
               // Entity entityTemp = ecb.Instantiate(TriggerWarningData[entityB].EntityParticle);
              //  ecb.SetComponent(entityA,new PhysicsVelocity { Linear = Vector3.one * 250});
            }

            //if (isBodyBCube)
            //{
            //    //TriggerWarningData[entityB].EntityManager.GetComponentData<Translation>(entityB).Value}
            //    Debug.Log("B : ");
            //    // ecb.DestroyEntity(entityB);  

            // //   ecb.Destr
            //}
        }
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
partial class TwitterSimulationSystem : SystemBase
{
    public EntityManager EntityManagerX = World.DefaultGameObjectInjectionWorld.EntityManager;
    StepPhysicsWorld _stepPhysicsWorldSystem;
    private EndSimulationEntityCommandBufferSystem endECBSystem;

    protected override void OnCreate()  
    {
        endECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        _stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        RequireForUpdate(GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(TriggerWarning) }
        }));
    }
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    protected override void OnUpdate()
    {
        var job = new TriggerWarningJob
        {
            ecb = endECBSystem.CreateCommandBuffer(),
            TriggerWarningData = GetComponentDataFromEntity<TriggerWarning>(isReadOnly: true),
            
        };
        Dependency = job.Schedule(_stepPhysicsWorldSystem.Simulation, Dependency);
        endECBSystem.AddJobHandleForProducer(Dependency);
    }
}