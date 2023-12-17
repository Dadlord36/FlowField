using System.Runtime.InteropServices;
using DOTS.Components;
using FunctionalLibraries;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [StructLayout(LayoutKind.Auto)]
    [DisableAutoCreation]
    public partial struct CylinderSurfacePositioningSystem : ISystem
    {
        private CylinderSurfacePositionCalculationJob cylinderSurfacePositionCalculationJob;

        [BurstCompile]
        [StructLayout(LayoutKind.Auto)]
        private partial struct CylinderSurfacePositionCalculationJob : IJobEntity
        {
            private void Execute(in CylinderParametersComponent cylinderParametersComponent, RefRO<LocalToWorld> transform,
                RefRW<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent)
            {
                CylinderCalculations.GetHeightAndAngleOnCylinderAt(cylinderParametersComponent.cylinderParameters, transform.ValueRO.Position,
                    out cylinderSurfacePositioningComponent.ValueRW.height, out cylinderSurfacePositioningComponent.ValueRW.angle);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<CylinderSurfacePositioningComponent>();
            state.RequireForUpdate<CylinderParametersComponent>();

            cylinderSurfacePositionCalculationJob = new CylinderSurfacePositionCalculationJob();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = cylinderSurfacePositionCalculationJob.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}