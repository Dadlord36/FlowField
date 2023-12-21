using System.Runtime.InteropServices;
using DOTS.Components;
using FunctionalLibraries;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Systems.InitializationPhase
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(FlowFieldToGridCellRelationSystem))]
    public partial struct RotationSystem : ISystem
    {
        [StructLayout(LayoutKind.Auto)]
        [BurstCompile]
        private partial struct RotationJob : IJobEntity
        {
            [ReadOnly] private readonly float _deltaTime;

            public RotationJob(float deltaTime) : this()
            {
                _deltaTime = deltaTime;
            }

            private void Execute(RefRW<RotationAlongSurfaceComponent> rotationAlongSurfaceComponent,
                RefRW<FlowFieldVelocityComponent> flowFieldVelocityComponent)
            {
                /*rotationAlongSurfaceComponent.ValueRW.angleInDegrees =
                    (rotationAlongSurfaceComponent.ValueRO.angleInDegrees + 30f * _deltaTime) % 360f;*/

                flowFieldVelocityComponent.ValueRW.flowVelocity +=
                    CylinderCalculations.Calculate2DVelocityFromDirectionAngle(rotationAlongSurfaceComponent.ValueRO.angleInDegrees);
            }
        }

        [BurstCompile]
        private partial struct FlowVelocityModificationJob : IJobEntity
        {
            private void Execute(RefRW<FlowFieldVelocityComponent> flowFieldVelocityComponent,
                RefRO<RotationAlongSurfaceComponent> rotationAlongSurfaceComponent)
            {
                flowFieldVelocityComponent.ValueRW.flowVelocity +=
                    CylinderCalculations.Calculate2DVelocityFromDirectionAngle(rotationAlongSurfaceComponent.ValueRO.angleInDegrees);
            }
        }


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RotationAlongSurfaceComponent>();
            state.RequireForUpdate<FlowFieldVelocityComponent>();
        }

        // private static readonly float3 DefaultScale = new(1f, 1f, 1f);

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // state.Dependency = new FlowVelocityModificationJob().ScheduleParallel(state.Dependency);
            state.Dependency = new RotationJob(deltaTime).ScheduleParallel(state.Dependency);
            // state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}