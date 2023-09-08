using System.Runtime.InteropServices;
using DOTS.Components;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(VelocityDrivenMovementSystem))]
    public partial struct CylinderSurfaceFacingSystem : ISystem
    {
        [BurstCompile]
        [StructLayout(LayoutKind.Auto)]
        private partial struct FaceCylinderSurfaceJob : IJobEntity
        {
            private readonly CylinderParameters _cylinderParameters;

            public FaceCylinderSurfaceJob(CylinderParameters cylinderParameters) : this()
            {
                _cylinderParameters = cylinderParameters;
            }

            private void Execute(RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent, RefRW<LocalToWorld> localToWorld)
            {
                ref readonly CylinderSurfacePositioningComponent cylinderSurfacePositioning = ref cylinderSurfacePositioningComponent.ValueRO;
                localToWorld.ValueRW = ComputeTransformOnCylinderSurface(cylinderSurfacePositioning.height, cylinderSurfacePositioning.angle);
            }

            private LocalToWorld ComputeTransformOnCylinderSurface(float height, float angle)
            {
                return CylinderCalculations.CalculateLocalToWorldOnCylinderSurfacePerpendicularToIt(_cylinderParameters, height, angle);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<CylinderSurfacePositioningComponent>();
            state.RequireForUpdate<CylinderParametersComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var faceCylinderSurfaceJob =
                new FaceCylinderSurfaceJob(SystemAPI.GetSingletonRW<CylinderParametersComponent>().ValueRO.cylinderParameters);
            faceCylinderSurfaceJob.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}