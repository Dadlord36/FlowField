using DOTS.Components;
using DOTS.Components.Tags;
using Drawing;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Systems.DebugSystems
{
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    public partial struct DebugDrawForwardArrowSystem : ISystem
    {
        [BurstCompile]
        private partial struct DrawArrowsJob : IJobEntity
        {
            private CommandBuilder _forwardArrowsBuilder;
            private CommandBuilder _velocityArrowsBuilder;

            public DrawArrowsJob(CommandBuilder forwardArrowsBuilder, CommandBuilder velocityArrowsBuilder) : this()
            {
                _forwardArrowsBuilder = forwardArrowsBuilder;
                _velocityArrowsBuilder = velocityArrowsBuilder;
            }

            private void Execute(RefRO<LocalToWorld> currentTransform, RefRO<VelocityComponent> velocityComponent)
            {
                float3 forwardArrowTipPosition = currentTransform.ValueRO.Position + currentTransform.ValueRO.Forward;
                _forwardArrowsBuilder.Arrow(currentTransform.ValueRO.Position, forwardArrowTipPosition, Color.blue);

                float3 velocityArrowTipPosition = currentTransform.ValueRO.Position + velocityComponent.ValueRO.velocity;
                _velocityArrowsBuilder.Arrow(currentTransform.ValueRO.Position, velocityArrowTipPosition, Color.red);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VelocityDrivenTag>();
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<VelocityComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            CommandBuilder forwardArrowsBuilder = DrawingManager.GetBuilder(true);
            CommandBuilder velocityArrowsBuilder = DrawingManager.GetBuilder(true);

            state.Dependency = new DrawArrowsJob(forwardArrowsBuilder, velocityArrowsBuilder).ScheduleParallel(state.Dependency);

            forwardArrowsBuilder.DisposeAfter(state.Dependency);
            velocityArrowsBuilder.DisposeAfter(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}