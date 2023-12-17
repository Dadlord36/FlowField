using DOTS.Components;
using DOTS.Jobs;
using Drawing;
using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;

namespace DOTS.Systems.DebugSystems
{
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup)), UpdateAfter(typeof(DebugDrawGridSystem))]
    [DisableAutoCreation]
    public partial struct DebugDrawOccupiedGridCellsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GridParametersComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            CommandBuilder drawingBuilder = DrawingManager.GetBuilder(true);
            var gridParametersComponent = SystemAPI.GetSingleton<GridParametersComponent>();
            ref GridParameters gridParameters = ref gridParametersComponent.gridParameters;
            JobHandle drawJob = new DebugDrawOccupiedGridCellJob(drawingBuilder, gridParameters).ScheduleParallel(state.Dependency);
            drawingBuilder.DisposeAfter(drawJob);
           
            drawJob.Complete();
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}