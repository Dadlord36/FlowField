using Unity.Burst;
using Unity.Mathematics;

namespace Structs
{
    [BurstCompile]
    public readonly struct GridParameters
    {
        public readonly float3 gridCenter;
        public readonly int cellsRowNumber;
        public readonly float2 gridTotalSize;

        public readonly float2 cellSize;
        public readonly int totalCellsNumber;

        public GridParameters(float3 gridCenter, int cellsRowNumber, float2 gridTotalSize)
        {
            this.gridCenter = gridCenter;
            this.cellsRowNumber = cellsRowNumber;
            this.gridTotalSize = gridTotalSize;
            totalCellsNumber = cellsRowNumber * cellsRowNumber;
            cellSize = gridTotalSize / cellsRowNumber;
        }
    }
}