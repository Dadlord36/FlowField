using System;
using DOTS.Buffers;
using DOTS.Components;
using DOTS.Components.Tags;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace DOTS.Systems
{
    [DisableAutoCreation]
    public partial class MainSpawnSystem : SystemBase
    {
        [BurstCompile]
        private struct SpawnOnCylinderSurface : IJobParallelFor
        {
            public readonly Entity prototype;
            public readonly GridParameters gridParameters;
            public readonly CylinderParametersComponent cylinderParameters;
            public EntityCommandBuffer.ParallelWriter ecb;
            public readonly float speed;

            public SpawnOnCylinderSurface(Entity prototype, GridParameters gridParameters, CylinderParametersComponent cylinderParameters,
                EntityCommandBuffer.ParallelWriter ecb, float speed)
            {
                this.prototype = prototype;
                this.gridParameters = gridParameters;
                this.cylinderParameters = cylinderParameters;
                this.ecb = ecb;
                this.speed = speed;
            }

            public void Execute(int index)
            {
                // Clone the Prototype entity to create a new entity.
                Entity e = ecb.Instantiate(index, prototype);
                var cylinderPositioningComponent = new CylinderSurfacePositioningComponent();
                // Prototype has all correct components up front, can use SetComponent to
                // set values unique to the newly created entity, such as the transform.

                LocalToWorld spawnTransform = ComputeTransformOnCylinderSurface(index);
                CylinderCalculations.GetHeightAndAngleOnCylinderAt(cylinderParameters.cylinderParameters, spawnTransform.Position,
                    out cylinderPositioningComponent.height, out cylinderPositioningComponent.angle);

                ecb.AddSharedComponent(index, e, cylinderParameters);
                ecb.SetComponent(index, e, spawnTransform);
                ecb.SetComponent(index, e, CellIndexComponent.Empty);
                ecb.SetComponent(index, e, cylinderPositioningComponent);
                ecb.SetComponent(index, e, new MovementSpeedComponent(speed));
                ecb.AddBuffer<BlockingEntityElement>(index, e);
                ecb.SetComponentEnabled<MovementIsBlockedTag>(index, e, false);
            }

            private LocalToWorld ComputeTransformOnCylinderSurface(int index)
            {
                return CylinderCalculations.GetCellCenterAtReversed(gridParameters, cylinderParameters.cylinderParameters, (uint)index, float3.zero);
            }
        }

        private RenderMeshArray _renderMeshArray;

        protected override void OnStartRunning()
        {
            foreach ((var _, RenderMeshArray renderMeshArray) in SystemAPI.Query<RefRO<SpawnEntityParametersTag>, RenderMeshArray>())
            {
                Assert.IsFalse(renderMeshArray.Meshes.Length == 0, "There are no meshes");
                _renderMeshArray = renderMeshArray;
            }
        }

        protected override void OnUpdate()
        {
            EntityManager entityManager = EntityManager;

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
 

            Entity prototype = CreatePrototype(ref entityManager, _renderMeshArray);

            foreach ((var _, Entity entity) in SystemAPI.Query<RefRO<SpawnEntityParametersTag>>().WithEntityAccess())
            {
                RefRW<EntitySpawnParametersComponent> entitySpawnParameters = SystemAPI.GetSingletonRW<EntitySpawnParametersComponent>();
                RefRW<GridParametersComponent> gridParametersComponent = SystemAPI.GetSingletonRW<GridParametersComponent>();
                var cylinderParametersComponent = entityManager.GetSharedComponent<CylinderParametersComponent>(entity);

                int entityCount = entitySpawnParameters.ValueRO.entityCount;
                JobHandle spawnJob = new SpawnOnCylinderSurface(prototype, gridParametersComponent.ValueRO.gridParameters,
                        cylinderParametersComponent,
                        ecb.AsParallelWriter(),
                        entitySpawnParameters.ValueRO.speed)
                    .Schedule(entityCount, entityCount / Environment.ProcessorCount);

                spawnJob.Complete();
            }

            CompleteDependency();

            ecb.Playback(entityManager);
            ecb.Dispose();
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
            return EntityManager.CreateArchetype(typeof(FlowDrivenTag), typeof(VelocityDrivenTag), typeof(VelocityComponent),
                typeof(FlowFieldVelocityComponent), typeof(MovementSpeedComponent), typeof(CellIndexComponent), typeof(LocalToWorld),
                typeof(CylinderSurfacePositioningComponent), typeof(MovementIsBlockedTag), typeof(RotationAlongSurfaceComponent));
        }
    }
}