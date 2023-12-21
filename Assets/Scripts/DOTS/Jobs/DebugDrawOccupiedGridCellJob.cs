using System.Runtime.InteropServices;
using DOTS.Components;
using Drawing;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Jobs
{
    [BurstCompile]
    [StructLayout(LayoutKind.Auto)]
    public partial struct DebugDrawOccupiedGridCellJob : IJobEntity
    {
        private CommandBuilder drawingCommandBuilder;
        [ReadOnly] private readonly GridParameters gridParameters;

        public DebugDrawOccupiedGridCellJob(in CommandBuilder drawingCommandBuilder, in GridParameters gridParameters) : this()
        {
            this.drawingCommandBuilder = drawingCommandBuilder;
            this.gridParameters = gridParameters;
        }

        private void Execute(RefRO<CellIndexComponent> cellIndexComponent)
        {
            /*float3 cellCenter = GridCalculations.GetCellCenterAt(gridParameters, cellIndexComponent.ValueRO.index);
            drawingCommandBuilder.SolidPlane(cellCenter, quaternion.identity, gridParameters.cellSize, Color.red);*/
        }
    }
}