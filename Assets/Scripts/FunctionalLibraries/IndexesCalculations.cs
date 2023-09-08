using Unity.Mathematics;

namespace FunctionalLibraries
{
    public static class IndexesCalculations
    {
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