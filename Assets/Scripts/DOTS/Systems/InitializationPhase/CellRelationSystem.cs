using DOTS.Components;
using FunctionalLibraries;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Systems.InitializationPhase
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CellRelationSystem : ISystem
    {
        [BurstCompile]
        private partial struct CellRelationCalculationJob : IJobEntity
        {
            /* Original
             private void Execute(RefRW<CellIndexComponent> cellIndexComponent,
                RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent,
                in CylinderParametersComponent cylinderParametersComponent)
            {
                ref readonly CylinderSurfacePositioningComponent surfacePositioning = ref cylinderSurfacePositioningComponent.ValueRO;
                cellIndexComponent.ValueRW.index = CylinderCalculations.GetCellIndex2DOnCylinderSurfaceAt(gridParameters,
                    cylinderParametersComponent.cylinderParameters,
                    surfacePositioning.orbitCoordinate.height, surfacePositioning.orbitCoordinate.angle);
            }*/

            private void Execute(RefRW<CellIndexComponent> cellIndexComponent,
                RefRO<CylinderSurfacePositioningComponent> cylinderSurfacePositioningComponent,
                in CylinderParametersComponent cylinderParametersComponent, in GridParametersComponent gridParametersComponent)
            {
                cellIndexComponent.ValueRW.index = CylinderCalculations.CalculateCellIndexFromOrbitCoordinate(gridParametersComponent.gridParameters,
                    cylinderSurfacePositioningComponent.ValueRO.surfaceCoordinate);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CellIndexComponent>();
            state.RequireForUpdate<CylinderSurfacePositioningComponent>();
            state.RequireForUpdate<CylinderParametersComponent>();
            state.RequireForUpdate<GridParametersComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new CellRelationCalculationJob().ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}