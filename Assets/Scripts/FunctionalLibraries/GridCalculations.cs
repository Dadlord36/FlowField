using Structs;
using Unity.Burst;
using Unity.Mathematics;

namespace FunctionalLibraries
{
    /// <summary>
    /// Grid related calculations.
    /// </summary>
    [BurstCompile]
    public static class GridCalculations
    {
        public static bool IsInGridBounds(in GridParameters gridParameters, in float3 position)
        {
            float2 gridLeftBottomCorner = gridParameters.gridCenter.xz - gridParameters.gridTotalSize / 2;
            float2 gridRightTopCorner = gridParameters.gridCenter.xz + gridParameters.gridTotalSize / 2;
            return position.x >= gridLeftBottomCorner.x && position.x <= gridRightTopCorner.x &&
                   position.z >= gridLeftBottomCorner.y && position.z <= gridRightTopCorner.y;
        }

        /// <summary>
        /// Calculate cell center position by cell 2D index.
        /// </summary>
        /// <param name="gridParameters"> Grid parameters. </param>
        /// <param name="x"> Cell X index. </param>
        /// <param name="y"> Cell Y index. </param>
        /// <returns> Cell center position. </returns>
        public static float3 GetCellCenterAt(in GridParameters gridParameters, int x, int y)
        {
            //Find grid left bottom corner position.
            float2 gridLeftBottomCorner = gridParameters.gridCenter.xz - gridParameters.gridTotalSize / 2;
            float2 cellCenter = gridLeftBottomCorner + new float2(x * gridParameters.cellSize.x, y * gridParameters.cellSize.y);
            //Offset cell center by half of cell size.
            cellCenter += gridParameters.cellSize / 2;
            return new float3(cellCenter.x, gridParameters.gridCenter.y, cellCenter.y);
        }

        /// <summary>
        /// Calculate cell 2D index from position on grid.
        /// </summary>
        /// <param name="gridParameters"> Grid parameters. </param>
        /// <param name="position"> Position on grid in world space. </param>
        /// <returns> Cell 2D index. </returns>
        public static int2 GetCellIndex2DAt(in GridParameters gridParameters, float3 position)
        {
            //Find grid left bottom corner position.
            float2 gridLeftBottomCorner = gridParameters.gridCenter.xz - gridParameters.gridTotalSize / 2;
            //Calculate cell index.
            var cellIndex = (int2)math.floor((position.xz - gridLeftBottomCorner) / gridParameters.cellSize);
            return cellIndex;
        }

        /// <summary>
        /// Calculate cell 1D index from position on grid.
        /// </summary>
        /// <param name="gridParameters"> Grid parameters. </param>
        /// <param name="position"> Position on grid in world space. </param>
        /// <returns> Cell 1D index. </returns>
        public static int GetCellIndex1DAt(in GridParameters gridParameters, float3 position)
        {
            int2 cellIndex = GetCellIndex2DAt(gridParameters, position);
            return cellIndex.x + cellIndex.y * gridParameters.cellsRowNumber;
        }

        /// <summary>
        /// Calculate cell center position by cell 1D index.
        /// </summary>
        /// <param name="gridParameters"> Grid parameters. </param>
        /// <param name="index"> Cell 1D index. </param>
        /// <returns> Cell center position. </returns>
        public static float3 GetCellCenterAt(in GridParameters gridParameters, int index)
        {
            int2 cellIndex2D = CalculateIndex2DFrom1D(index, gridParameters.cellsRowNumber);
            return GetCellCenterAt(gridParameters, cellIndex2D.x, cellIndex2D.y);
        }
        
        public static float3 GetCellCenterAtReversed(in GridParameters gridParameters, int index)
        {
            int2 cellIndex2D = CalculateReversedIndex2DFrom1D(index, gridParameters.cellsRowNumber);
            return GetCellCenterAt(gridParameters, cellIndex2D.x, cellIndex2D.y);
        }

        public static int2 CalculateIndex2DFrom1D(int index1D, int cellsRowNumber)
        {
            int x = index1D % cellsRowNumber;
            int y = index1D / cellsRowNumber;
            return new int2(x, y);
        }

        /// <summary>
        /// Calculate cell 1D index by cell 2D index so that the 0 index will be the last cell.
        /// </summary>
        /// <param name="index1D"> Cell 1D index. </param>
        /// <param name="cellsRowNumber"> Number of cells in row. </param>
        /// <returns> Cell 2D index. </returns>
        public static int2 CalculateReversedIndex2DFrom1D(int index1D, int cellsRowNumber)
        {
            int2 cellIndex2D = CalculateIndex2DFrom1D(index1D, cellsRowNumber);
            
            return new int2(cellsRowNumber - cellIndex2D.y - 1, cellsRowNumber - cellIndex2D.x - 1);
        }
    }
}