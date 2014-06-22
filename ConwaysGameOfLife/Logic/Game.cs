using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConwaysGameOfLife.Model;

namespace ConwaysGameOfLife.Logic
{
    /**
     * Model of conway's game of life
     */
    class Game
    {
        /**
         * Start game
         */
        public IEnumerable<IEnumerable<Cell>> Start(IEnumerable<Cell> startGeneration)
        {
            while (true)
            {
                if (CreateTheNextGeneration(ref startGeneration))
                    yield break;
                yield return startGeneration;
            }
        }

        /**
         * Create a new generation based on the current
         */
        bool CreateTheNextGeneration(ref IEnumerable<Cell> currentGeneration)
        {
            HashSet<Cell> nextGeneration = new HashSet<Cell>(currentGeneration);
            foreach (var cell in currentGeneration)
            {
                var neighboringCells = GetOfNeighboringCells(cell);

                if (!CheckLiveCell(currentGeneration, cell, neighboringCells))
                    nextGeneration.Remove(cell);

                // subtraction of live cells
                neighboringCells.Except(currentGeneration);
                foreach (var neighboringCell in neighboringCells)
                {
                    var neighboringAdjacentCells = GetOfNeighboringCells(neighboringCell);
                    if (CheckDeadCell(currentGeneration, neighboringCell, neighboringAdjacentCells))
                        nextGeneration.Add(neighboringCell);
                }
            }
            bool isFinished = CheckFinishingGeneration(currentGeneration, nextGeneration);
            currentGeneration = nextGeneration;
            return isFinished;
        }

        /**
         * Check the end of the game
         * if a new generation died or changed then the end
         * @param currentGeneration
         * @param nextGeneration
         * @return if end then true
         */
        bool CheckFinishingGeneration(IEnumerable<Cell> currentGeneration, 
            IEnumerable<Cell> nextGeneration)
        {
            if (currentGeneration != null && nextGeneration != null)
                return (nextGeneration.Count() == 0 || currentGeneration.SequenceEqual(nextGeneration));
            else
                return true;
        }

        /**
         * Receipt of neighboring cells for the current cell
         * @return list of neighboring cells
         */
        IEnumerable<Cell> GetOfNeighboringCells(Cell cell)
        {
            long x = cell.X;
            long y = cell.Y;
            var neighboringCells = new HashSet<Cell> 
            {
                new Cell { X = x - 1, Y = y + 1}, // left, top
                new Cell { X = x - 1, Y = y},// left, center
                new Cell { X = x - 1, Y = y - 1},// left, bottom
                new Cell { X = x, Y = y + 1},// center, top
                new Cell { X = x, Y = y - 1},// center, bottom
                new Cell { X = x + 1, Y = y + 1},// right, top
                new Cell { X = x + 1, Y = y},// right, center
                new Cell { X = x + 1, Y = y - 1},// right, bottom
            };
            return neighboringCells;
        }

        /**
         * Verification of a living cell
         * if neighboring cells alive for at least two and no more than 3 then cell is alive
         * @param currentGeneration
         * @param cell
         * @param neighboringCells
         * @return if cells alive then true
         */
        bool CheckLiveCell(IEnumerable<Cell> currentGeneration, Cell cell, 
            IEnumerable<Cell> neighboringCells)
        {
            if (cell != null && neighboringCells != null)
            {
                int countIntersect = currentGeneration.Intersect(neighboringCells).Count();
                return (countIntersect == 2 || countIntersect == 3);
            }
            else
                return false;
        }

        /**
         * verification of dead cells
         * if the number of adjacent living cells is 3, the cell alive
         * @param currentGeneration
         * @param cell
         * @param neighboringCells
         * @return if cells alive then true
         */
        bool CheckDeadCell(IEnumerable<Cell> currentGeneration, Cell cell, 
            IEnumerable<Cell> neighboringCells)
        {
            if (cell != null && neighboringCells != null)
            {
                int countIntersect = currentGeneration.Intersect(neighboringCells).Count();
                return (countIntersect == 3);
            }
            else
                return false;
        }
    }
}
