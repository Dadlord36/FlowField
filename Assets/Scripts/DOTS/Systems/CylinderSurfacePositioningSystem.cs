using System.Runtime.InteropServices;
using DOTS.Components;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CylinderSurfacePositioningSystem : ISystem
    {
        [BurstCompile]
        [StructLayout(LayoutKind.Auto)]
        private partial struct CylinderSurfacePositionCalculationJob : IJobEntity
        {
            private readonly CylinderParameters _cylinderParameters;

            public CylinderSurfacePositionCalculationJob(in CylinderParameters cylinderParameters) : this()
            {
                _cylinderParameters = cylinderParameters;
            }

            private void Execute(RefRO<LocalToWorld> transform, RefRW<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent)
            {
                ref CylinderSurfacePositioningComponent cylinderSurfacePositioning = ref cylinderSurfacePositioningComponent.ValueRW;
                /*if (CylinderCalculations.IsOnCylinderSurface(_cylinderParameters, transform.ValueRO.Position) == false)
                {
                    return;
                }*/

                CylinderCalculations.GetHeightAndAngleOnCylinderAt(_cylinderParameters, transform.ValueRO.Position,
                    out cylinderSurfacePositioning.height, out cylinderSurfacePositioning.angle);
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
            var cylinderSurfacePositionCalculationJob =
                new CylinderSurfacePositionCalculationJob(SystemAPI.GetSingletonRW<CylinderParametersComponent>().ValueRO.cylinderParameters);
            cylinderSurfacePositionCalculationJob.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}