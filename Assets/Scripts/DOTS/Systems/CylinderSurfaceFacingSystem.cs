using System.Runtime.InteropServices;
using DOTS.Components;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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
            private void Execute(RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent, RefRW<LocalToWorld> localToWorld,
                RefRO<VelocityComponent> velocityComponent, in CylinderParametersComponent cylinderParametersComponent)
            {
                ref readonly CylinderSurfacePositioningComponent cylinderSurfacePositioning = ref cylinderSurfacePositioningComponent.ValueRO;
                localToWorld.ValueRW = ComputeTransformOnCylinderSurface(cylinderParametersComponent.cylinderParameters,cylinderSurfacePositioning.height, cylinderSurfacePositioning.angle,
                    velocityComponent.ValueRO.velocity);
            }

            private static LocalToWorld ComputeTransformOnCylinderSurface(CylinderParameters cylinderParameters, float height, float angle,
                in float3 finalForwardVector)
            {
                return CylinderCalculations.CalculateLocalToWorldOnCylinderSurfacePerpendicularToIt(cylinderParameters, height, angle,
                    finalForwardVector);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<CylinderSurfacePositioningComponent>();
            state.RequireForUpdate<CylinderParametersComponent>();
            state.RequireForUpdate<VelocityComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FaceCylinderSurfaceJob().ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}