using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(CellRelationSystem))]
    public partial struct FlowFieldMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlowDrivenTag>();
            state.RequireForUpdate<FlowFieldVelocityComponent>();
            state.RequireForUpdate<CellIndexComponent>();
            state.RequireForUpdate<FlowMapComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            RefRW<FlowMapComponent> flowMapComponent = SystemAPI.GetSingletonRW<FlowMapComponent>();
            // RefRW<GridParametersComponent> gridParametersComponent = SystemAPI.GetSingletonRW<GridParametersComponent>();

            foreach (var (cellIndexComponent, flowFieldVelocity) in SystemAPI.Query<RefRO<CellIndexComponent>, RefRW<FlowFieldVelocityComponent>>())
            {
                if (!cellIndexComponent.ValueRO.IsValid)
                {
                    continue;
                }

                int2 cellIndex = cellIndexComponent.ValueRO.index;
                flowFieldVelocity.ValueRW.flowVelocity = flowMapComponent.ValueRO.flowMap[cellIndex.x, cellIndex.y];
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}