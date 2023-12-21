using DOTS.Components;
using DOTS.Components.Tags;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace DOTS.Systems.Situational
{
    [DisableAutoCreation]
    public partial class MainSpawnSystem : SystemBase
    {
        [BurstCompile]
        [WithAll(typeof(SurfaceWalkerTag))]
        private partial struct SpawnOnCylinderSurface : IJobEntity
        {
            private EntityCommandBuffer.ParallelWriter ecb;
            private readonly float speed;

            public SpawnOnCylinderSurface(EntityCommandBuffer.ParallelWriter ecb, float speed) : this()
            {
                this.ecb = ecb;
                this.speed = speed;
            }

            private void Execute([EntityIndexInQuery] int index, in CylinderParametersComponent cylinderParametersComponent,
                in GridParametersComponent gridParametersComponent, Entity e)
            {
                ref readonly GridParameters gridParameters = ref gridParametersComponent.gridParameters;
                ref readonly CylinderParameters cylinderParameters = ref cylinderParametersComponent.cylinderParameters;

                // Clone the Prototype entity to create a new entity.

                var cylinderPositioningComponent = new CylinderSurfacePositioningComponent();
                // Prototype has all correct components up front, can use SetComponent to
                // set values unique to the newly created entity, such as the transform.

                /*uint2 index2d = GridCalculations.CalculateIndex2DFromIndex1D(gridParameters, (uint)index);
                cylinderPositioningComponent.orbitCoordinate =
                    CylinderCalculations.CalculateOrbitFrom1DIndex(gridParameters, index2d);*/
                var uindex = (uint)index;
                uint2 index2d = GridCalculations.CalculateIndex2DFromIndex1D(gridParameters, uindex);
                cylinderPositioningComponent.surfaceCoordinate = CylinderCalculations.CalculateSurfaceCoordinateFromIndex(gridParameters, index2d);
                
                var spawnTransform = new LocalToWorld
                {
                    Value = CylinderCalculations.SurfaceCoordinateToLocalToWorld(cylinderPositioningComponent.surfaceCoordinate,cylinderParameters)
                };

                ecb.SetComponent(index, e, new CellIndexComponent(index2d));
                ecb.SetComponent(index, e, spawnTransform);
                ecb.SetComponent(index, e, cylinderPositioningComponent);
                ecb.SetComponent(index, e, new MovementSpeedComponent(speed));
                // ecb.AddBuffer<BlockingEntityElement>(index, e);
                ecb.SetComponentEnabled<MovementIsBlockedTag>(index, e, false);
            }
        }

        private RenderMeshArray _renderMeshArray;

        protected override void OnStartRunning()
        {
            foreach ((var _, RenderMeshArray renderMeshArray) in SystemAPI.Query<RefRO<WalkingSurfaceTag>, RenderMeshArray>())
            {
                Assert.IsFalse(renderMeshArray.Meshes.Length == 0, "There are no meshes");
                _renderMeshArray = renderMeshArray;
            }
        }

        protected override void OnUpdate()
        {
            EntityManager entityManager = EntityManager;
            Entity prototype = CreatePrototype(ref entityManager, _renderMeshArray);

            var ecb1 = new EntityCommandBuffer(Allocator.TempJob);

            ref readonly EntitySpawnParametersComponent
                entitySpawnParameters = ref SystemAPI.GetSingletonRW<EntitySpawnParametersComponent>().ValueRO;
            int entitiesCount = entitySpawnParameters.entityCount;
            float movementSpeed = entitySpawnParameters.speed;

            //For each walking surface
            foreach (var (cylinderParametersComponent, flowMapComponent, gridParametersComponent) in SystemAPI
                         .Query<CylinderParametersComponent, FlowMapComponent, GridParametersComponent>().WithAll<WalkingSurfaceTag>())
            {
                NativeArray<Entity> entities = entityManager.Instantiate(prototype, entitiesCount, Allocator.Temp);
                ecb1.AddSharedComponent(entities, cylinderParametersComponent);
                ecb1.AddSharedComponent(entities, gridParametersComponent);
                ecb1.AddSharedComponent(entities, flowMapComponent);
            }

            ecb1.Playback(entityManager);

            var ecb2 = new EntityCommandBuffer(Allocator.TempJob);
            Dependency = new SpawnOnCylinderSurface(ecb2.AsParallelWriter(), movementSpeed).ScheduleParallel(Dependency);

            CompleteDependency();
            //Things to do after completing dependency 

            //1. Should be played only after completion of SpawnOnCylinderSurface job
            ecb2.Playback(entityManager);

            ecb1.Dispose();
            ecb2.Dispose();
            entityManager.DestroyEntity(prototype);
        }

        private Entity CreatePrototype(ref EntityManager entityManager, in RenderMeshArray renderMeshArray)
        {
            // Create a RenderMeshDescription using the convenience constructor
            // with named parameters.
            var desc = new RenderMeshDescription(
                shadowCastingMode: ShadowCastingMode.Off,
                receiveShadows: false);

            // Create empty base entity
            Entity prototype = entityManager.CreateEntity(CreateEntityArchetype());

            // Call AddComponents to populate base entity with the components required
            // by Entities Graphics
            RenderMeshUtility.AddComponents(
                prototype,
                entityManager,
                desc,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

            return prototype;
        }

        private EntityArchetype CreateEntityArchetype()
        {
            return EntityManager.CreateArchetype(typeof(SurfaceWalkerTag), typeof(FlowDrivenTag), typeof(VelocityDrivenTag),
                typeof(VelocityComponent), typeof(FlowFieldVelocityComponent), typeof(MovementSpeedComponent), typeof(CellIndexComponent),
                typeof(LocalToWorld), typeof(CylinderSurfacePositioningComponent), typeof(MovementIsBlockedTag),
                typeof(RotationAlongSurfaceComponent));
        }
    }
}