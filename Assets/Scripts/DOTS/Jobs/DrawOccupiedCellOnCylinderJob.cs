using System.Runtime.InteropServices;
using DOTS.Components;
using Drawing;
using FunctionalLibraries;
using Structs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Jobs
{
    [BurstCompile]
    [StructLayout(LayoutKind.Auto)]
    public partial struct DrawOccupiedCellOnCylinderJob : IJobEntity
    {
        private CommandBuilder drawingCommandBuilder;
        [ReadOnly] private readonly GridParameters gridParameters;
        [ReadOnly] private readonly CylinderParameters cylinderParameters;

        public DrawOccupiedCellOnCylinderJob(CommandBuilder drawingCommandBuilder, GridParameters gridParameters,
            CylinderParameters cylinderParameters) : this()
        {
            this.drawingCommandBuilder = drawingCommandBuilder;
            this.gridParameters = gridParameters;
            this.cylinderParameters = cylinderParameters;
        }

        private void Execute(RefRO<CellIndexComponent> cellIndexComponent)
        {
            if (cellIndexComponent.ValueRO.IsValid == false)
            {
                return;
            }
            LocalToWorld localToWorld =
                CylinderCalculations.CalculateLocalToWorldMatrixAt(gridParameters, cylinderParameters, cellIndexComponent.ValueRO.index);
            drawingCommandBuilder.SolidPlane(localToWorld.Position, localToWorld.Rotation, gridParameters.cellSize, Color.red);
        }
    }
}