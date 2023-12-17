using System.Runtime.InteropServices;
using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(CellRelationSystem))]
    [UpdateBefore(typeof(CylinderVelocityConvertingSystem))]
    public partial struct FlowFieldMovementSystem : ISystem
    {
        [BurstCompile]
        [StructLayout(LayoutKind.Auto)]
        private partial struct FlowFieldMovementJob : IJobEntity
        {
            [ReadOnly] private readonly FlowMapComponent _flowMapComponent;

            public FlowFieldMovementJob(in FlowMapComponent flowMapComponent) : this()
            {
                _flowMapComponent = flowMapComponent;
            }

            private void Execute(RefRW<FlowFieldVelocityComponent> flowFieldVelocityComponent, RefRO<CellIndexComponent> cellIndexComponent)
            {
                flowFieldVelocityComponent.ValueRW.flowVelocity = _flowMapComponent.flowMap[cellIndexComponent.ValueRO.index];
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
            state.Dependency = new FlowFieldMovementJob(SystemAPI.GetSingletonRW<FlowMapComponent>().ValueRO).ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}