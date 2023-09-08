﻿using Structs;
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
            int2 cellIndex2D = IndexesCalculations.CalculateIndex2DFrom1D(index, gridParameters.cellsRowNumber);
            return GetCellCenterAt(gridParameters, cellIndex2D.x, cellIndex2D.y);
        }
        
        public static float3 GetCellCenterAt(in GridParameters gridParameters, int2 cellIndex2D)
        {
            return GetCellCenterAt(gridParameters, cellIndex2D.x, cellIndex2D.y);
        }
        
        public static float3 GetCellCenterAtReversed(in GridParameters gridParameters, int index)
        {
            int2 cellIndex2D = IndexesCalculations.CalculateReversedIndex2DFrom1D(index, gridParameters.cellsRowNumber);
            return GetCellCenterAt(gridParameters, cellIndex2D.x, cellIndex2D.y);
        }
        

    }
}