using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

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

            foreach (var (cellIndexComponent, flowFieldVelocity)
                     in SystemAPI.Query<RefRO<CellIndexComponent>, RefRW<FlowFieldVelocityComponent>>())
            {
                flowFieldVelocity.ValueRW.flowVelocity = cellIndexComponent.ValueRO.IsValid
                    ? flowMapComponent.ValueRO.flowMap[cellIndexComponent.ValueRO.index]
                    : float2.zero;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}