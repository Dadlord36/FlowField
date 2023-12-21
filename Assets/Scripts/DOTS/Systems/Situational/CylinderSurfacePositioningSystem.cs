using System.Runtime.InteropServices;
using DOTS.Components;
using DOTS.Components.Tags;
using FunctionalLibraries;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Systems.Situational
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [StructLayout(LayoutKind.Auto)]
    [DisableAutoCreation]
    public partial struct CylinderSurfacePositioningSystem : ISystem
    {
        private CylinderSurfacePositionCalculationJob cylinderSurfacePositionCalculationJob;

        [BurstCompile]
        [StructLayout(LayoutKind.Auto)]
        [WithAll(typeof(SurfaceWalkerTag))]
        private partial struct CylinderSurfacePositionCalculationJob : IJobEntity
        {
            private void Execute(in CylinderParametersComponent cylinderParametersComponent, RefRO<LocalToWorld> transform,
                RefRW<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent)
            {
                /*ref OrbitCoordinate orbitCoordinate = ref cylinderSurfacePositioningComponent.ValueRW.orbitCoordinate;
                CylinderCalculations.GetHeightAndAngleOnCylinderAt(cylinderParametersComponent.cylinderParameters, transform.ValueRO.Position,
                    ref orbitCoordinate);*/
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