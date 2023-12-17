using Unity.Mathematics;

namespace FunctionalLibraries
{
    public static class IndexesCalculations
    {
        public static uint2 CalculateIndex2DFrom1D(uint index1D, ushort cellsRowNumber)
        {
            return new uint2(index1D % cellsRowNumber, index1D / cellsRowNumber);
        }

        /// <summary>
        /// Calculate cell 1D index by cell 2D index so that the 0 index will be the last cell.
        /// </summary>
        /// <param name="index1D"> Cell 1D index. </param>
        /// <param name="cellsRowNumber"> Number of cells in row. </param>
        /// <returns> Cell 2D index. </returns>
        public static uint2 CalculateReversedIndex2DFrom1D(uint index1D, ushort cellsRowNumber)
        {
            uint2 cellIndex2D = CalculateIndex2DFrom1D(index1D, cellsRowNumber);

            return new uint2(cellsRowNumber - cellIndex2D.y - 1, cellsRowNumber - cellIndex2D.x - 1);
        }
    }
}