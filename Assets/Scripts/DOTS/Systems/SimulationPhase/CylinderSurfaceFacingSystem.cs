using DOTS.Components;
using FunctionalLibraries;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Systems.SimulationPhase
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(VelocityDrivenMovementSystem))]
    [DisableAutoCreation]
    public partial struct CylinderSurfaceFacingSystem : ISystem
    {
        [BurstCompile]
        private partial struct FaceCylinderSurfaceJob : IJobEntity
        {
            private void Execute(RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent, RefRW<LocalToWorld> localToWorld,
                RefRO<VelocityComponent> velocityComponent, in CylinderParametersComponent cylinderParametersComponent)
            {
                /*ref readonly CylinderSurfacePositioningComponent cylinderSurfacePositioning = ref cylinderSurfacePositioningComponent.ValueRO;
                localToWorld.ValueRW = CylinderCalculations.CalculateLocalToWorldOnCylinderSurfacePerpendicularToIt(
                    cylinderParametersComponent.cylinderParameters, cylinderSurfacePositioning.orbitCoordinate.height,
                    cylinderSurfacePositioning.orbitCoordinate.angle,
                    velocityComponent.ValueRO.velocity);*/
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