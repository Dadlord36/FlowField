using System;
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
using UnityEngine.Rendering;

namespace DOTS.Systems
{
    [DisableAutoCreation]
    public partial class MainSpawnSystem : SystemBase
    {
        [BurstCompile]
        private struct SpawnJob : IJobParallelFor
        {
            public Entity prototype;
            public GridParameters gridParameters;
            public EntityCommandBuffer.ParallelWriter ecb;
            public float speed;

            public void Execute(int index)
            {
                // Clone the Prototype entity to create a new entity.
                Entity e = ecb.Instantiate(index, prototype);
                // Prototype has all correct components up front, can use SetComponent to
                // set values unique to the newly created entity, such as the transform.
                ecb.SetComponent(index, e, new LocalToWorld { Value = ComputeTransform(index) });
                ecb.SetComponent(index, e, CellIndexComponent.Invalid);
                ecb.AddSharedComponent(index, e, new SpeedComponent(speed));
            }

            private float4x4 ComputeTransform(int index)
            {
                float3 position = GridCalculations.GetCellCenterAtReversed(gridParameters, index);
                return float4x4.Translate(position);
            }
        }

        private RenderMeshArray _renderMeshArrayRecord;

        protected override void OnStartRunning()
        {
            foreach (var (spawnEntityParametersTag, renderMeshArray) in SystemAPI
                         .Query<RefRO<SpawnEntityParametersTag>, RenderMeshArray>())
            {
                _renderMeshArrayRecord = renderMeshArray;
            }
        }

        protected override void OnUpdate()
        {
            EntityManager entityManager = EntityManager;

            Entity prototype = CreatePrototype(entityManager, _renderMeshArrayRecord);
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            RefRW<EntitySpawnParametersComponent> entitySpawnParameters = SystemAPI.GetSingletonRW<EntitySpawnParametersComponent>();
            RefRW<GridParametersComponent> gridParametersComponent = SystemAPI.GetSingletonRW<GridParametersComponent>();

            // Spawn most of the entities in a Burst job by cloning a pre-created prototype entity,
            // which can be either a Prefab or an entity created at run time like in this sample.
            // This is the fastest and most efficient way to create entities at run time.
            int entityCount = entitySpawnParameters.ValueRO.entityCount;
            JobHandle spawnJob = new SpawnJob
            {
                prototype = prototype,
                speed = entitySpawnParameters.ValueRO.speed,
                gridParameters = gridParametersComponent.ValueRO.gridParameters,
                ecb = ecb.AsParallelWriter(),
            }.Schedule(entityCount, entityCount / Environment.ProcessorCount);

            spawnJob.Complete();
            CompleteDependency();
            
            ecb.Playback(entityManager);
            ecb.Dispose();
        }

        private Entity CreatePrototype(EntityManager entityManager, in RenderMeshArray renderMeshArray)
        {
            // Create a RenderMeshDescription using the convenience constructor
            // with named parameters.
            var desc = new RenderMeshDescription(
                shadowCastingMode: ShadowCastingMode.Off,
                receiveShadows: false);

            EntityArchetype archetype = entityManager.CreateArchetype(typeof(FlowDrivenTag), typeof(VelocityDrivenTag),
                typeof(VelocityComponent), typeof(FlowFieldVelocityComponent), typeof(CellIndexComponent), typeof(LocalToWorld));
            // Create empty base entity
            Entity prototype = entityManager.CreateEntity(archetype);

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
    }
}