using DOTS.Components;
using Drawing;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.Systems.DebugSystems
{
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    [DisableAutoCreation]
    public partial struct DebugDrawGridSystem : ISystem
    {
        [BurstCompile]
        private struct DrawGridJob : IJob
        {
            public float3 gridCenter;
            public int cellsNumber;
            public float2 gridTotalSize;

            public CommandBuilder drawingCommandBuilder;

            public void Execute()
            {
                drawingCommandBuilder.WireGrid(gridCenter, quaternion.identity, cellsNumber, gridTotalSize);
            }
        }

        [BurstCompile]
        private struct DrawFlowVectorsJob : IJobParallelFor
        {
            [ReadOnly] public CommandBuilder drawingCommandBuilder;
            [ReadOnly] public NativeArray2D<float2> flowMap;
            [ReadOnly] public GridParameters gridParameters;

            void IJobParallelFor.Execute(int index)
            {
                float3 cellCenter = GridCalculations.GetCellCenterAt(gridParameters, (uint)index);
                var flowVector = new float3(flowMap[index].x, 0, flowMap[index].y);
                drawingCommandBuilder.Arrow(cellCenter, cellCenter + flowVector, Color.red);
            }
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GridParametersComponent>();
            state.RequireForUpdate<FlowMapComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            CommandBuilder drawingBuilder = DrawingManager.GetBuilder(true);
            var flowMapComponent = SystemAPI.GetSingleton<FlowMapComponent>();
            var gridParametersComponent = SystemAPI.GetSingleton<GridParametersComponent>();
            ref GridParameters gridParameters = ref gridParametersComponent.gridParameters;

            JobHandle drawGridJob = new DrawGridJob
            {
                gridCenter = gridParameters.gridCenter,
                cellsNumber = gridParameters.rowNumber,
                gridTotalSize = gridParameters.gridSize,
                drawingCommandBuilder = drawingBuilder
            }.Schedule();

            JobHandle drawFlowVectorsJob = new DrawFlowVectorsJob
            {
                drawingCommandBuilder = drawingBuilder,
                gridParameters = gridParameters,
                flowMap = flowMapComponent.flowMap
            }.Schedule(gridParameters.totalCellsNumber, gridParameters.totalCellsNumber / 10, drawGridJob);
            drawingBuilder.DisposeAfter(drawFlowVectorsJob);
          
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}