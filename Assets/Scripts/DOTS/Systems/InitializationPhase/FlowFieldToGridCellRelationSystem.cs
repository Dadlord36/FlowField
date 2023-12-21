using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Systems.InitializationPhase
{
    // [DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(CellRelationSystem))]
    public partial struct FlowFieldToGridCellRelationSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(FlowDrivenTag))]
        private partial struct FlowFieldToGridCellRelation : IJobEntity
        {
            private void Execute(RefRW<FlowFieldVelocityComponent> flowFieldVelocityComponent, RefRO<CellIndexComponent> cellIndexComponent,
                in FlowMapComponent flowMapComponent)
            {
                flowFieldVelocityComponent.ValueRW.flowVelocity = flowMapComponent.flowFieldAsset.Value.flowMap[cellIndexComponent.ValueRO.index];
            }
        }

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
            state.Dependency = new FlowFieldToGridCellRelation().ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}