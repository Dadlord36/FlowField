using System.Runtime.InteropServices;
using DOTS.Components;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(CylinderSurfacePositioningSystem))]
    public partial struct CellRelationSystem : ISystem
    {
        [StructLayout(LayoutKind.Auto)]
        [BurstCompile]
        private partial struct CellRelationCalculationJob : IJobEntity
        {
            private readonly GridParameters gridParameters;
            // private readonly CylinderParameters cylinderParameters;

            public CellRelationCalculationJob(in GridParametersComponent gridParametersComponent /*,
                in CylinderParametersComponent cylinderParametersComponent*/) : this()
            {
                gridParameters = gridParametersComponent.gridParameters;
                // cylinderParameters = cylinderParametersComponent.cylinderParameters;
            }

            private void Execute(RefRW<CellIndexComponent> cellIndexComponent,
                RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent,
                in CylinderParametersComponent cylinderParametersComponent)
            {
                ref readonly CylinderSurfacePositioningComponent surfacePositioning = ref cylinderSurfacePositioningComponent.ValueRO;
                cellIndexComponent.ValueRW.index = CylinderCalculations.GetCellIndex2DOnCylinderSurfaceAt(gridParameters,
                    cylinderParametersComponent.cylinderParameters,
                    surfacePositioning.height, surfacePositioning.angle);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GridParametersComponent>();
            state.RequireForUpdate<CylinderParametersComponent>();
            state.RequireForUpdate<CellIndexComponent>();
            state.RequireForUpdate<CylinderSurfacePositioningComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new CellRelationCalculationJob(SystemAPI.GetSingletonRW<GridParametersComponent>().ValueRO)
                .ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}