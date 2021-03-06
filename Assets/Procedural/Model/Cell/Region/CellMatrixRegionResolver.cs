﻿using System.Collections.Generic;
using System.Linq;
using Util;

namespace Procedural.Model
{
    public class CellMatrixRegionResolver
    {
        public List<Region> Resolve(int cellValue)
        {
            InitializeVisited(cellMatrix);
            return cellMatrix
                .Where(cell => wasNotVisitedAndHasValue(cell, cellValue))
                .Aggregate(new List<Region>(), (regions, cell) => {
                    var region = GetContainerRegionOf(cell, cellValue);
                    region.ForEach(MakeVisited);                    
                    regions.Add(region);
                    return regions;
                });
        }

        private Region GetContainerRegionOf(Cell startCell, int cellValue)
        {
            var regionCells = new List<Cell>();
            var incomingCells = new Queue<Cell>();
            incomingCells.Enqueue(startCell);

            while (incomingCells.Count > 0)
            {
                var centralCell = incomingCells.Dequeue();
                regionCells.Add(centralCell);

                cellMatrix
                    .RadialForEach(centralCell, RADIO)
                    .Where(cell => cellMatrix.Contains(cell))
                    .WhereNot(cell => cellMatrix.IsEdge(cell))
                    .WhereNot(cell => cell.Equals(centralCell))
                    .Where(cell => wasNotVisitedAndHasValue(cell, cellValue))
                    .ForEach(cell => {
                        MakeVisited(cell);
                        incomingCells.Enqueue(cell);
                    });
            }
            return new Region(regionCells);
        }
        
        private void MakeVisited(Cell cell)
        {
            MakeVisited(cell.Coord.X, cell.Coord.Y);
        }

        private void MakeVisited(int x, int y)
        {
            visitedCells[x, y] = true;
        }

        private bool wasNotVisitedAndHasValue(Cell cell, int value)
        {
            return !visitedCells[cell.Coord.X, cell.Coord.Y] && cell.Value == value;
        }

        private void InitializeVisited(CellMatrix cellMatrix)
        {
            visitedCells = new bool[cellMatrix.Width, cellMatrix.Height];
        }
        
        private const int RADIO = 1;

        private bool[,] visitedCells;

        private readonly CellMatrix cellMatrix;
       
        public CellMatrixRegionResolver(CellMatrix cellMatrix)
        {
            this.cellMatrix = cellMatrix;
            InitializeVisited(cellMatrix);
        }
    }
}