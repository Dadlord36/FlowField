using Unity.Burst;
using Unity.Mathematics;

namespace Structs
{
    /// <summary>
    /// Represents the parameters for a grid, containing information such as the number of columns and rows, grid and cell sizes, and the grid center.
    /// </summary>
    [BurstCompile]
    public readonly struct GridParameters
    {
        public static readonly int2 InvalidIndex2D = new(-1, -1);

        public readonly ushort columnNumber;
        public readonly ushort rowNumber;
        public readonly ushort maxColumnIndex;
        public readonly ushort maxRowIndex;

        public readonly float2 gridSize;
        public readonly float2 cellSize;

        public readonly ushort totalCellsNumber;

        public GridParameters(ushort columnNumber, ushort rowNumber, float2 cellSize) : this()
        {
            this.columnNumber = maxColumnIndex = columnNumber;
            this.rowNumber = maxRowIndex = rowNumber;
            this.cellSize = cellSize;
            
            --maxColumnIndex;
            --maxRowIndex;

            gridSize = CalculateGridSize(columnNumber, rowNumber, cellSize);
            totalCellsNumber = (ushort)(columnNumber * rowNumber);
        }

        /// <summary>
        /// Calculates the total size of the grid based on the number of columns, rows, and the size of each cell.
        /// </summary>
        /// <param name="columnNumber">The number of columns in the grid.</param>
        /// <param name="rowNumber">The number of rows in the grid.</param>
        /// <param name="cellSize">The size of each cell in the grid.</param>
        /// <returns>The total size of the grid.</returns>
        private static float2 CalculateGridSize(ushort columnNumber, ushort rowNumber, float2 cellSize)
        {
            return new float2(columnNumber * cellSize.x, rowNumber * cellSize.y);
        }
    }
}