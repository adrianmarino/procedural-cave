﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;

namespace Procedural.Model {
    public class CellMatrix : IEnumerable<Cell>, ITwoDimensionIndexable<Cell> {

        public CellMatrix MakeRegionPassages() {
            Passages.Clear();
            var floorRegions = RegionsBy(CellValue.Floor).OrderBy(region => region.Count);
            if (floorRegions.Count() == 1) return this;

            foreach (var regionA in floorRegions){
                RegionPassage bestPassage = null;
                var bestDistance = 0f;

                foreach (var regionB in floorRegions.WhereNot(regionA.Equals).WhereNot(regionA.Reach)){
                    var edgeCellsA = regionA.EdgeCells.ToArray();
                    var edgeCellsB = regionB.EdgeCells.ToArray();

                    for (var a = 0; a < edgeCellsA.Length; a = a+3) {
                        for (var b = 0; b < edgeCellsB.Length; b=b+3){
                            var cellA = edgeCellsA[a];
                            var cellB = edgeCellsB[b];

                            var distance = cellA.Distance(cellB);
                            if (!(distance < bestDistance) && bestPassage != null) continue;

                            bestDistance = distance;
                            bestPassage = new RegionPassage(regionA, regionB, cellA, cellB);
                        }
                    }
                }
                if (bestPassage != null)
                    Passages.Add(bestPassage);
            }

            Debug.LogFormat("Regions: {0} - Passages: {1}", floorRegions.Count(), Passages.Count);
            return this;
        }
        
        public CellMatrix MakeBorders() {
            this.Where(IsEdge).ForEach(cell => cell.MakeWall());
            return this;
        }

        public CellMatrix Fill(IFillStrategy fillStrategy) {
            this.ForEach(cell => cell.Value = IsEdge(cell) ? CellValue.Wall : fillStrategy.Next());
            return this;
        }

        public CellMatrix RemoveWallRegionLessThan(int cellSize) {
            SwapRegionValues(CellValue.Wall, CellValue.Floor, it => it.Count < cellSize);
            return this;
        }

        public CellMatrix RemoveCaveRegionLessThan(int cellSize) {
            SwapRegionValues(CellValue.Floor, CellValue.Wall,it => it.Count < cellSize);
            return this;
        }

        public CellMatrix Smooth(int steps, int maxSurroundWalls, int wallsSearchRadio) {
            for (var step = 0; step < steps; step++)
                this.WhereNot(IsEdge).ForEach(cell => {
                    var surroundWalls = SurroundWallsCount(cell, wallsSearchRadio);

                    if (surroundWalls > maxSurroundWalls) cell.MakeWall();
                    else if (surroundWalls < maxSurroundWalls) cell.MakeFloor();
                });
            return this;
        }

        private void SwapRegionValues(CellValue previousValue, CellValue laterValue, Func<Region, bool> where) {
            RegionsBy(previousValue).Where(where).ResetValues(laterValue);
        }

        public IEnumerable<Region> RegionsBy(CellValue value) {
            return new CellMatrixRegionResolver(this).Resolve(value);
        }

        public IEnumerable<Cell> NeighbourOf(Cell centralCell, int radio) {
            return this
                .RadialForEach(centralCell, radio)
                .Where(Contains)
                .WhereNot(Equals);
        }

        private int SurroundWallsCount(Cell centralCell, int radio) {
            return NeighbourOf(centralCell, radio).Select(Cell.IntValue).Sum();
        }
        
        public bool Contains(Cell cell) {
            var coord = cell.Coord;
            return coord.X >= 0 && coord.X < Width && coord.Y >= 0 && coord.Y < Height;
        }

        public bool IsEdge(Cell cell) {
            var coord = cell.Coord;
            return coord.X <= borderSize || coord.Y <= borderSize || coord.X >= Width - borderSize ||
                   coord.Y >= Height - borderSize;
        }

        #region Properties

        public IList<RegionPassage> Passages { get; private set; }

        private CellValue[,] Positions {
            get { return map ?? (map = new CellValue[width, height]); }
        }

        public Vector3 BottomLeft {
            get { return new Vector3(-Width / 2, 0, -Height / 2); }
        }

        public int Width {
            get { return width; }
        }

        public int Height {
            get { return height; }
        }

        #endregion

        #region Enumeration

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator() {
            for (var x = 0; x < Width; x++){
                for (var y = 0; y < Height; y++){
                    yield return new Cell(this, new Coord(x, y));
                }
            }
        }

        public Cell this[int x, int y] {
            get { return new Cell(this, new Coord(x, y)); }
        }

        #endregion

        #region Cell Value management

        internal CellValue Value(Coord coord) {
            return Positions[coord.X, coord.Y];
        }

        internal void Value(Coord coord, CellValue value) {
            Positions[coord.X, coord.Y] = value;
        }

        #endregion

        #region Constructors

        public CellMatrix(int width, int height, int borderSize) {
            this.width = width;
            this.height = height;
            this.borderSize = borderSize;
            Passages = new List<RegionPassage>();
        }

        #endregion

        #region Attributes

        private CellValue[,] map;

        private readonly int width;

        private readonly int height;

        private readonly int borderSize;

        #endregion
    }
}
