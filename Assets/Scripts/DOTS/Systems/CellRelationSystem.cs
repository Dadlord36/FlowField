using DOTS.Components;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(CylinderSurfacePositioningSystem))]
    public partial struct CellRelationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CellIndexComponent>();
            state.RequireForUpdate<GridParametersComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref readonly GridParameters gridParameters = ref SystemAPI.GetSingletonRW<GridParametersComponent>().ValueRO.gridParameters;
            ref readonly CylinderParameters cylinderParameters =
                ref SystemAPI.GetSingletonRW<CylinderParametersComponent>().ValueRO.cylinderParameters;

            foreach (var (cellIndexComponent, surfacePositioningComponent) in SystemAPI
                         .Query<RefRW<CellIndexComponent>, RefRO<CylinderSurfacePositioningComponent>>())
            {
                ref readonly CylinderSurfacePositioningComponent surfacePositioning = ref surfacePositioningComponent.ValueRO;
                cellIndexComponent.ValueRW.index = CylinderCalculations.GetCellIndex2DAt(gridParameters, cylinderParameters,
                    surfacePositioning.height, surfacePositioning.angle);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}