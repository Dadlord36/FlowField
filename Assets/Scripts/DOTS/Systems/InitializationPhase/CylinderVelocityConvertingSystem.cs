using System.Runtime.InteropServices;
using DOTS.Components;
using FunctionalLibraries;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Systems.InitializationPhase
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimulationPhase.VelocityDrivenMovementSystem))]
    [DisableAutoCreation]
    public partial struct CylinderVelocityConvertingSystem : ISystem
    {
        [BurstCompile]
        [StructLayout(LayoutKind.Auto)]
        private partial struct CylinderVelocityConvertingJob : IJobEntity
        {
            private void Execute(RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent,
                RefRO<FlowFieldVelocityComponent> flowFieldVelocityComponent, RefRW<VelocityComponent> velocityComponent)
            {
                /*velocityComponent.ValueRW.velocity = CylinderCalculations.ConvertToCylinderVelocity(
                    flowFieldVelocityComponent.ValueRO.flowVelocity, cylinderSurfacePositioningComponent.ValueRO.orbitCoordinate.angle);*/
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CylinderSurfacePositioningComponent>();
            state.RequireForUpdate<FlowFieldVelocityComponent>();
            state.RequireForUpdate<VelocityComponent>();
            _cylinderVelocityConvertingJob = new CylinderVelocityConvertingJob();
            // _random = new Random(214234);
        }

        // private Random _random;

        private CylinderVelocityConvertingJob _cylinderVelocityConvertingJob;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = _cylinderVelocityConvertingJob.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        // private static readonly float2 Float2One = new(1, 1);

        /*private float2 GetRandomVelocity()
        {
            return _random.NextFloat2(float2.zero, Float2One);
        }*/
    }
}