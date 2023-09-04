using DOTS.Components;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CellRelationSystem : ISystem
    {
        // private EntityQuery _cellRelatedQuery;
        // private GridParameters _gridParameters;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<CellIndexComponent>();
            state.RequireForUpdate<GridParametersComponent>();
            // _gridParameters = SystemAPI.GetSingletonRW<GridParametersComponent>().ValueRO.gridParameters;
            /*_cellRelatedQuery = state.GetEntityQuery(new NativeArray<ComponentType>(2, Allocator.Temp)
            {
                [0] = ComponentType.ReadWrite<CellIndexComponent>(),
                [1] = ComponentType.ReadOnly<LocalTransform>()
            });*/
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            GridParameters gridParameters = SystemAPI.GetSingletonRW<GridParametersComponent>().ValueRO.gridParameters;
            foreach (var (cellIndexComponent, localTransform) in SystemAPI.Query<RefRW<CellIndexComponent>, RefRO<LocalToWorld>>())
            {
                if (GridCalculations.IsInGridBounds(gridParameters, localTransform.ValueRO.Position) == false)
                {
                    cellIndexComponent.ValueRW.index = CellIndexComponent.Invalid.index;
                    return;
                }

                cellIndexComponent.ValueRW.index =
                    GridCalculations.GetCellIndex2DAt(gridParameters, localTransform.ValueRO.Position);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}