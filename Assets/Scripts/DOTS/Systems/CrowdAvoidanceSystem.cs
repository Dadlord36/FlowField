using System.Runtime.InteropServices;
using DOTS.Components;
using DOTS.Components.Tags;
using DOTS.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Systems
{
    /// <summary>
    /// System that handles the avoidance of crowd members from each other.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(VelocityDrivenMovementSystem))]
    [StructLayout(LayoutKind.Auto)]
    [DisableAutoCreation]
    public partial struct CrowdAvoidanceSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<MovementSpeedComponent>();
            state.RequireForUpdate<MovementIsBlockedTag>();
            state.RequireForUpdate<VelocityDrivenTag>();

            _actorEntityBounds.size = new Vector3(3f, 2f, 3f);
        }

        private Bounds _actorEntityBounds;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref readonly MovementParametersComponent movementParameters = ref SystemAPI.GetSingletonRW<MovementParametersComponent>().ValueRO;

            /*Bounds frontViewBounds = default;
            Bounds otherEntityBounds = default;

            foreach (var (localToWorld, firstMovementSpeedComponent, firstEntity) in SystemAPI
                         .Query<RefRO<LocalToWorld>, RefRW<MovementSpeedComponent>>().WithEntityAccess())
            {
                //Make a box that will represent the front view of the entity, so later we can check if other entities are inside it
                // CommandBuilder drawingBuilder = DrawingManager.GetBuilder(true);
                float3 pointInFrontOfEntity = localToWorld.ValueRO.Position + localToWorld.ValueRO.Forward * 2f;
                frontViewBounds.center = pointInFrontOfEntity;
                frontViewBounds.size = new Vector3(3f, 2f, 3f);

                Color intersectionColor = Color.green;
                var intersects = false;

                foreach (var (otherLocalToWorld, secondMovementSpeedComponent, secondEntity) in
                         SystemAPI.Query<RefRO<LocalToWorld>, RefRW<MovementSpeedComponent>>().WithEntityAccess())
                {
                    if (firstEntity == secondEntity)
                    {
                        continue;
                    }

                    // Make bounds for the other entity
                    otherEntityBounds.center = otherLocalToWorld.ValueRO.Position;
                    otherEntityBounds.size = new Vector3(1f, 1f, 1f);

                    intersects = frontViewBounds.Intersects(otherEntityBounds);

                    // drawingBuilder.WireBox(otherEntityBounds.center, otherLocalToWorld.ValueRO.Rotation, otherEntityBounds.size, Color.yellow);

                    if (!intersects) continue;
                    intersectionColor = Color.red;

                    break;
                }

                // drawingBuilder.WireBox(frontViewBounds.center, localToWorld.ValueRO.Rotation, frontViewBounds.size, intersectionColor);
                firstMovementSpeedComponent.ValueRW.speed = intersects ? 0f : movementParameters.maxSpeed;
                // drawingBuilder.DisposeAfter(state.Dependency);
            }
            state.CompleteDependency();*/

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            JobHandle jobHandle = state.Dependency;
            foreach (var (_, locationComponent, entity) in SystemAPI.Query<VelocityDrivenTag, RefRO<LocalToWorld>>().WithEntityAccess())
            {
                _actorEntityBounds.center = locationComponent.ValueRO.Position + locationComponent.ValueRO.Forward * 2f;

                new EntityAvoidanceJob(entity, _actorEntityBounds, ecb.AsParallelWriter()).ScheduleParallel(jobHandle);
            }

            state.CompleteDependency();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        //Custom implementation of Bounds.Intersects
        private static bool Intersects(in Bounds first, in Bounds second)
        {
            if (first.Contains(second.center) || second.Contains(first.center))
            {
                return true;
            }

            float3 firstMin = first.min;
            float3 firstMax = first.max;
            float3 secondMin = second.min;
            float3 secondMax = second.max;

            return firstMin.x <= secondMax.x && firstMax.x >= secondMin.x &&
                   firstMin.y <= secondMax.y && firstMax.y >= secondMin.y &&
                   firstMin.z <= secondMax.z && firstMax.z >= secondMin.z;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}