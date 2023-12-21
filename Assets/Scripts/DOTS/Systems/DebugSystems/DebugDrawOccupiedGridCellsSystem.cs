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
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}